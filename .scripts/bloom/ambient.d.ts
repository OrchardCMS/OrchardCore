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

// OrchardCore.UrlRewriting/Assets/js/sortable-rules.js - a classic global shared by any
// admin list needing drag-to-reorder + a single AJAX call to persist the new position.
declare const sortingListManager: {
    create(selector: string, sortUrl: string, errorMessage?: string): void;
};

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

// Defined by OrchardCore.Liquid (wwwroot/monaco/liquid-intellisense.js), consumed here.
declare function ConfigureLiquidIntellisense(monacoInstance: typeof import("monaco-editor"), registerHtml?: boolean): void;

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

// Defined by OrchardCore.AdminMenu (Assets/js/admin-menu-icon-picker.js), consumed here.
declare const iconPickerVue: {
    show(relatedNodeId: string, sampleIconId: string): void;
};

// Defined by OrchardCore.AdminMenu (Assets/js/admin-menu-permission-picker.js), consumed here.
declare function initAdminMenuPermissionsPicker(element: Element | null): void;

// Defined by OrchardCore.Menu (Assets/js/menu-permission-picker.js) - a separate, near-identical
// copy of the picker above for MenuItemPermissionPart, unrelated to the AdminMenu tree editors.
declare function initMenuPermissionsPicker(element: Element | null): void;

interface NoUiSliderInstance {
    set(value: number | string): void;
    get(): string;
    on(event: string, handler: () => void): void;
}

interface NoUiSliderElement extends HTMLElement {
    noUiSlider: NoUiSliderInstance;
}

// Defined by the nouislider vendored resource, consumed here.
declare const noUiSlider: {
    create(element: HTMLElement, options: Record<string, unknown>): void;
};

// Defined by OrchardCore.OpenId (Scripts/parametersEditor.js), consumed here.
declare function initializeParametersEditor(
    element: Element | null,
    parameters: unknown,
    modalBodyElements: HTMLCollectionOf<Element>,
): void;
