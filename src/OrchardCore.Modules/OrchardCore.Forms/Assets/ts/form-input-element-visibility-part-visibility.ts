// Defined by OrchardCore.Forms (Assets/js/form-visibility-rules.js), consumed here.
declare global {
    interface Window {
        formVisibilityGroupRules: {
            initialize(config: unknown): void;
        };
    }
}

document.querySelectorAll<HTMLElement>(".form-visibility-config").forEach((element) => {
    const json = element.getAttribute("data-model");
    const config = json ? JSON.parse(json) : {};
    window.formVisibilityGroupRules.initialize(config);
});

export {};
