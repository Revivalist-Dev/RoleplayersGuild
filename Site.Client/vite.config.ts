import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

export default defineConfig(({ command }) => {
    return {
        plugins: [react()],

        // The base path for assets will now be /react-dist/
        base: command === 'serve' ? '/vite-dev/' : '/react-dist/',

        build: {
            // Build directly into a 'react-dist' folder inside wwwroot
            outDir: '../wwwroot/react-dist',
            // Ensure the directory is empty before building
            emptyOutDir: true,
            // This change places manifest.json in the root of outDir, preventing the .vite folder
            manifest: 'manifest.json',
            rollupOptions: {
                input: 'src/main.tsx'
            }
        },
        server: {
            port: 5173,
            strictPort: true,
            origin: 'http://localhost:5173'
        }
    }
})