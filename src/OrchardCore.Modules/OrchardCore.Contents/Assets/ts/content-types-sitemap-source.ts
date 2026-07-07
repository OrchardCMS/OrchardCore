// Uses plain document-wide id lookups throughout (not scoped to a container element), matching
// the original inline script's own implicit assumption that exactly one ContentTypesSitemapSource
// editor is ever on the page at a time (its fixed ids, e.g. "index-all-row", would collide across
// multiple instances anyway).
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

const indexAllCheckbox = document.querySelector<HTMLInputElement>(".content-types-sitemap-index-all");
const limitItemsCheckbox = document.querySelector<HTMLInputElement>(".content-types-sitemap-limit-items");

if (indexAllCheckbox && limitItemsCheckbox) {
    const switchContentTypesCheckBoxes = () => {
        const indexAll = indexAllCheckbox.checked;
        const limitItems = limitItemsCheckbox.checked;

        if (indexAll) {
            showElement(document.getElementById("index-all-row"));
            hideElement(document.getElementById("index-selected-row"));
            hideElement(document.getElementById("index-limit-row"));
            hideElement(document.getElementById("ContentTypesSitemapSource_LimitItems_Container"));
        } else {
            hideElement(document.getElementById("index-all-row"));
            showElement(document.getElementById("ContentTypesSitemapSource_LimitItems_Container"));

            if (limitItems) {
                hideElement(document.getElementById("index-selected-row"));
                showElement(document.getElementById("index-limit-row"));
            } else {
                showElement(document.getElementById("index-selected-row"));
                hideElement(document.getElementById("index-limit-row"));
            }
        }
    };

    document.querySelectorAll<HTMLInputElement>(".content-type-checkbox").forEach((checkbox) => {
        checkbox.addEventListener("click", (event) => {
            if (!(event.target instanceof HTMLInputElement)) {
                return;
            }

            const listItem = event.target.closest(".list-group-item");
            const selected = event.target.checked;

            listItem
                ?.querySelectorAll<HTMLInputElement | HTMLSelectElement>("select, input:not(.content-type-checkbox)")
                .forEach((el) => {
                    el.disabled = !selected;

                    if ("readOnly" in el) {
                        el.readOnly = !selected;
                    }
                });
        });
    });

    document.querySelectorAll<HTMLInputElement>(".content-type-radio").forEach((radio) => {
        radio.addEventListener("click", (event) => {
            if (!(event.target instanceof HTMLInputElement)) {
                return;
            }

            const selectedContentType = event.target.value;

            document.querySelectorAll<HTMLElement>("#index-limit-row .list-group-item").forEach((listItem) => {
                const contentType = listItem.querySelector<HTMLInputElement>(".content-type-radio")?.value;
                const matches = contentType === selectedContentType;

                listItem.querySelectorAll<HTMLInputElement>("input:not(.content-type-radio)").forEach((el) => {
                    el.readOnly = !matches;
                });

                listItem.querySelectorAll<HTMLSelectElement>("select").forEach((el) => {
                    el.disabled = !matches;
                });
            });
        });
    });

    indexAllCheckbox.addEventListener("change", switchContentTypesCheckBoxes);
    limitItemsCheckbox.addEventListener("change", switchContentTypesCheckBoxes);
}
