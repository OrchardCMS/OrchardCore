import { defineConfig } from "vitest/config";

export default defineConfig({
    test: {
        environment: "jsdom",
        include: ["**/__tests__/**/*.spec.ts"],
        exclude: ["**/node_modules/**"],
        reporters: ["default", "junit"],
        outputFile: "./testing/vitest-results.xml",
    },
});
