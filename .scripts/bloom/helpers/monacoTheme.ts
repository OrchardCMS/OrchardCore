import type * as Monaco from "monaco-editor";

declare global {
    interface Window {
        __orchardCoreMonacoThemeSyncStarted?: boolean;
    }
}

const applyTheme = (monacoInstance: typeof Monaco) => {
    const theme = document.documentElement.dataset.bsTheme;
    const isDark = theme === "dark" || (theme === "auto" && window.matchMedia("(prefers-color-scheme: dark)").matches);

    monacoInstance.editor.setTheme(isDark ? "vs-dark" : "vs");
};

// Monaco's theme is a single global setting, not per-editor-instance, so one shared observer
// (anchored on `window` for the same reason as observeAndInit - each view importing this helper
// compiles into its own standalone Parcel bundle, so a module-scope flag wouldn't actually be
// shared across them) covers however many Monaco editors end up on one page, instead of each
// extracted view setting up its own redundant MutationObserver watching the same attribute.
const syncMonacoTheme = (monacoInstance: typeof Monaco) => {
    if (!window.__orchardCoreMonacoThemeSyncStarted) {
        window.__orchardCoreMonacoThemeSyncStarted = true;
        new MutationObserver(() => applyTheme(monacoInstance)).observe(document.documentElement, {
            attributes: true,
        });
    }

    applyTheme(monacoInstance);
};

export default syncMonacoTheme;
