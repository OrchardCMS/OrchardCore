import type * as Monaco from "monaco-editor";

// `import type` is erased at compile time - this adds zero runtime Monaco code to whichever
// Parcel bundle imports it. The actual `import * as monaco from "monaco-editor"` lives in exactly
// one place, OrchardCore.Resources/Assets/monaco/monaco-loader.ts, built and loaded once per page
// as the "monaco" resource (mirroring the old AMD loader's deployment model, just with a real ES
// import instead of require(['vs/editor/editor.main'], cb)). Every Monaco call site awaits this
// helper instead of racing script execution order against a bare global.
declare global {
    interface Window {
        __orchardCoreMonacoReady?: Promise<typeof Monaco>;
    }
}

const waitForMonaco = (): Promise<typeof Monaco> => {
    if (!window.__orchardCoreMonacoReady) {
        return Promise.reject(
            new Error(
                'Monaco is not loaded on this page. Add <script asp-name="monaco" at="Foot"></script> and depends-on="monaco" to the view that needs it.',
            ),
        );
    }

    return window.__orchardCoreMonacoReady;
};

export default waitForMonaco;
