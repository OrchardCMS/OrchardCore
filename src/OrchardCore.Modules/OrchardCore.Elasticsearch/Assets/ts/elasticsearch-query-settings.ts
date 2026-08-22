const menu = document.getElementById("SearchType") as HTMLSelectElement | null;
const queryContainer = document.getElementById("DefaultQueryContainer");
const fieldsContainer = document.getElementById("DefaultQueryFields");

if (menu && queryContainer && fieldsContainer) {
    menu.addEventListener("change", (e) => {
        const target = e.target as HTMLSelectElement;

        if (target.value === target.getAttribute("data-raw-type")) {
            queryContainer.classList.remove("d-none");
            fieldsContainer.classList.add("d-none");
        } else {
            queryContainer.classList.add("d-none");
            fieldsContainer.classList.remove("d-none");
        }
    });

    menu.dispatchEvent(new Event("change"));
}

export {};
