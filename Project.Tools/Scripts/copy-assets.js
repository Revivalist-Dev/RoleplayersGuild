console.log('--- EXECUTING THE MODERN V2 SCRIPT ---');
'use strict';

const fs = require('fs-extra');
const path = require('path');

// Configuration for the files to be copied
const sourceDir = 'node_modules';
const libTargetDir = 'wwwroot/lib';
const jsTargetDir = 'wwwroot/js';

const filesToCopy = [
    // jQuery and its validation plugins
    { source: 'jquery/dist/jquery.min.js', dest: 'jquery/dist', targetBase: libTargetDir },
    { source: 'jquery-validation/dist/jquery.validate.min.js', dest: 'jquery-validation/dist', targetBase: libTargetDir },
    { source: 'jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js', dest: 'jquery-validation-unobtrusive/dist', targetBase: libTargetDir },

    // Bootstrap Icons
    { source: 'bootstrap-icons/font/bootstrap-icons.min.css', dest: 'bootstrap-icons/font', targetBase: libTargetDir },
    { source: 'bootstrap-icons/font/fonts/bootstrap-icons.woff', dest: 'bootstrap-icons/font/fonts', targetBase: libTargetDir },
    { source: 'bootstrap-icons/font/fonts/bootstrap-icons.woff2', dest: 'bootstrap-icons/font/fonts', targetBase: libTargetDir },
    { source: 'bootstrap-icons/bootstrap-icons.svg', dest: 'bootstrap-icons', targetBase: libTargetDir },

    // Bootstrap's JavaScript
    { source: 'bootstrap/dist/js/bootstrap.bundle.min.js', dest: 'bootstrap/dist/js', targetBase: libTargetDir },

    // SignalR client-side library
    { source: '@microsoft/signalr/dist/browser/signalr.min.js', destinationFile: 'signalr.min.js', targetBase: jsTargetDir },
    { source: '@microsoft/signalr/dist/browser/signalr.js', destinationFile: 'signalr.js', targetBase: jsTargetDir }
];

/**
 * Main async function to run the copy process.
 */
async function copyAssets() {
    console.log('Starting asset copy...');
    let copyCount = 0;
    let errorCount = 0;

    for (const file of filesToCopy) {
        const srcPath = path.join(sourceDir, file.source);
        const currentTargetBase = file.targetBase || libTargetDir;

        const finalDestPath = file.destinationFile
            ? path.join(currentTargetBase, file.destinationFile) // Use an explicit destination filename if provided
            : path.join(currentTargetBase, file.dest, path.basename(file.source)); // Maintain the directory structure from the source

        try {
            await fs.copy(srcPath, finalDestPath, { overwrite: true });
            console.log(`Copied ${file.source} to ${finalDestPath}`);
            copyCount++;
        } catch (err) {
            console.error(`Error copying ${file.source} to ${finalDestPath}:`, err.message);
            errorCount++;
        }
    }

    console.log(`Asset copying finished: ${copyCount} succeeded, ${errorCount} failed.`);
    if (errorCount > 0) {
        // Exit with an error code if any copy operations failed
        process.exit(1);
    }
}

// Execute the main function
copyAssets();