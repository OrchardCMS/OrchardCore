import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";
import postcssRTLCSS from "postcss-rtlcss";
import { Mode } from "postcss-rtlcss/options";
import path from "path";

export default defineConfig({
    resolve: {
        alias: {
            vue: "vue/dist/vue.esm-bundler.js",
            "@bloom": path.resolve(__dirname, "../../../../../.scripts/bloom"),
        },
    },
    plugins: [tailwindcss(), vue()],
    css: {
        postcss: {
            plugins: [postcssRTLCSS({ mode: Mode.combined }) as never],
        },
    },
    define: {
        "process.env.NODE_ENV": JSON.stringify("production"),
    },
    build: {
        outDir: path.resolve(__dirname, "../../wwwroot"),
        emptyOutDir: false,
        // Minification is handled by the asset-manager pipeline (vite-plugin-minify).
        minify: false,
        lib: {
            entry: path.resolve(__dirname, "src/main.ts"),
            formats: ["es"],
            fileName: () => "Scripts/media2.js",
        },
        rollupOptions: {
            output: {
                assetFileNames: "Styles/media2.[ext]",
            },
        },
    },
});
