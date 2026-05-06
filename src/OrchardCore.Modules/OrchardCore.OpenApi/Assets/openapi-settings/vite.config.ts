import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'
import { fileURLToPath } from 'url'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

export default defineConfig({
    plugins: [vue()],
    build: {
        outDir: path.resolve(__dirname, '../../wwwroot/Scripts/openapi-settings/'),
        emptyOutDir: true,
        copyPublicDir: false,
        rollupOptions: {
            input: path.resolve(__dirname, 'src/main.ts'),
            output: {
                entryFileNames: 'openapi-settings.js',
                assetFileNames: 'openapi-settings.[ext]',
                // Single chunk - no code splitting for admin script.
                manualChunks: undefined,
            },
        },
    },
    resolve: {
        alias: {
            '@bloom': path.resolve(__dirname, '../../../../../.scripts/bloom'),
        },
    },
})
