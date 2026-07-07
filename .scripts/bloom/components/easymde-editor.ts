// Shared by the Markdown BodyPart/Field Wysiwyg editors - both build an EasyMDE instance on a
// markdown textarea, optionally merging admin-configured toolbar settings over the module's
// default mdeToolbar, and both apply the same RTL toolbar styling once initialized.
interface EasyMdeCustomOptions {
    toolbar?: Array<string | { name?: string }>;
    [key: string]: unknown;
}

const buildOptions = (markdownElement: HTMLTextAreaElement, customOptions?: EasyMdeCustomOptions) => {
    if (!customOptions) {
        return {
            element: markdownElement,
            forceSync: true,
            toolbar: mdeToolbar,
            autoDownloadFontAwesome: false,
        };
    }

    if (customOptions.toolbar) {
        const toolbarByName: Record<string, string | Record<string, unknown>> = {};

        mdeToolbar.forEach((button) => {
            const name = typeof button === "string" ? undefined : (button as { name?: string }).name;
            if (name) {
                toolbarByName[name] = button;
            }
        });

        customOptions.toolbar = customOptions.toolbar.map((item) =>
            typeof item === "string" ? (toolbarByName[item] ?? item) : item,
        );
    } else {
        customOptions.toolbar = mdeToolbar;
    }

    customOptions.element = markdownElement;
    customOptions.forceSync = true;
    customOptions.autoDownloadFontAwesome = false;

    return customOptions;
};

const initEasyMdeEditor = (
    markdownElement: HTMLTextAreaElement | null,
    isRtl: boolean,
    customOptions?: EasyMdeCustomOptions,
) => {
    // When rendered by a flow part only the element's own scripts are rendered, so the markdown textarea may not exist.
    if (!markdownElement) {
        return;
    }

    const mde = new EasyMDE(buildOptions(markdownElement, customOptions));

    initializeMdeShortcodeWrapper(mde);

    mde.codemirror.on("change", () => {
        document.dispatchEvent(new CustomEvent("contentpreview:render"));
    });

    if (isRtl) {
        document.querySelectorAll<HTMLElement>(".editor-toolbar").forEach((element) => {
            element.setAttribute("style", "direction:rtl;text-align:right");
        });
        document.querySelectorAll<HTMLElement>(".CodeMirror").forEach((element) => {
            element.setAttribute("style", "text-align:right");
        });
    }
};

export default initEasyMdeEditor;
