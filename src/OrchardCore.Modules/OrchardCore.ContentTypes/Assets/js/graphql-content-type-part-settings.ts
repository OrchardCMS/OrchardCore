// The DisplayDriver prefixes generated ids, so hardcoded getElementById("Settings_Collapse")/
// getElementById("Settings_PreventFieldNameCollision") never match - use attribute
// selectors against the field-name suffix instead.
const collapseCheckbox = document.querySelector<HTMLInputElement>("input[id$='Settings_Collapse']");
const preventFieldNameCollisionCheckbox = document.querySelector<HTMLInputElement>(
    "input[id$='Settings_PreventFieldNameCollision']",
);

if (collapseCheckbox && preventFieldNameCollisionCheckbox) {
    const setFieldVisibility = (checked: boolean) => {
        if (preventFieldNameCollisionCheckbox.parentElement) {
            preventFieldNameCollisionCheckbox.parentElement.style.display = checked ? "block" : "none";
        }

        if (!checked) {
            preventFieldNameCollisionCheckbox.checked = false;
        }
    };

    collapseCheckbox.addEventListener("change", (e) => setFieldVisibility((e.target as HTMLInputElement).checked));

    setFieldVisibility(collapseCheckbox.checked);
}

export {};
