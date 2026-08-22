const collapseCheckbox = document.getElementById("Settings_Collapse") as HTMLInputElement | null;
const preventFieldNameCollisionCheckbox = document.getElementById(
    "Settings_PreventFieldNameCollision",
) as HTMLInputElement | null;

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
