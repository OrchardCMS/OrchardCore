const addSelectedPartsButton = document.getElementById("add-selected-parts-button") as HTMLButtonElement | null;
const partSelectionCheckboxes = document.querySelectorAll<HTMLInputElement>('input[type="checkbox"][name$=".IsSelected"]');

const updateAddSelectedPartsButton = () => {
    if (!addSelectedPartsButton) {
        return;
    }

    addSelectedPartsButton.hidden = !Array.from(partSelectionCheckboxes).some((checkbox) => checkbox.checked);
};

partSelectionCheckboxes.forEach((checkbox) => checkbox.addEventListener("change", updateAddSelectedPartsButton));
updateAddSelectedPartsButton();

export {};
