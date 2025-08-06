# S3 Migration Script for RoleplayersGuild.com
# Version 2: Handles mixed data formats (old filenames and new structured paths)

# --- CONFIGURATION ---
$bucketName = "images-roleplayersguild-com"
$avatarManifestPath = "C:\migration\avatars_to_migrate.csv"
$cardManifestPath = "C:\migration\cards_to_migrate.csv"
# ---------------------

function Migrate-S3Objects {
    param(
        [string]$ManifestPath,
        [string]$OldFolder,
        [string]$NewFolderSuffix
    )

    if (-not (Test-Path $ManifestPath)) {
        Write-Host "Manifest file not found: $ManifestPath. Skipping." -ForegroundColor Yellow
        return
    }

    $items = Import-Csv $ManifestPath

    foreach ($item in $items) {
        $dbPath = $item.OldFilename # Path from the database
        $userId = $item.UserId
        $characterId = $item.CharacterId
        
        $sourceFilename = ""
        $destinationKey = ""

        # --- NEW LOGIC TO HANDLE MIXED PATHS ---
        # Check if the path from the DB is already in the new, structured format
        if ($dbPath -like '*\*' -or $dbPath -like '*/*') {
            # It's a new path. The source is still in the old flat folder.
            # The destination is the path we already have.
            $sourceFilename = [System.IO.Path]::GetFileName($dbPath)
            $destinationKey = "UserFiles/$($dbPath.Replace('\', '/'))"
        }
        else {
            # It's an old filename. Construct the new path.
            $sourceFilename = $dbPath
            $destinationKey = "UserFiles/$userId/$characterId/$NewFolderSuffix/$sourceFilename"
        }
        # --- END NEW LOGIC ---

        # Construct the final source S3 path
        $sourceKey = "$OldFolder/$sourceFilename"
        
        Write-Host "Migrating '$sourceFilename'..."
        Write-Host "  -> FROM: s3://$bucketName/$sourceKey"
        Write-Host "  -> TO:   s3://$bucketName/$destinationKey"

        # 1. Copy the file to the new, structured location
        aws s3 cp "s3://$bucketName/$sourceKey" "s3://$bucketName/$destinationKey" --quiet

        # 2. Verify the copy was successful before deleting the original
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  -> SUCCESS. Deleting original." -ForegroundColor Green
            aws s3 rm "s3://$bucketName/$sourceKey" --quiet
        } else {
            Write-Host "  -> FAILED for $sourceFilename. The original file has not been deleted." -ForegroundColor Red
        }
    }
}

Write-Host "--- Starting Avatar Migration ---"
Migrate-S3Objects -ManifestPath $avatarManifestPath -OldFolder "CharacterAvatars" -NewFolderSuffix "Avatars"

Write-Host "`n--- Starting Character Card Migration ---"
Migrate-S3Objects -ManifestPath $cardManifestPath -OldFolder "CharacterCards" -NewFolderSuffix "Cards"

Write-Host "`n--- Migration Complete ---"