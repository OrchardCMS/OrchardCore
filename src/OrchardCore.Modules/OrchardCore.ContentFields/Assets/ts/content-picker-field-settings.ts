import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const sourceWrapper = document.getElementById("Source");
const sources = document.getElementsByName("Source");
const contentTypesContainer = document.getElementById("ContentTypesContainer");
const stereotypeContainer = document.getElementById("StereotypesContainer");

if (sourceWrapper) {
    const contentTypesValue = sourceWrapper.dataset.contentTypesValue;
    const stereotypesValue = sourceWrapper.dataset.stereotypesValue;

    sources.forEach((source) => {
        source.addEventListener("change", (event) => {
            const target = event.target as HTMLInputElement;

            if (!target.checked) {
                return;
            }

            if (target.value === stereotypesValue) {
                contentTypesContainer?.classList.add("d-none");
                stereotypeContainer?.classList.remove("d-none");
            } else if (target.value === contentTypesValue) {
                contentTypesContainer?.classList.remove("d-none");
                stereotypeContainer?.classList.add("d-none");
            } else {
                contentTypesContainer?.classList.add("d-none");
                stereotypeContainer?.classList.add("d-none");
            }
        });
        source.dispatchEvent(new Event("change"));
    });
}

const titlePatternTextArea = document.getElementById("TitlePattern") as HTMLTextAreaElement | null;

if (titlePatternTextArea) {
    initLiquidPatternEditor(titlePatternTextArea);
}
