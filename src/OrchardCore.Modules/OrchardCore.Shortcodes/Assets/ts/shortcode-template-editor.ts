import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";
import initShortcodeCategoriesEditor from "@orchardcore/bloom/components/shortcode-categories-editor";

// Defined by this module's own Assets/js/shortcode-templates.js (classic global) - still handles
// the CodeMirror wiring and preview-panel updates, unrelated to the Vue-owned categories editor
// migrated below.
declare function initializeShortcodeCodeMirrorEditors(
    contentElement: Element | null,
    usageElement: Element | null,
    previewElement: Element | null,
    nameElement: Element | null,
    hintElement: Element | null,
): void;

initializeShortcodeCodeMirrorEditors(
    document.getElementById("Content"),
    document.getElementById("Usage"),
    document.getElementById("shortcodePreview"),
    document.getElementById("Name"),
    document.getElementById("Hint"),
);

const categoriesElement = document.getElementById("shortcodeCategories");
if (categoriesElement) {
    const allCategories = getDatasetJson<string[]>(categoriesElement, "categories") ?? [];
    const selectedCategories = getDatasetJson<string[]>(categoriesElement, "selectedCategories") ?? [];
    const translations = getDatasetJson<Record<string, string>>(categoriesElement, "translations");

    if (translations) {
        initShortcodeCategoriesEditor({
            element: categoriesElement,
            allCategories,
            selectedCategories,
            hiddenInputId: categoriesElement.dataset.hiddenInputId ?? "",
            hiddenInputName: categoriesElement.dataset.hiddenInputName ?? "",
            translations,
        });
    }
}
