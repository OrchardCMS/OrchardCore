import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";
import initSelectPartEditor, { SelectOption } from "./select-part-editor";

observeAndInit(".select-part-editor", (wrapper) => {
    const fieldOptionsWrapper = wrapper.querySelector<HTMLElement>(".field-options-wrapper");

    if (fieldOptionsWrapper) {
        const options = getDatasetJson<SelectOption[]>(fieldOptionsWrapper, "options") ?? [];
        const defaultValue = fieldOptionsWrapper.dataset.defaultValue ?? "";
        const translations = getDatasetJson<Record<string, string>>(fieldOptionsWrapper, "translations");

        if (translations) {
            initSelectPartEditor({
                element: fieldOptionsWrapper,
                options,
                defaultValue,
                translations,
                hiddenDefaultValueInputId: fieldOptionsWrapper.dataset.defaultValueInputId ?? "",
                hiddenDefaultValueInputName: fieldOptionsWrapper.dataset.defaultValueInputName ?? "",
                hiddenOptionsInputId: fieldOptionsWrapper.dataset.optionsInputId ?? "",
                hiddenOptionsInputName: fieldOptionsWrapper.dataset.optionsInputName ?? "",
            });
        }
    }

    const selectMenus = wrapper.getElementsByClassName("field-type-select-menu");
    for (let i = 0; i < selectMenus.length; i++) {
        const selectMenu = selectMenus[i] as HTMLSelectElement;
        selectMenu.addEventListener("change", (e) => {
            const widgetWrapper = (e.target as HTMLElement).closest(".widget-editor-body");
            const visibleForInputContainers = widgetWrapper?.getElementsByClassName("show-for-input") ?? [];

            for (let j = 0; j < visibleForInputContainers.length; j++) {
                const container = visibleForInputContainers[j];
                const value = (e.target as HTMLSelectElement).value;
                if (value === "reset" || value === "submit" || value === "hidden") {
                    container.classList.add("d-none");
                } else {
                    container.classList.remove("d-none");
                }
            }
        });
        selectMenu.dispatchEvent(new Event("change"));
    }
});
