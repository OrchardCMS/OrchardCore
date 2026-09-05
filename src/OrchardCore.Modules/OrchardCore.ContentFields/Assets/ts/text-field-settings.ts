import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

// The DisplayDriver prefixes generated ids, so hardcoded getElementById("Pattern")/
// getElementById("Type") never match - use attribute selectors against the field-name
// suffix instead. "patternEditor" is a plain unprefixed id="" on a <div>, not an asp-for
// target, so it's unaffected and stays a getElementById lookup.
const patternTextArea = document.querySelector<HTMLTextAreaElement>("textarea[id$='Pattern']");
const typeSelect = document.querySelector<HTMLSelectElement>("select[id$='Type']");
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
