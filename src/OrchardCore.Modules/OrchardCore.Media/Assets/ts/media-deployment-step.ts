import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { initReverseToggle } from "@orchardcore/bloom/components/checkbox-relations";

const setCollapse = (element: Element | null, action: "show" | "hide") => {
    if (!element) {
        return;
    }

    bootstrap.Collapse.getOrCreateInstance(element, { toggle: false })[action]();
};

const checkboxClicked = (checkbox: HTMLInputElement) => {
    const path = checkbox.value;

    document.querySelectorAll<HTMLElement>("li[data-path]").forEach((element) => {
        if (element.dataset.path === path) {
            setCollapse(element, checkbox.checked ? "hide" : "show");
        }
    });

    document.querySelectorAll<HTMLInputElement>("input[data-parent-value]:checked").forEach((element) => {
        if (element.dataset.parentValue !== path) {
            return;
        }

        element.checked = false;
        checkboxClicked(element);
    });
};

observeAndInit("[data-reversetoggle]", initReverseToggle);

document.querySelectorAll<HTMLInputElement>("[data-parent-value]").forEach((element) => {
    element.addEventListener("click", () => checkboxClicked(element));
});
