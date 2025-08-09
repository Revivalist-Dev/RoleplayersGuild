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
    },
    'packery': {
        src: path.join(projectRoot, 'Site.Client', 'node_modules', 'packery', 'dist'),
        dest: path.join(vendorDest, 'packery')
    },
    'imagesloaded': {
        src: path.join(projectRoot, 'Site.Client', 'node_modules', 'imagesloaded', 'imagesloaded.pkgd.min.js'),
        dest: path.join(vendorDest, 'imagesloaded', 'imagesloaded.pkgd.min.js')
    }
};

// Function to copy assets
async function copyAssets() {
    try {
        console.log('Starting to copy vendor assets...');

        for (const [name, paths] of Object.entries(assetsToCopy)) {
            if (await fs.exists(paths.src)) {
                // Check if the source is a file or directory
                const isFile = (await fs.stat(paths.src)).isFile();
                const destDir = isFile ? path.dirname(paths.dest) : paths.dest;
                
                await fs.ensureDir(destDir);
                await fs.copy(paths.src, paths.dest);
                console.log(`Successfully copied ${name} assets to ${paths.dest}`);
            } else {
                console.error(`Source path not found for ${name}: ${paths.src}`);
                console.error('Please ensure you have run "npm install" in the Site.Client directory.');
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
