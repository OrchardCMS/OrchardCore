import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";
import path from "path";

// https://vitejs.dev/config/
export default defineConfig({
    resolve: {
        alias: {
            vue: "vue/dist/vue.esm-bundler.js",
            "@bloom": path.resolve(__dirname, "../../../../../../../.scripts/bloom"),
        },
    },
    plugins: [tailwindcss(), vue()],
    define: {
        "process.env.NODE_ENV": JSON.stringify("production"),
    },
    build: {
        outDir: path.resolve(__dirname, "../../../../wwwroot"),
        emptyOutDir: false,
        minify: false,
        lib: {
            entry: path.resolve(__dirname, "src/main.ts"),
            formats: ["es"],
            fileName: () => "Scripts/media-field2.js",
        },
        rollupOptions: {
            output: {
                assetFileNames: `Styles/media-field2.[ext]`,
            },
        },
    },
});
