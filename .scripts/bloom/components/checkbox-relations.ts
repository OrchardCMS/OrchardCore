// A handful of generic checkbox-driven DOM relationships, copy-pasted identically across several
// deployment-step editors (Content Definitions, Custom Settings, Deployment Plans, Media). Each
// behavior is independent and reads its own target selector off the triggering checkbox's own
// data-* attribute, so a view only needs to register the ones it actually uses.

// Checking the checkbox collapses (and unchecking expands) every element matched by its own
// data-reversetoggle selector - "reverse" because a Bootstrap collapse's natural checkbox wiring
// shows content when checked, and this is used for an "include all" checkbox that should do the
// opposite (hide the individual picker once "include all" is checked).
const initReverseToggle = (element: HTMLElement) => {
    const checkbox = element as HTMLInputElement;

    checkbox.addEventListener("click", () => {
        const selector = checkbox.dataset.reversetoggle;

        if (!selector) {
            return;
        }

        document.querySelectorAll<HTMLElement>(selector).forEach((target) => {
            const collapse = bootstrap.Collapse.getOrCreateInstance(target, { toggle: false });

            if (checkbox.checked) {
                collapse.hide();
            } else {
                collapse.show();
            }
        });
    });
};

// Every checkbox matched by data-checkbox mirrors this checkbox's checked state exactly.
const initCheckboxLink = (element: HTMLElement) => {
    const checkbox = element as HTMLInputElement;

    checkbox.addEventListener("click", () => {
        const selector = checkbox.dataset.checkbox;

        if (!selector) {
            return;
        }

        document.querySelectorAll<HTMLInputElement>(selector).forEach((target) => {
            target.checked = checkbox.checked;
        });
    });
};

// Checking this checkbox also checks every checkbox matched by data-checkboxchecked (one-way).
const initCheckboxCheckedLink = (element: HTMLElement) => {
    const checkbox = element as HTMLInputElement;

    checkbox.addEventListener("click", () => {
        const selector = checkbox.dataset.checkboxchecked;

        if (!checkbox.checked || !selector) {
            return;
        }

        document.querySelectorAll<HTMLInputElement>(selector).forEach((target) => {
            target.checked = true;
        });
    });
};

// Unchecking this checkbox also unchecks every checkbox matched by data-checkboxunchecked (one-way).
const initCheckboxUncheckedLink = (element: HTMLElement) => {
    const checkbox = element as HTMLInputElement;

    checkbox.addEventListener("click", () => {
        const selector = checkbox.dataset.checkboxunchecked;

        if (checkbox.checked || !selector) {
            return;
        }

        document.querySelectorAll<HTMLInputElement>(selector).forEach((target) => {
            target.checked = false;
        });
    });
};

export { initReverseToggle, initCheckboxLink, initCheckboxCheckedLink, initCheckboxUncheckedLink };
