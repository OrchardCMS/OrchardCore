import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

// Defined by this module's own Assets/js/customMetaTagsEditor.js (classic global).
declare function initializeCustomMetaTagsEditor(
    element: HTMLElement,
    customMetaTags: unknown,
    modalBodyElements: HTMLCollectionOf<Element>,
): void;

observeAndInit(".seo-meta-part-custom-tags", (element) => {
    const customMetaTags = getDatasetJson(element, "customMetaTags") ?? [];
    const modalBodyElements = document.getElementsByClassName(`${element.id}-ModalBody`);

    initializeCustomMetaTagsEditor(element, customMetaTags, modalBodyElements);
});
