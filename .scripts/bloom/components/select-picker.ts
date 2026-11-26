import { onEditorUnmounting } from "../helpers/editorLifecycle";

// Helpers for the pickers built on the bootstrap-select resource (@crestapps/bootstrap-select), loaded as a classic
// global <script> (asp-name="bootstrap-select") that exposes `window.Selectpicker`. Every picker of the admin, a static
// list of options as well as a list searched on the server, uses it so they look and behave the same.
//
// A picker is a <select> configured by its data-* attributes (data-live-search, data-show-selected-tags,
// data-selected-items-style, data-none-results-text, ...), see https://github.com/CrestApps/bootstrap-select.

// The part of a bootstrap-select instance the pickers use.
export interface SelectpickerInstance {
    element: HTMLSelectElement;
    newElement: HTMLElement;
    button: HTMLButtonElement;
    menuInner: HTMLElement;
    searchbox: HTMLInputElement | null;
    // The button creating an option from the search, when `openOptions` is on.
    createOptionButton: HTMLButtonElement | null;
    destroy(): void;
    refresh(): void;
    render(): void;
    hide(): void;
    show(): void;
}

// An option of a list built in JavaScript, with the `source` option.
export interface SelectpickerOptionData {
    value: string;
    text: string;
    subtext?: string;
    disabled?: boolean;
}

// Called with the options to show, whether there are more of them, and their total.
export type SelectpickerSourceCallback = (data: SelectpickerOptionData[], more?: boolean, totalItems?: number) => void;

export interface SelectpickerSource {
    // Loads the options shown before anything is typed in the search box.
    data?: (callback: SelectpickerSourceCallback, page: number) => void;
    // Loads the options matching what is typed in the search box.
    search?: (callback: SelectpickerSourceCallback, page: number, searchValue: string) => void;
    // Creates an option from what is typed in the search box, when `openOptions` is on. Returns the option, a
    // promise of it, or calls the callback with it; null cancels the creation.
    create?: (
        callback: (option: SelectpickerOptionData | null) => void,
        searchValue: string,
    ) => SelectpickerOptionData | Promise<SelectpickerOptionData | null> | null | void;
}

interface SelectpickerStatic {
    getOrCreateInstance(element: Element, options?: Record<string, unknown>): SelectpickerInstance | null;
    getInstance(element: Element): SelectpickerInstance | null;
}

export const getSelectpicker = (): SelectpickerStatic | undefined =>
    (window as unknown as { Selectpicker?: SelectpickerStatic }).Selectpicker;

// The values of the selected options, in the order of the options.
export const getSelectedValues = (select: HTMLSelectElement): string[] =>
    Array.from(select.selectedOptions)
        .map((option) => option.value)
        .filter((value) => value !== "");

// Tells the content preview, when there is one, that the edited content changed.
export const notifyContentPreview = () => {
    document.dispatchEvent(new CustomEvent("contentpreview:render"));
};

const initialized = new WeakSet<HTMLSelectElement>();

// Creates the option typed in the search box when Enter is pressed and no option matches it, like a click on the button
// creating it. bootstrap-select has a handler doing it, but its own keyboard handler keeps the event from reaching it.
const createOptionOnEnter = (picker: SelectpickerInstance) => {
    const { searchbox, createOptionButton } = picker;

    if (!searchbox || !createOptionButton) {
        return;
    }

    searchbox.addEventListener("keyup", (event) => {
        // When an option matches the search, Enter picks it instead.
        if (event.key !== "Enter" || createOptionButton.hidden || createOptionButton.disabled || picker.menuInner.querySelector("[role=option]")) {
            return;
        }

        createOptionButton.click();
    });
};

// Turns a server-rendered <select> into a bootstrap-select picker. When the form expects the selected values in a
// single field, `data-values-input` holds the id of that hidden <input>, and `data-values-format` how the values are
// written in it: "csv" (the default) for values separated by commas, or "json" for a JSON array. The picker is
// destroyed when the editor holding it is unmounted.
export const initSelectPicker = (
    select: HTMLSelectElement,
    options: Record<string, unknown> = {},
): SelectpickerInstance | null => {
    const Selectpicker = getSelectpicker();

    if (!Selectpicker || initialized.has(select)) {
        return Selectpicker?.getInstance(select) ?? null;
    }

    initialized.add(select);

    const picker = Selectpicker.getOrCreateInstance(select, options);

    if (picker) {
        createOptionOnEnter(picker);
    }

    const valuesInputId = select.dataset.valuesInput;
    const valuesInput = valuesInputId ? document.getElementById(valuesInputId) : null;

    if (valuesInput instanceof HTMLInputElement) {
        const writeValues = () => {
            const values = getSelectedValues(select);

            valuesInput.value = select.dataset.valuesFormat === "json" ? JSON.stringify(values) : values.join(",");
        };

        writeValues();

        select.addEventListener("change", () => {
            writeValues();
            notifyContentPreview();
        });
    } else {
        select.addEventListener("change", notifyContentPreview);
    }

    onEditorUnmounting(select, () => {
        picker?.destroy();
        initialized.delete(select);
    });

    return picker;
};
