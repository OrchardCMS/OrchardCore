import { toJsDecimal, toServerDecimal } from "../helpers/numericFieldDecimal";

// Widget-attachable content field editor (repeatable), so the caller wires this up via
// observeAndInit rather than a top-level call.
const initNumericFieldSlider = (wrapper: HTMLElement) => {
    const slider = wrapper.querySelector<NoUiSliderElement>(".numeric-field-slider-track");
    const input = wrapper.querySelector<HTMLInputElement>(".numeric-field-slider-value");

    if (!slider || !input) {
        return;
    }

    const min = Number(wrapper.dataset.min ?? "0");
    const max = Number(wrapper.dataset.max ?? "0");
    const step = Number(wrapper.dataset.step ?? "1");
    const direction = wrapper.dataset.direction ?? "ltr";
    const value = wrapper.dataset.value ?? "";

    noUiSlider.create(slider, {
        start: [min],
        step,
        direction,
        connect: true,
        range: { min, max },
    });

    slider.noUiSlider.set(value);

    slider.noUiSlider.on("change", () => {
        input.value = toServerDecimal(slider.noUiSlider.get(), wrapper);
    });

    input.addEventListener("keyup", () => {
        const converted = toJsDecimal(input.value, wrapper);
        slider.noUiSlider.set(Number(converted) || 0);
    });
};

export default initNumericFieldSlider;
