import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

// Defined by this module's own Assets/js/OptionsEditor/*.js (classic globals, shared by
// MultiTextFieldSettings and TextFieldPredefinedListEditorSettings - both wire an identical
// name/value options table plus an edit-in-modal JSON view over it).
declare function initializeOptionsEditor(
    element: HTMLElement,
    options: unknown,
    defaultValue: string,
    modalBodyElements: HTMLCollectionOf<Element>,
): void;

observeAndInit(".options-editor-wrapper", (element) => {
    const options = getDatasetJson(element, "options") ?? [];
    const defaultValue = element.dataset.defaultValue ?? "";
    const modalBodyElements = document.getElementsByClassName(`${element.id}-ModalBody`);

    initializeOptionsEditor(element, options, defaultValue, modalBodyElements);
});
