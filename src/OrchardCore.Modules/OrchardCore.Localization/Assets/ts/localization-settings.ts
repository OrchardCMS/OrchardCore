import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

// Defined by this module's own Assets/js/optionsEditor.js - a separate, unrelated component
// from ContentFields' options editor of the same name (this one edits the site's supported
// culture list, not a field's predefined value list).
declare function initializeOptionsEditor(
    element: HTMLElement,
    supportedCultures: unknown,
    defaultCulture: string,
    selectedCulture: string,
    allCultures: unknown,
): void;

const wrapper = document.querySelector<HTMLElement>(".localization-settings-wrapper");

if (wrapper) {
    const supportedCultures = getDatasetJson(wrapper, "supportedCultures") ?? [];
    const allCultures = getDatasetJson(wrapper, "allCultures") ?? [];
    const defaultCulture = wrapper.dataset.defaultCulture ?? "";
    const selectedCulture = wrapper.dataset.selectedCulture ?? "";

    initializeOptionsEditor(wrapper, supportedCultures, defaultCulture, selectedCulture, allCultures);
}
