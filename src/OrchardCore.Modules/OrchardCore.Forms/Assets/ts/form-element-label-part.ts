import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

// Defined by OrchardCore.Forms (Assets/js/formElementLabelManager.js), consumed here.
declare const formElementLabelManager: {
    initilize(wrapper: HTMLElement): void;
};

observeAndInit(".form-element-label-part-editor", (wrapper) => {
    formElementLabelManager.initilize(wrapper);
});
