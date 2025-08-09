[CmdletBinding()]
param ()

# --- Configuration Discovery ---
Clear-Host
Write-Host "Searching for .aiexclude file..." -ForegroundColor Cyan

$currentDir = $PSScriptRoot
$projectRoot = $null
$maxDepth = 10 # Prevents searching past the root of the drive
$depth = 0

# Traverse up from the script's directory to find the project root
while ($depth -lt $maxDepth -and $currentDir -and (Get-Item $currentDir).Parent) {
    $potentialIgnoreFile = Join-Path $currentDir ".aiexclude"
    if (Test-Path $potentialIgnoreFile) {
        $projectRoot = $currentDir
        Write-Host "Found project root at: $projectRoot" -ForegroundColor Green
        break
    }
    $currentDir = (Get-Item $currentDir).Parent.FullName
    $depth++
}

if (-not $projectRoot) {
    Write-Error "FATAL: Could not find an .aiexclude file in this directory or any parent directory up to $maxDepth levels. Aborting."
    Read-Host -Prompt "Press ENTER to exit"
    exit
}

# --- Ignore File Parsing ---
$ignoreFile = Join-Path $projectRoot ".aiexclude"
$fileContent = Get-Content $ignoreFile

# The scan path is always the project root where the .aiexclude file was found.
$scanPath = $projectRoot
Write-Host "Using project root as scan path: $scanPath" -ForegroundColor Yellow

# Get ignore patterns, filtering out comments and empty lines.
# Convert gitignore-style patterns to PowerShell -like wildcards.
$ignorePatterns = $fileContent | Where-Object { $_.Trim() -and $_.Trim() -notmatch '^#' } | ForEach-Object {
    $pattern = $_.Trim().Replace('**/', '*').TrimStart('/').Replace('/', '\')
    if ($pattern.EndsWith('\')) {
        $pattern = $pattern.TrimEnd('\') + '\*'
    }
    $pattern
}

# Resolve the final path to ensure it's a valid, absolute path
$rootPath = Resolve-Path -Path $scanPath

# --- Recursive Path Generation Function ---
function Get-DirectoryPaths {
    param (
        [string]$Directory,
        [array]$IgnoreList
    )
    # Get all items recursively
    $items = Get-ChildItem -Path $Directory -Recurse -Force
    
    foreach ($item in $items) {
        $isIgnored = $false
        # Generate a relative path from the root for comparison
        $relativePath = $item.FullName.Substring($rootPath.Path.Length).TrimStart('\/')
        
        foreach ($pattern in $IgnoreList) {
            # Check if any part of the relative path or just the name matches an ignore pattern
            if (($relativePath -like $pattern) -or ($item.Name -like $pattern)) {
                $isIgnored = $true
                break
            }
        }
        
        if (-not $isIgnored) {
            # Output the full path of the item
            $item.FullName
        }
    }
}


# --- Main Execution ---
Write-Host "--------------------------------------------------------"
Write-Host "Generating file paths for: $rootPath" -ForegroundColor Green
Write-Host "--------------------------------------------------------"

$pathList = Get-DirectoryPaths -Directory $rootPath -IgnoreList $ignorePatterns
$pathListString = $pathList -join [Environment]::NewLine
Set-Clipboard -Value $pathListString

Write-Host "`n--------------------------------------------------------"
Write-Host "SUCCESS: The file path list has been copied to your clipboard." -ForegroundColor Green
Write-Host "You can now paste it into Gemini or another application."
Write-Host "Total paths generated: $($pathList.Count)"
Write-Host "--------------------------------------------------------"

# Optional: Keep the window open
# Read-Host -Prompt "Press ENTER to exit"