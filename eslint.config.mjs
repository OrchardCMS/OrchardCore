import globals from "globals";
import pluginJs from "@eslint/js";
import tseslint from "typescript-eslint";
import pluginVue from "eslint-plugin-vue";

/** @type {import('eslint').Linter.Config[]} */
export default [
    { files: ["**/*.{js,mjs,cjs,ts,vue}"] },
    {
        ignores: [
            "**/node_modules/**",
            "**/wwwroot/**",
            "**/obj/**",
            "**/bin/**",
            "**/vendor/**",
            "**/**.min.js**",
            ".yarn/**/*",
            ".parcel-cache/**/*",
            "**/vite-env.d.ts",
            "**/eslintrc.cjs",
            "**/po-gtranslator.js",
            "**/OrchardCore.Tests.Functional/**",
            "**/OrchardCore.Themes**/",
            "**/OrchardCore.Modules**/",
            "gulpfile.js",
            "**/docs/assets/js/**",
        ],
    },
    { languageOptions: { globals: globals.browser } },
    pluginJs.configs.recommended,
    ...tseslint.configs.recommended,
    ...pluginVue.configs["flat/essential"],
    { files: ["**/*.vue"], languageOptions: { parserOptions: { parser: tseslint.parser } } },
];
