import { defineConfig } from 'vite';
import path from 'path';
import fs from 'fs';

const wwwroot = path.resolve(__dirname, '../../../wwwroot');

// Plugin to move CSS output from Scripts/ to Styles/ after the minify plugin runs.
function moveCssToStyles(): import('vite').Plugin {
    return {
        name: 'move-css-to-styles',
        enforce: 'post',
        closeBundle() {
            const stylesDir = path.join(wwwroot, 'Styles');
            if (!fs.existsSync(stylesDir)) {
                fs.mkdirSync(stylesDir, { recursive: true });
            }

            const scriptsDir = path.join(wwwroot, 'Scripts');
            const cssFiles = ['phone-input.css', 'phone-input.min.css', 'phone-input.css.map'];

            for (const file of cssFiles) {
                const src = path.join(scriptsDir, file);
                if (fs.existsSync(src)) {
                    fs.renameSync(src, path.join(stylesDir, file));
                }
            }
        },
    };
}

export default defineConfig({
    define: {
        'process.env.NODE_ENV': JSON.stringify('production'),
    },
    plugins: [moveCssToStyles()],
    build: {
        outDir: path.join(wwwroot, 'Scripts'),
        emptyOutDir: false,
        lib: {
            entry: path.resolve(__dirname, 'main.ts'),
            fileName: () => 'phone-input.js',
            formats: ['es'],
        },
        rollupOptions: {
            output: {
                assetFileNames: 'phone-input.[ext]',
            },
        },
    },
});
