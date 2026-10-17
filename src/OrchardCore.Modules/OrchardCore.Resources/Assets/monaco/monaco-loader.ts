import * as monaco from "monaco-editor";

// The only place in the codebase that imports monaco-editor's runtime - every other Monaco call
// site awaits window.__orchardCoreMonacoReady (via helpers/monaco.ts's waitForMonaco) instead of
// importing it themselves, so the whole admin UI shares exactly one Monaco module instance
// (and thus one theme, one set of registered languages) no matter how many separately-built
// Parcel bundles use it on a given page.

// Each worker is its own standalone Assets.json Parcel entry (see workers/{json,ts,html,editor}/
// below this script's own dist folder) instead of being discovered transitively from this file via
// `new Worker(new URL("./x.ts", import.meta.url))`. Parcel would otherwise bundle all 4 workers
// together in one build and extract their shared monaco-editor runtime code into a chunk loaded via
// `importScripts()`, which isn't available inside `{ type: "module" }` workers - and plain classic
// workers don't support it either once monaco-editor's own ESM source forces module semantics on
// that chunk. Building each worker standalone means there's no sibling bundle to share code with,
// so every worker is fully self-contained and can load as a plain classic worker.
// `relativePath` is deliberately taken as a parameter rather than inlined as a string literal
// argument to `new URL(...)`, so Parcel's static analyzer doesn't try to resolve and bundle it as a
// dependency (the target files aren't part of this build's source graph at all).
//
// This script's own build (see Assets.json's "monaco-esm" entry for monaco-loader.ts) uses Parcel's
// default non-scope-hoisted "global" output format, which has no real ES module runtime - Parcel
// statically replaces `import.meta.url` with the literal build-time source path (a `file://` URL on
// the machine that ran `yarn build`) instead of resolving it in the browser. `document.currentScript`
// is captured synchronously here, during this classic script's own top-level evaluation, since by the
// time `getWorker()` runs later (once Monaco actually needs a worker) `document.currentScript` would
// already be null.
const scriptUrl = (document.currentScript as HTMLScriptElement | null)?.src ?? self.location.href;

const workerUrl = (relativePath: string): URL => new URL(relativePath, scriptUrl);

self.MonacoEnvironment = {
    getWorker(_workerId: string, label: string): Worker {
        switch (label) {
            case "json":
                return new Worker(workerUrl("workers/json/json-worker-entry.js"));
            case "typescript":
            case "javascript":
                return new Worker(workerUrl("workers/ts/ts-worker-entry.js"));
            // monaco-editor's HTML contribution shares one htmlMode.js/html.worker.js across these
            // three registered language ids (see monaco-editor/esm/vs/language/html/monaco.contribution.js).
            // "liquid" is OrchardCore's own registration (see OrchardCore.Liquid/Assets/monaco/
            // liquid-intellisense.ts's `monaco.languages.html.registerHTMLLanguageService("liquid", ...)`) -
            // it reuses the same htmlMode.js worker infrastructure under its own language id as the label.
            case "html":
            case "handlebars":
            case "razor":
            case "liquid":
                return new Worker(workerUrl("workers/html/html-worker-entry.js"));
            // Likewise, monaco-editor's CSS contribution shares one cssMode.js/css.worker.js across
            // these three (see monaco-editor/esm/vs/language/css/monaco.contribution.js) - HTML's
            // embedded <style> language support routes through this too.
            case "css":
            case "less":
            case "scss":
                return new Worker(workerUrl("workers/css/css-worker-entry.js"));
            default:
                return new Worker(workerUrl("workers/editor/editor-worker-entry.js"));
        }
    },
};

declare global {
    interface Window {
        __orchardCoreMonacoReady?: Promise<typeof monaco>;
    }
}

window.__orchardCoreMonacoReady = Promise.resolve(monaco);
