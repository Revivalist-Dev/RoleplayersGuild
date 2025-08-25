import { Plugin } from 'vite';
import path from 'path';
import fs from 'fs-extra';

export function copyAssets(): Plugin {
    return {
        name: 'copy-assets',
        async writeBundle(outputOptions) {
            const outDir = outputOptions.dir || 'dist';
            const assetSrc = path.resolve(__dirname, '../../Site.Assets');
            
            // Define source and destination for fonts
            const fontSrc = path.join(assetSrc, 'fonts');
            const fontDest = path.join(outDir, 'assets', 'fonts');
            await fs.copy(fontSrc, fontDest);

            // Define source and destination for images
            const imageSrc = path.join(assetSrc, 'images');
            const imageDest = path.join(outDir, 'assets', 'images');
            await fs.copy(imageSrc, imageDest);
        },
    };
}