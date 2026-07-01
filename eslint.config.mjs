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
            "**/Assets/*Theme/dist/**",
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
            // Vendored third-party code embedded directly rather than under a vendor/ folder:
            // the jQuery UI Nested Sortable plugin (MIT, github.com/ilikenwf/nestedSortable),
            // duplicated verbatim in both modules.
            "**/OrchardCore.Taxonomies/Assets/js/menu.js",
            "**/OrchardCore.Menu/Assets/js/menu.js",
            // Hand-authored ambient type declarations for the untyped third-party jsPlumb library
            // (not from DefinitelyTyped) - a typing compatibility shim, not application code.
            // Properly typing jsPlumb's full API is a separate, substantial effort.
            "**/OrchardCore.Workflows/Assets/Lib/jsplumb/typings.d.ts",
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
                L: "readonly",
                confirmDialog: "readonly",
                // Defined by the (currently lint-deferred) OrchardCore.Media module, consumed here.
                mediaApp: "readonly",
                // Defined by OrchardCore.Shortcodes, consumed here.
                shortcodesApp: "readonly",
            },
        },
    },
    {
        // Node-based build tooling scripts (pug/scss/webpack orchestration) for the themes that
        // ship their own asset pipeline, as opposed to browser-side theme scripts. Plain CommonJS,
        // not bundled, so require() is the correct import style here, not just a known global.
        files: ["**/Assets/*Theme/scripts/**/*.js"],
        languageOptions: { globals: globals.node },
        rules: { "@typescript-eslint/no-require-imports": "off" },
    },
];
