// Ambient declarations for third-party libraries that are still loaded as classic global
// <script> resources (via ResourceManifest/asp-name, not an npm import), so extracted view
// modules can reference them by name instead of each re-declaring its own "declare const".
// Kept intentionally minimal - only what the extracted views actually call - and grown
// incrementally as later waves need more of a given library's surface.

interface CodeMirrorEditor {
    on(event: string, handler: (editor: CodeMirrorEditor) => void): void;
    save(): void;
    getValue(): string;
    setValue(value: string): void;
    setOption(key: string, value: unknown): void;
    display: { wrapper: HTMLElement };
}

declare const CodeMirror: {
    fromTextArea(textArea: HTMLTextAreaElement, options?: Record<string, unknown>): CodeMirrorEditor;
};

// Defined by OrchardCore.Shortcodes (Assets/js/shortcodes.js), consumed here.
declare function initializeCodeMirrorShortcodeWrapper(editor: CodeMirrorEditor): void;
declare const shortcodesApp: {
    init(callback: (value: string) => void): void;
};

interface MonacoTextModel {
    setValue(value: string): void;
    getValue(): string;
    onDidChangeContent(listener: (event: unknown) => void): void;
}

interface MonacoEditorInstance {
    getModel(): MonacoTextModel;
    getValue(): string;
    getSelection(): unknown;
    executeEdits(source: string, edits: unknown[]): void;
    addAction(action: Record<string, unknown>): void;
    focus(): void;
}

// Monaco is lazy-loaded via its own RequireJS/AMD loader (the "monaco" resource just installs
// `require`/`monaco` as globals), not bundled by Parcel - extracted views still call the global
// `require(['vs/editor/editor.main'], callback)` rather than an ES `import`.
declare const monaco: {
    editor: {
        create(element: HTMLElement, options?: Record<string, unknown>): MonacoEditorInstance;
        createModel(value: string, language: string, uri?: { toString(): string }): MonacoTextModel;
        setTheme(theme: string): void;
    };
    languages: {
        json: {
            jsonDefaults: {
                setDiagnosticsOptions(options: Record<string, unknown>): void;
            };
        };
        typescript: {
            javascriptDefaults: {
                addExtraLib(content: string, filePath?: string): void;
            };
        };
    };
    Uri: {
        parse(value: string): { toString(): string };
    };
    KeyMod: Record<string, number>;
    KeyCode: Record<string, number>;
};
declare function require(dependencies: string[], callback: () => void): void;

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

declare const bootstrap: typeof import("bootstrap");

declare const confirmDialog: (options: Record<string, unknown> & { callback: (response: boolean) => void }) => void;

// The qrcodejs vendored library (loaded as a classic global "qrcode" resource).
declare class QRCode {
    constructor(element: HTMLElement, options: Record<string, unknown>);
}

// EasyMDE (markdown Wysiwyg editor) and its toolbar/shortcode-wrapper helper, both loaded as
// classic global resources (easymde.min.js, OrchardCore.Markdown's mde.mediatoolbar.js).
interface EasyMdeInstance {
    codemirror: CodeMirrorEditor;
}

declare class EasyMDE implements EasyMdeInstance {
    constructor(options: Record<string, unknown>);
    codemirror: CodeMirrorEditor;
}

declare const mdeToolbar: Array<string | Record<string, unknown>>;
declare function initializeMdeShortcodeWrapper(mde: EasyMdeInstance): void;

// Defined by OrchardCore.Resources (Assets/js/credential-helpers.js), consumed here.
declare function randomUUID(options?: { includeHyphens?: boolean }): string;
declare function togglePasswordVisibility(passwordCtl: HTMLElement, togglePasswordCtl: HTMLElement): void;
declare function copyToClipboard(str: string): Promise<void>;
declare function generateStrongPassword(options?: { generateBase64?: boolean }): string;

// Defined by OrchardCore.Users (Assets/js/password-generator.js), consumed here.
declare const passwordManager: {
    generatePassword(
        requiredPasswordLength: number,
        requireUppercase: boolean,
        requireLowercase: boolean,
        requireDigit: boolean,
        requireNonAlphanumeric: boolean,
        requiredUniqueChars: boolean,
    ): string;
    copyPassword(password: string): void;
};
