// The DisplayDriver prefixes generated ids (ElasticsearchIndexProfileDisplayDriver's
// default prefix is its model type name, "IndexProfile"), so a hardcoded
// getElementById("SearchType") never matches - use an attribute selector against the
// field-name suffix instead. "DefaultQueryContainer"/"DefaultQueryFields" are plain
// unprefixed id="" on <div>s, not asp-for targets, so they're unaffected and stay
// getElementById lookups.
const menu = document.querySelector<HTMLSelectElement>("select[id$='SearchType']");
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
