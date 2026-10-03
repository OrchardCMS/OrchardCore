import initIconPickerTriggers from "@orchardcore/bloom/components/icon-picker-trigger";

document.querySelectorAll<HTMLInputElement>(".add-parent-link-checkbox").forEach((checkbox) => {
    checkbox.addEventListener("click", (e) => {
        const fieldSet = (e.currentTarget as HTMLElement).closest(".add-parent-link-fieldset");
        if (!fieldSet?.parentElement) {
            return;
        }

        const iconPickerFieldSets = Array.from(fieldSet.parentElement.children).filter(
            (element) => element !== fieldSet && element.classList.contains("icon-picker-for-content-type"),
        );
        const selected = (e.currentTarget as HTMLInputElement).checked;

        iconPickerFieldSets.forEach((iconPickerFieldSet) => {
            if (selected) {
                iconPickerFieldSet.classList.remove("collapse");
            } else {
                iconPickerFieldSet.classList.add("collapse");
            }
        });
    });
});

initIconPickerTriggers();
