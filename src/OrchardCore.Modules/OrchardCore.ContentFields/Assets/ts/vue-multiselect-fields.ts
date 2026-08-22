import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

// Defined by OrchardCore.Resources (Assets/js/vue-multiselect-wrapper.js), shared by every plain
// content-picker-shaped field (ContentPickerField, LocalizationSetContentPickerField).
declare function initVueMultiselect(element: Element | null): void;

// Defined by this module's own Assets/js/vue-multiselect-userpicker.js - a separate copy with its
// own debounced search, so it stays its own function rather than merging into initVueMultiselect.
declare function initVueMultiselectUserPicker(element: Element | null): void;

// Defined by this module's own Assets/js/vue-multiselect-multitextfieldpicker.js.
declare function initMultiTextFieldPicker(element: Element | null): void;

observeAndInit(
    ".vue-multiselect[data-editor-type='ContentPicker'], .vue-multiselect[data-editor-type='LocalizationSetContentPicker']",
    (element) => initVueMultiselect(element),
);
observeAndInit(".vue-multiselect[data-editor-type='UserPicker']", (element) => initVueMultiselectUserPicker(element));
observeAndInit(".multitextfieldpicker", (element) => initMultiTextFieldPicker(element));
