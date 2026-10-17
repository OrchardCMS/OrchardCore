import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import postcssRTLCSS from "postcss-rtlcss";
import { Mode } from "postcss-rtlcss/options";
import path from "path";

export default defineConfig({
    resolve: {
        // The designer only uses precompiled single-file components, so it bundles Vue's runtime-only build.
        alias: {
            "@bloom": path.resolve(import.meta.dirname, "../../../../../.scripts/bloom"),
        },
    },
    plugins: [vue()],
    css: {
        postcss: {
            plugins: [postcssRTLCSS({ mode: Mode.combined }) as never],
        },
    },
    define: {
        "process.env.NODE_ENV": JSON.stringify("production"),
        __VUE_OPTIONS_API__: "false",
        __VUE_PROD_DEVTOOLS__: "false",
        __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: "false",
    },
    build: {
        outDir: path.resolve(import.meta.dirname, "../../wwwroot"),
        emptyOutDir: false,
        // Minification is handled by the asset-manager pipeline (vite-plugin-minify).
        minify: false,
        lib: {
            entry: path.resolve(import.meta.dirname, "src/main.ts"),
            formats: ["es"],
            fileName: () => "Scripts/DataPipelines/designer/data-pipelines-designer.js",
        },
        rollupOptions: {
            output: {
                assetFileNames: "Styles/data-pipelines-designer.[ext]",
            },
        },
    },
});
