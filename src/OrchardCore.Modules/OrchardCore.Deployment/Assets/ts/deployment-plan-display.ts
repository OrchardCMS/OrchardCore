import removeDiacritics from "@orchardcore/bloom/helpers/removeDiacritics";

function getDeploymentStepCategory() {
    const activeCategory = document.querySelector<HTMLElement>("#deployment-step-categories .nav-link.active");

    return activeCategory ? activeCategory.dataset.category || "all" : "all";
}

function filterDeploymentSteps() {
    const searchBox = document.getElementById("search-box") as HTMLInputElement | null;
    const search = removeDiacritics(String(searchBox ? searchBox.value || "" : "").toLowerCase());
    const selectedCategory = getDeploymentStepCategory();

    document.querySelectorAll<HTMLElement>(".deployment-step-item").forEach((card) => {
        const cardText = removeDiacritics((card.textContent ?? "").toLowerCase());
        const matchesSearch = search === "" || cardText.indexOf(search) > -1;
        const matchesCategory = selectedCategory === "all" || card.dataset.category === selectedCategory;

        card.style.display = matchesSearch && matchesCategory ? "" : "none";
    });
}

const modalSteps = document.getElementById("modalSteps");
const searchBox = document.getElementById("search-box") as HTMLInputElement | null;
const deploymentStepCategories = document.getElementById("deployment-step-categories");

if (modalSteps) {
    modalSteps.addEventListener("shown.bs.modal", () => {
        searchBox?.focus();
        filterDeploymentSteps();
    });

    modalSteps.addEventListener("hidden.bs.modal", () => {
        if (searchBox) {
            searchBox.value = "";
        }
        document.querySelectorAll("#deployment-step-categories .nav-link").forEach((link) => {
            link.classList.remove("active");
        });
        const allCategory = document.querySelector('#deployment-step-categories .nav-link[data-category="all"]');
        allCategory?.classList.add("active");
        filterDeploymentSteps();
    });
}

searchBox?.addEventListener("input", () => {
    filterDeploymentSteps();
});

deploymentStepCategories?.addEventListener("click", (e) => {
    const link = (e.target as HTMLElement).closest<HTMLElement>(".nav-link");

    if (!link || !deploymentStepCategories.contains(link)) {
        return;
    }

    e.preventDefault();
    document.querySelectorAll("#deployment-step-categories .nav-link").forEach((navLink) => {
        navLink.classList.remove("active");
    });
    link.classList.add("active");
    filterDeploymentSteps();
});

export {};
