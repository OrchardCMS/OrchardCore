import { toServerDecimal, toJsDecimal } from "../helpers/numericFieldDecimal";

// Widget-attachable content field editor (repeatable), so the caller wires this up via
// observeAndInit rather than a top-level call.
const initNumericFieldRange = (wrapper: HTMLElement) => {
    const textInput = wrapper.querySelector<HTMLInputElement>(".numeric-field-range-text");
    const rangeInput = wrapper.querySelector<HTMLInputElement>(".numeric-field-range-slider");

    if (!textInput || !rangeInput) {
        return;
    }

    rangeInput.addEventListener("input", () => {
        textInput.value = toServerDecimal(rangeInput.value, wrapper);
    });
    rangeInput.addEventListener("change", () => {
        textInput.value = toServerDecimal(rangeInput.value, wrapper);
    });

    textInput.addEventListener("change", () => {
        const converted = toJsDecimal(textInput.value, wrapper);
        rangeInput.value = String(Number(converted) || 0);
    });
};

export default initNumericFieldRange;
