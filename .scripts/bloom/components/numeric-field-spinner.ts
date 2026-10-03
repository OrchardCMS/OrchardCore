import { toJsDecimal, toServerDecimal } from "../helpers/numericFieldDecimal";

// Widget-attachable content field editor (repeatable), so the caller wires this up via
// observeAndInit rather than a top-level call.
const initNumericFieldSpinner = (wrapper: HTMLElement) => {
    const input = wrapper.querySelector<HTMLInputElement>(".numeric-field-spinner-value");

    if (!input) {
        return;
    }

    const scale = Number(wrapper.dataset.scale ?? "0");
    const min = Number(wrapper.dataset.min ?? "0");
    const max = Number(wrapper.dataset.max ?? "0");

    wrapper.querySelectorAll<HTMLButtonElement>(".numeric-field-spinner-step").forEach((button) => {
        const step = Number(button.dataset.step ?? "0");

        button.addEventListener("click", () => {
            const jsValue = toJsDecimal(input.value, wrapper);
            let numericValue = (Number(jsValue) || 0) + step;
            numericValue = Math.min(numericValue, max); // Never go above max
            numericValue = Math.max(numericValue, min); // or below min
            const rounded = Number(numericValue.toFixed(scale)).toString(); // Drop insignificant trailing zeros
            input.value = toServerDecimal(rounded, wrapper);
        });
    });
};

export default initNumericFieldSpinner;
