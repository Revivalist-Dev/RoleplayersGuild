import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';
import { fileURLToPath } from 'url';
import { copyAssets } from './vite-plugins/copy-assets';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default defineConfig(({ command }) => {
    return {
        resolve: {
            alias: {
                'bootstrap': path.resolve(__dirname, 'node_modules/bootstrap'),
                '@': path.resolve(__dirname, '../Site.Assets'),
                'jquery': path.resolve(__dirname, 'node_modules/jquery/dist/jquery.js'),
            },
        },
        plugins: [react(), copyAssets()],
        base: command === 'build' ? '/Assets/' : '/vite-dev/',
        build: {
            outDir: 'dist',
            emptyOutDir: true,
            manifest: 'manifest.json',
            sourcemap: true,
            rollupOptions: {
                input: {
                    main: path.resolve(__dirname, 'src/main.tsx'),
                    vendor: path.resolve(__dirname, 'src/vendor.ts'),
                    heartbeat: path.resolve(__dirname, 'src/Site.Scripts/user-activity-heartbeat.ts'),
                },
                output: {
                    entryFileNames: 'assets/[name]-[hash].js',
                    chunkFileNames: 'assets/[name]-[hash].js',
                    assetFileNames: 'assets/[name]-[hash][extname]',
                },
            },
        },
        server: {
            port: parseInt(process.env.VITE_PORT || '5173', 10),
            strictPort: true,
            hmr: {
                host: 'localhost',
                clientPort: parseInt(process.env.HTTP_PORT || '8080', 10),
            },
            fs: {
                allow: [path.resolve(__dirname, '../')],
            },
            allowedHosts: ['nginx'],
            // --- Docker Compatibility ---
            // The watch option is crucial for hot-reloading to work correctly
            // when running the Vite dev server inside a Docker container.
            watch: {
                // usePolling forces Vite to actively check for file changes
                // at a regular interval, as the default file system event-based
                // watching does not work reliably across the host-container boundary.
                usePolling: true,
                // interval specifies how often (in milliseconds) Vite should
                // check for file changes. A lower value provides faster
                // hot-reloading at the cost of slightly higher CPU usage.
                interval: 300,
            },
        },
        css: {
            preprocessorOptions: {
                scss: {
                    loadPaths: [
                        path.resolve(__dirname, '../Site.Assets/styles/scss'),
                        path.resolve(__dirname, 'node_modules')
                    ],
                    quietDeps: true,
                },
            },
        },
    };
});
