import initIconPickerTriggers from "@orchardcore/bloom/components/icon-picker-trigger";

const showAllToggle = document.getElementById("ContentTypesAdminNode_ShowAll") as HTMLInputElement | null;
const showAllCard = document.getElementById("show-all-card");
const showSelectedCard = document.getElementById("show-selected-card");

const showElement = (element: HTMLElement | null) => {
    if (element) {
        element.style.display = "block";
    }
};

const hideElement = (element: HTMLElement | null) => {
    if (element) {
        element.style.display = "none";
    }
};

const switchContentTypesCheckBoxes = () => {
    if (showAllToggle?.checked) {
        showElement(showAllCard);
        hideElement(showSelectedCard);
    } else {
        hideElement(showAllCard);
        showElement(showSelectedCard);
    }
};

switchContentTypesCheckBoxes();
showAllToggle?.addEventListener("change", switchContentTypesCheckBoxes);

document.querySelectorAll<HTMLInputElement>(".content-type-checkbox").forEach((checkbox) => {
    checkbox.addEventListener("click", (e) => {
        const listItem = (e.target as HTMLElement).closest(".list-group-item");
        const selected = (e.target as HTMLInputElement).checked;

        if (selected) {
            listItem?.classList.remove("disabled-content-type");
        } else {
            listItem?.classList.add("disabled-content-type");
        }
    });
});

initIconPickerTriggers();
