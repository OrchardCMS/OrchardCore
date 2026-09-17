const isVisible = (element: HTMLElement) =>
    !!(element.offsetWidth || element.offsetHeight || element.getClientRects().length);

const searchBox = document.querySelector<HTMLInputElement>("#search-box");

if (searchBox) {
    searchBox.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            const visible = Array.from(document.querySelectorAll<HTMLElement>(".roles > ul > li")).filter(isVisible);

            if (visible.length === 1) {
                const href = visible[0].querySelector<HTMLAnchorElement>(".edit")?.getAttribute("href");

                if (href) {
                    window.location.href = href;
                }
            }

            event.preventDefault();
            event.stopPropagation();
        }
    });

    searchBox.addEventListener("keyup", (event) => {
        const search = searchBox.value.toLowerCase();
        const elementsToFilter = document.querySelectorAll<HTMLElement>("[data-filter-value]");

        if (event.key === "Escape" || search === "") {
            searchBox.value = "";
            elementsToFilter.forEach((element) => {
                element.classList.remove("d-none", "first-child-visible", "last-child-visible");
            });
        } else {
            elementsToFilter.forEach((element) => {
                const text = (element.dataset.filterValue ?? "").toLowerCase();
                const found = text.indexOf(search) > -1;

                if (found) {
                    element.classList.remove("d-none", "first-child-visible", "last-child-visible");
                } else {
                    element.classList.add("d-none");
                }
            });

            const visibleElements = Array.from(elementsToFilter).filter(
                (element) => !element.classList.contains("d-none"),
            );

            if (visibleElements.length > 0) {
                visibleElements[0].classList.add("first-child-visible");
                visibleElements[visibleElements.length - 1].classList.add("last-child-visible");
            }

            const visible = Array.from(document.querySelectorAll<HTMLElement>(".roles > ul > li")).filter(isVisible);

            document.getElementById("list-alert")?.classList.toggle("d-none", visible.length !== 0);
        }
    });
}

// Forces module scope so this file's top-level declarations don't collide (at the type-checker
// level only - Parcel already bundles each entry file standalone) with roles-edit.ts's, since
// neither file otherwise has an import/export of its own.
export {};
