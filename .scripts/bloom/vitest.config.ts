import path from "path";
import vue from "@vitejs/plugin-vue";
import { defineConfig } from "vitest/config";

export default defineConfig({
    plugins: [vue()],
    resolve: {
        alias: {
            "@bloom": import.meta.dirname,
        },
    },
    define: {
        "process.env.NODE_ENV": JSON.stringify("test"),
    },
    test: {
        environment: "jsdom",
        include: ["**/__tests__/**/*.spec.ts"],
        exclude: ["**/node_modules/**"],
        setupFiles: [path.resolve(import.meta.dirname, "vitest.setup.ts")],
        reporters: ["default", "junit"],
        outputFile: "./testing/vitest-results.xml",
    },
});
