import { mergeConfig, defineConfig, configDefaults } from "vitest/config";
import viteConfig from "./vite.config.ts";

export default mergeConfig(
    viteConfig,
    defineConfig({
        define: {
            "process.env.NODE_ENV": JSON.stringify("test"),
        },
        test: {
            globals: true,
            environment: "jsdom",
            setupFiles: ["./vitest.setup.ts"],
            exclude: [...configDefaults.exclude],
            reporters: ["default", "junit"],
            outputFile: "./testing/vitest-results.xml",
        },
    }),
);
