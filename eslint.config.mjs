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
            "**/docs/assets/js/**",
            // TODO: large, jQuery-heavy legacy codebases; deferred to a dedicated follow-up.
            "**/OrchardCore.Resources/**",
            "**/OrchardCore.Media/**",
        ],
    },
    { languageOptions: { globals: globals.browser } },
    pluginJs.configs.recommended,
    ...tseslint.configs.recommended,
    ...pluginVue.configs["flat/essential"],
    { files: ["**/*.vue"], languageOptions: { parserOptions: { parser: tseslint.parser } } },
    {
        // Legacy (pre-module) module/theme scripts that rely on libraries loaded as global
        // <script> tags rather than bundled imports. Scoped to .js only: the .ts sources that
        // have been migrated off jQuery must keep failing no-undef if $/jQuery reappear in them.
        files: ["**/OrchardCore.Modules/**/*.js", "**/OrchardCore.Themes/**/*.js"],
        languageOptions: {
            globals: {
                $: "readonly",
                jQuery: "readonly",
                define: "readonly",
                require: "readonly",
                Vue: "readonly",
                bootstrap: "readonly",
                EasyMDE: "readonly",
                Sortable: "readonly",
                CodeMirror: "readonly",
            },
        },
    },
    {
        // Node-based build tooling scripts (pug/scss/webpack orchestration) for the themes that
        // ship their own asset pipeline, as opposed to browser-side theme scripts.
        files: ["**/Assets/*Theme/scripts/**/*.js"],
        languageOptions: { globals: globals.node },
    },
];
