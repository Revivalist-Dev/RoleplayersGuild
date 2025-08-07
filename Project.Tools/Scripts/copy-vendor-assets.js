const fs = require('fs-extra');
const path = require('path');

// Define the root of the project
const projectRoot = path.resolve(__dirname, '..', '..');

// Define source and destination paths
const vendorDest = path.join(projectRoot, 'wwwroot', 'vendor');

const assetsToCopy = {
    'bootstrap': {
        src: path.join(projectRoot, 'node_modules', 'bootstrap', 'dist'),
        dest: path.join(vendorDest, 'bootstrap')
    },
    'bootstrap-icons': {
        src: path.join(projectRoot, 'node_modules', 'bootstrap-icons'),
        dest: path.join(vendorDest, 'bootstrap-icons')
    }
};

// Function to copy assets
async function copyAssets() {
    try {
        console.log('Starting to copy vendor assets...');

        for (const [name, paths] of Object.entries(assetsToCopy)) {
            if (await fs.exists(paths.src)) {
                await fs.ensureDir(paths.dest);
                await fs.copy(paths.src, paths.dest);
                console.log(`Successfully copied ${name} assets to ${paths.dest}`);
            } else {
                console.error(`Source directory not found for ${name}: ${paths.src}`);
                console.error('Please ensure you have run "npm install" or "yarn install" in the root directory.');
            }
        }

        console.log('Vendor assets copied successfully.');

    } catch (err) {
        console.error('Error copying vendor assets:', err);
        process.exit(1);
    }
}

// Run the copy function
copyAssets();
