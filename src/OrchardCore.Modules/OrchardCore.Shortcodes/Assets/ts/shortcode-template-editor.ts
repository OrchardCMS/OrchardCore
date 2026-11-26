import { initSelectPicker } from "@orchardcore/bloom/components/select-picker";

// Defined by this module's own Assets/js/shortcode-templates.js (classic global): the CodeMirror
// wiring and the preview-panel updates.
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

// The categories of the shortcode: a bootstrap-select of the categories of the shortcodes, where a category typed in
// the search box that doesn't exist yet is added.
const categoriesSelect = document.querySelector<HTMLSelectElement>("select[data-shortcode-categories]");

if (categoriesSelect) {
    initSelectPicker(categoriesSelect);
}
