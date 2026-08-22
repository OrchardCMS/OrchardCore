import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

const initializeFieldType = (wrapper: HTMLElement) => {
    const selectMenus = wrapper.getElementsByClassName("field-type-select-menu");

    for (let i = 0; i < selectMenus.length; i++) {
        const selectMenu = selectMenus[i] as HTMLSelectElement;
        const widgetWrapper = selectMenu.closest<HTMLElement>(".widget-editor-body");

        selectMenu.addEventListener("change", (e) => {
            const visibleForInputContainers = (widgetWrapper ?? wrapper).getElementsByClassName("show-for-input");
            if (visibleForInputContainers.length === 0) {
                return;
            }

            const value = (e.target as HTMLSelectElement).value;
            for (let j = 0; j < visibleForInputContainers.length; j++) {
                if (value === "reset" || value === "submit" || value === "hidden") {
                    visibleForInputContainers[j].classList.add("d-none");
                } else {
                    visibleForInputContainers[j].classList.remove("d-none");
                }
            }
        });
    }
};

observeAndInit(".input-part-editor", initializeFieldType);
