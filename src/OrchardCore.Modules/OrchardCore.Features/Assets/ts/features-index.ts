const EscapeKey = "Escape";
const searchBox = document.getElementById("search-box") as HTMLInputElement | null;
const featureGroups = Array.from(document.querySelectorAll<HTMLElement>(".feature-group"));
const featureItems = featureGroups.flatMap((featureGroup) =>
    Array.from(featureGroup.querySelectorAll<HTMLElement>(".list-group-item[data-search-text]")),
);
const visibilityFilter = document.getElementById("visibility-filter") as HTMLSelectElement | null;
const statusFilter = document.getElementById("status-filter") as HTMLSelectElement | null;
const listAlert = document.getElementById("list-alert");
const featuresSummary = document.getElementById("features-summary");
const bulkActionInput = document.querySelector<HTMLInputElement>("input[name='BulkAction']");
const bulkActionSubmit = document.querySelector<HTMLInputElement>("input[name='submit.BulkAction']");
const bulkActionLinks = Array.from(document.querySelectorAll<HTMLAnchorElement>("a[data-action]"));
const badgeShowMoreButtons = Array.from(document.querySelectorAll<HTMLElement>(".badge-show-more"));
const featureGroupSelectAllCheckboxes = Array.from(
    document.querySelectorAll<HTMLInputElement>(".feature-group-select-all"),
);

function isVisible(element: Element) {
    return !element.classList.contains("d-none");
}

function getVisibleFeatureItemCheckboxes(featureGroup: HTMLElement) {
    return Array.from(featureGroup.querySelectorAll<HTMLInputElement>("input[name='featureIds']")).filter(
        (checkbox) => {
            const featureItem = checkbox.closest(".list-group-item");

            return featureItem !== null && isVisible(featureItem);
        },
    );
}

function updateFeatureGroupSelectAllState(featureGroup: HTMLElement) {
    const featureGroupSelectAllCheckbox = featureGroup.querySelector<HTMLInputElement>(".feature-group-select-all");
    const featureGroupToggleContainer = featureGroup.querySelector<HTMLElement>(".feature-group-toggle-container");

    if (!featureGroupSelectAllCheckbox || !featureGroupToggleContainer) {
        return;
    }

    const visibleFeatureCheckboxes = getVisibleFeatureItemCheckboxes(featureGroup);
    const hasVisibleFeatureCheckboxes = visibleFeatureCheckboxes.length > 0;
    const allVisibleFeatureCheckboxesChecked =
        hasVisibleFeatureCheckboxes && visibleFeatureCheckboxes.every((checkbox) => checkbox.checked);
    const hasAnyVisibleFeatureCheckboxChecked = visibleFeatureCheckboxes.some((checkbox) => checkbox.checked);
    const featureGroupSelectAllLabel = featureGroup.querySelector<HTMLElement>(".feature-group-select-all-label");
    const featureGroupSelectAllText = featureGroup.querySelector<HTMLElement>(".feature-group-select-all-text");

    const isHidden = !hasVisibleFeatureCheckboxes;
    featureGroupToggleContainer.classList.toggle("d-none", isHidden);

    let nextListItem = featureGroupToggleContainer.closest("li")?.nextElementSibling;
    while (nextListItem && nextListItem.tagName !== "LI") {
        nextListItem = nextListItem.nextElementSibling;
    }

    if (nextListItem) {
        nextListItem.classList.toggle("first-child-visible", isHidden);
    }

    featureGroupSelectAllCheckbox.disabled = isHidden;
    featureGroupSelectAllCheckbox.checked = allVisibleFeatureCheckboxesChecked;
    featureGroupSelectAllCheckbox.indeterminate = hasAnyVisibleFeatureCheckboxChecked && !allVisibleFeatureCheckboxesChecked;

    if (featureGroupSelectAllLabel && featureGroupSelectAllText) {
        const selectAllText = featureGroupSelectAllLabel.dataset.selectAllText ?? "";
        featureGroupSelectAllText.textContent = selectAllText.replace(
            "__COUNT__",
            visibleFeatureCheckboxes.length.toString(),
        );
    }
}

function refreshVisibleItemClasses() {
    featureGroups.forEach((featureGroup) => {
        const items = Array.from(featureGroup.querySelectorAll<HTMLElement>(".list-group-item[data-search-text]"));
        const visibleItems = items.filter((item) => isVisible(item));

        items.forEach((item) => item.classList.remove("last-child-visible"));

        if (visibleItems.length > 0) {
            visibleItems[visibleItems.length - 1].classList.add("last-child-visible");
        }
    });
}

function applyFilters() {
    const search = (searchBox?.value ?? "").trim().toLowerCase();
    const selectedVisibility = Array.from(visibilityFilter?.selectedOptions ?? []).map((opt) => opt.value);
    const hideOnDemandFeatures = selectedVisibility.includes("on-demand");
    const hideAlwaysEnabledFeatures = selectedVisibility.includes("always-enabled");
    const selectedStatus = statusFilter?.value ?? "any";

    featureItems.forEach((featureItem) => {
        const searchText = (featureItem.dataset.searchText ?? "").toLowerCase();
        const isOnDemandFeature = featureItem.dataset.isOnDemand === "true";
        const isAlwaysEnabledFeature = featureItem.dataset.isAlwaysEnabled === "true";
        const isEnabledFeature = featureItem.dataset.isEnabled === "true";
        const matchesSearch = search === "" || searchText.includes(search);
        const shouldHideForOnDemandFilter = hideOnDemandFeatures && isOnDemandFeature;
        const shouldHideForAlwaysEnabledFilter = hideAlwaysEnabledFeatures && isAlwaysEnabledFeature;
        const matchesStatus =
            selectedStatus === "any" ||
            (selectedStatus === "enabled" && isEnabledFeature) ||
            (selectedStatus === "disabled" && !isEnabledFeature);

        featureItem.classList.toggle(
            "d-none",
            !matchesSearch || shouldHideForOnDemandFilter || shouldHideForAlwaysEnabledFilter || !matchesStatus,
        );
    });

    featureGroups.forEach((featureGroup) => {
        const hasVisibleFeatures = featureGroup.querySelector(".list-group-item[data-search-text]:not(.d-none)") !== null;

        featureGroup.classList.toggle("d-none", !hasVisibleFeatures);
        updateFeatureGroupSelectAllState(featureGroup);
    });

    refreshVisibleItemClasses();
    const visibleFeatureCount = featureItems.filter((featureItem) => isVisible(featureItem)).length;

    listAlert?.classList.toggle("d-none", visibleFeatureCount > 0);

    if (featuresSummary) {
        const summaryText = featuresSummary.dataset.summaryText ?? "";
        const totalFeatureCount = featuresSummary.dataset.totalCount ?? "0";
        featuresSummary.textContent = summaryText
            .replace("__VISIBLE__", visibleFeatureCount.toString())
            .replace("__TOTAL__", totalFeatureCount);
    }
}

searchBox?.addEventListener("keydown", (e) => {
    if (e.key === EscapeKey) {
        searchBox.value = "";
        applyFilters();
        e.preventDefault();
        return;
    }

    if (e.key === "Enter") {
        e.preventDefault();
    }
});

searchBox?.addEventListener("input", applyFilters);
searchBox?.addEventListener("search", applyFilters);

document.querySelectorAll<HTMLElement>(".selectpicker.nosubmit").forEach((element) => {
    element.addEventListener("changed.bs.select", applyFilters);
});

bulkActionLinks.forEach((link) =>
    link.addEventListener("click", (e) => {
        e.preventDefault();

        if (!bulkActionInput || !bulkActionSubmit) {
            return;
        }

        bulkActionInput.value = link.dataset.action ?? "";
        bulkActionSubmit.click();
    }),
);

badgeShowMoreButtons.forEach((button) =>
    button.addEventListener("click", (e) => {
        e.preventDefault();

        const transitiveDependencies = button.previousElementSibling;

        if (!transitiveDependencies?.classList.contains("transitive-dependencies")) {
            return;
        }

        transitiveDependencies.classList.toggle("d-none");
        button.classList.toggle("d-none");
    }),
);

featureGroupSelectAllCheckboxes.forEach((checkbox) =>
    checkbox.addEventListener("change", () => {
        const featureGroup = checkbox.closest<HTMLElement>(".feature-group");

        if (!featureGroup) {
            return;
        }

        getVisibleFeatureItemCheckboxes(featureGroup).forEach((featureCheckbox) => {
            featureCheckbox.checked = checkbox.checked;
        });

        updateFeatureGroupSelectAllState(featureGroup);
    }),
);

featureGroups.forEach((featureGroup) =>
    featureGroup.addEventListener("change", (e) => {
        const target = e.target;

        if (!(target instanceof HTMLInputElement) || target.name !== "featureIds") {
            return;
        }

        updateFeatureGroupSelectAllState(featureGroup);
    }),
);

applyFilters();
