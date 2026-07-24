import { defineConfig } from 'vite'
import path from 'path'
import { fileURLToPath } from 'url'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

export default defineConfig({
    build: {
        outDir: path.resolve(__dirname, '../../wwwroot/Scripts/openapi-ui-auth/'),
        emptyOutDir: true,
        copyPublicDir: false,
        rollupOptions: {
            input: path.resolve(__dirname, 'src/openapi-ui-auth.ts'),
            output: {
                // Self-bootstrapping ES module injected as <script type="module"> into the
                // third-party Swagger and Scalar pages; configured via data-* attributes on
                // its own script tag, no globals.
                format: 'es',
                entryFileNames: 'openapi-ui-auth.js',
                assetFileNames: 'openapi-ui-auth.[ext]',
                // Single chunk - no code splitting for an injected script.
                manualChunks: undefined,
            },
        },
    },
})
