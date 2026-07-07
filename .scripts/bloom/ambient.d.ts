// Ambient declarations for third-party libraries that are still loaded as classic global
// <script> resources (via ResourceManifest/asp-name, not an npm import), so extracted view
// modules can reference them by name instead of each re-declaring its own "declare const".
// Kept intentionally minimal - only what the extracted views actually call - and grown
// incrementally as later waves need more of a given library's surface.

declare const CodeMirror: {
    fromTextArea(textArea: HTMLTextAreaElement, options?: Record<string, unknown>): unknown;
};

// jQuery itself is typed as loosely as possible on purpose: its plugins (iconpicker, trumbowyg,
// etc.) each attach their own chainable methods, which don't share a common shape worth modeling
// centrally. Call sites cast the (unknown) result of `$(...)` to a small, file-local interface
// describing only the plugin methods they actually use.
declare const $: (target: string | Element) => unknown;

declare const Cookies: {
    set(name: string, value: string, options?: Record<string, unknown>): void;
    get(name: string): string | undefined;
};

declare const Sortable: {
    create(element: HTMLElement, options?: Record<string, unknown>): unknown;
};
