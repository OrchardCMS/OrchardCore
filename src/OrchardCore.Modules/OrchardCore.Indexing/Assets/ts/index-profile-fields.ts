declare const TechnicalNameGenerator: {
    initialize(displayId: string, nameId: string): void;
};

const wrapper = document.querySelector<HTMLElement>(".index-profile-technical-name-generator");

if (wrapper) {
    const displayId = wrapper.dataset.displayId;
    const nameId = wrapper.dataset.nameId;

    if (displayId && nameId) {
        TechnicalNameGenerator.initialize(displayId, nameId);
    }
}
