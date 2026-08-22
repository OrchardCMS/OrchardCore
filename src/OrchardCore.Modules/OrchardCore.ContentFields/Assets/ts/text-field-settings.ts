import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const patternTextArea = document.getElementById("Pattern") as HTMLTextAreaElement | null;
const typeSelect = document.getElementById("Type") as HTMLSelectElement | null;
const patternEditor = document.getElementById("patternEditor");

if (patternTextArea) {
    initLiquidPatternEditor(patternTextArea);
}

if (typeSelect && patternEditor) {
    const setPatternVisibility = () => {
        const generatedTypes = (patternEditor.dataset.generatedTypes ?? "").split(",");
        patternEditor.classList.toggle("d-none", !generatedTypes.includes(typeSelect.value));
    };

    setPatternVisibility();
    typeSelect.addEventListener("change", setPatternVisibility);
}
