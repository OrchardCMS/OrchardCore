import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

const initColorField = (wrapper: HTMLElement) => {
    const colorField = wrapper.querySelector<HTMLInputElement>(".color-field-value");
    const colorPicker = wrapper.querySelector<HTMLInputElement>(".color");
    const colorOpacity = wrapper.querySelector<HTMLInputElement>(".color_opacity");

    if (!colorField || !colorPicker || !colorOpacity) {
        return;
    }

    const updateColor = () => {
        const alpha =
            colorOpacity.value === "255" ? "" : parseInt(colorOpacity.value, 10).toString(16).padStart(2, "0");

        wrapper.style.backgroundColor = colorPicker.value + alpha;
        colorField.value = colorPicker.value + alpha;
    };

    colorPicker.addEventListener("input", updateColor);
    colorOpacity.addEventListener("input", updateColor);
    updateColor();
};

observeAndInit(".color_wrapper", initColorField);
