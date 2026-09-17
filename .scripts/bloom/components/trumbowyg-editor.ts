// Shared by the HTML field/part Trumbowyg-based editors (both the customizable "Trumbowyg"
// variant, whose options come from admin-configured settings, and the fixed-default "Wysiwyg"
// variant, which extends Trumbowyg's own default button set once per page).
interface TrumbowygInstance {
    on(event: string, handler: () => void): TrumbowygInstance;
}

interface TrumbowygJQueryResult {
    trumbowyg(settings: Record<string, unknown>): TrumbowygInstance;
}

interface TrumbowygStatic {
    langs: Record<string, { _dir?: string } & Record<string, unknown>>;
    defaultOptions: { btns: Array<string | string[]> };
}

declare const jQuery: {
    (target: string | Element): TrumbowygJQueryResult;
    trumbowyg: TrumbowygStatic;
};

const ensureRtlLanguageClone = (languageCode: string, languageDirection: string) => {
    if (!jQuery.trumbowyg.langs[languageCode]) {
        jQuery.trumbowyg.langs[languageCode] = {
            ...jQuery.trumbowyg.langs["en"],
            _dir: languageDirection,
        };
    }
};

// Only used by the "Wysiwyg" variant, which gets all buttons by default. Mutates the shared
// Trumbowyg default options once per page - idempotent via the index-6 check below, so calling
// it again for a second Wysiwyg instance on the same page is harmless.
const extendDefaultButtonsWithShortcode = () => {
    const defaultButtons = jQuery.trumbowyg.defaultOptions.btns;

    if (defaultButtons[6] !== "insertShortcode") {
        defaultButtons.splice(6, 0, "insertShortcode");
    }

    defaultButtons.forEach((group) => {
        if (Array.isArray(group) && group.includes("undo") && group.includes("redo")) {
            group[group.indexOf("undo")] = "historyUndo";
            group[group.indexOf("redo")] = "historyRedo";
        }
    });
};

interface TrumbowygEditorOptions {
    element: HTMLTextAreaElement;
    languageCode: string;
    isRtl: boolean;
    languageDirection: string;
    customOptions?: Record<string, unknown>;
    extendDefaultButtons?: boolean;
}

const initTrumbowygEditor = (options: TrumbowygEditorOptions) => {
    const { element, languageCode, isRtl, languageDirection, customOptions, extendDefaultButtons } = options;

    if (isRtl) {
        ensureRtlLanguageClone(languageCode, languageDirection);
    }

    const settings = extendDefaultButtons
        ? { lang: languageCode, semantic: false }
        : { ...customOptions, lang: languageCode };

    if (extendDefaultButtons) {
        extendDefaultButtonsWithShortcode();
    }

    jQuery(element)
        .trumbowyg(settings)
        .on("tbwchange", () => {
            document.dispatchEvent(new CustomEvent("contentpreview:render"));
        });
};

export default initTrumbowygEditor;
