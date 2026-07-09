import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const patternTextArea = document.getElementById("Pattern") as HTMLTextAreaElement | null;
const optionsSelect = document.getElementById("Options") as HTMLSelectElement | null;
const patternEditor = document.getElementById("patternEditor");

if (patternTextArea) {
    initLiquidPatternEditor(patternTextArea);
}

if (optionsSelect && patternEditor) {
    const setPatternVisibility = () => {
        const generatedOptions = (patternEditor.dataset.generatedOptions ?? "").split(",");
        patternEditor.classList.toggle("d-none", !generatedOptions.includes(optionsSelect.value));
    };

    setPatternVisibility();
    optionsSelect.addEventListener("change", setPatternVisibility);
}
