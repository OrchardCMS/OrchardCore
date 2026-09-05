import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

// The DisplayDriver prefixes generated ids, so hardcoded getElementById("Pattern")/
// getElementById("Options") never match - use attribute selectors against the field-name
// suffix instead. "patternEditor" is a plain unprefixed id="" on a <div>, not an asp-for
// target, so it's unaffected and stays a getElementById lookup.
const patternTextArea = document.querySelector<HTMLTextAreaElement>("textarea[id$='Pattern']");
const optionsSelect = document.querySelector<HTMLSelectElement>("select[id$='Options']");
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
