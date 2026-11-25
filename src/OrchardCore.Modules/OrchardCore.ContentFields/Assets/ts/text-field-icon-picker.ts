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
    iconpicker(options?: { templates?: Record<string, string> }): IconPickerToggle;
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

    const searchInput = document.createElement("input");
    searchInput.type = "search";
    searchInput.id = `${valueInput.id}_Search`;
    searchInput.className = "form-control iconpicker-search";
    searchInput.placeholder = "Type to filter";

    $toggle.iconpicker({ templates: { search: searchInput.outerHTML } });

    const storedIcon = valueInput.value;

    if (storedIcon) {
        $toggle.data("iconpicker").update(storedIcon);
    }

    $toggle.on("iconpickerSelected", (event) => {
        valueInput.value = event.iconpickerInstance.options.fullClassFormatter(event.iconpickerValue);
    });
};

observeAndInit(".icon-picker-field", initIconPickerField);
