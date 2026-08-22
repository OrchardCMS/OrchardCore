import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

// Defined by OrchardCore.Forms (Assets/js/SelectOptionsEditor/selectOptionsEditor.js), consumed here.
declare const selectOptionsEditor: {
    initilizeElement(elementId: string, options: unknown, defaultValue: string): void;
    initilizeFieldType(wrapper: HTMLElement): void;
};

observeAndInit(".select-part-editor", (wrapper) => {
    const fieldOptionsWrapper = wrapper.querySelector<HTMLElement>(".field-options-wrapper");

    if (fieldOptionsWrapper) {
        const options = JSON.parse(fieldOptionsWrapper.dataset.options ?? "[]");
        const defaultValue = fieldOptionsWrapper.dataset.defaultvalue ?? "";

        selectOptionsEditor.initilizeElement(fieldOptionsWrapper.id, options, defaultValue);
    }

    selectOptionsEditor.initilizeFieldType(wrapper);
});
