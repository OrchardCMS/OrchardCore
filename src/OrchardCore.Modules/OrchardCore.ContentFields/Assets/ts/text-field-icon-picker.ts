import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

interface IconPickerSelectedEvent {
    iconpickerValue: string;
    iconpickerInstance: {
        options: {
            fullClassFormatter(value: string): string;
        };
    };
}

interface IconPickerToggle {
    iconpicker(): IconPickerToggle;
    data(key: "iconpicker"): { update(value: string): void };
    on(event: "iconpickerSelected", handler: (event: IconPickerSelectedEvent) => void): IconPickerToggle;
}

const initIconPickerField = (wrapper: HTMLElement) => {
    const toggle = wrapper.querySelector<HTMLElement>(".icon-picker-toggle");
    const valueInput = wrapper.querySelector<HTMLInputElement>(".icon-picker-value");

    if (!toggle || !valueInput) {
        return;
    }

    const $toggle = $(toggle) as IconPickerToggle;

    $toggle.iconpicker();

    const storedIcon = valueInput.value;

    if (storedIcon) {
        $toggle.data("iconpicker").update(storedIcon);
    }

    $toggle.on("iconpickerSelected", (event) => {
        valueInput.value = event.iconpickerInstance.options.fullClassFormatter(event.iconpickerValue);
    });
};

observeAndInit(".icon-picker-field", initIconPickerField);
