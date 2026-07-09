// Shared by admin list pages with a #search-box filtering [data-filter-value] rows and an
// optional #list-alert shown when the filter matches nothing (IndexProfiles, UrlRewriting
// rules - both byte-identical copies of this logic before this extraction).
const initListSearchFilter = (searchBox: HTMLInputElement) => {
    const listAlert = document.getElementById("list-alert");
    const filterElements = document.querySelectorAll<HTMLElement>("[data-filter-value]");

    const clear = () => {
        filterElements.forEach((element) => {
            element.classList.remove("d-none", "first-child-visible", "last-child-visible");
        });

        if (filterElements.length > 0) {
            filterElements[0].classList.add("first-child-visible");
            filterElements[filterElements.length - 1].classList.add("last-child-visible");
        }

        listAlert?.classList.add("d-none");
    };

    searchBox.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            event.preventDefault();
        }
    });

    searchBox.addEventListener("keyup", (event) => {
        const search = searchBox.value.toLowerCase();

        if (event.key === "Escape" || search === "") {
            searchBox.value = "";
            clear();

            return;
        }

        const visibleElements: HTMLElement[] = [];

        filterElements.forEach((element) => {
            const text = element.dataset.filterValue ?? "";

            if (!text) {
                element.classList.add("d-none");

                return;
            }

            if (text.indexOf(search) > -1) {
                element.classList.remove("d-none", "first-child-visible", "last-child-visible");
                visibleElements.push(element);
            } else {
                element.classList.add("d-none");
            }
        });

        if (visibleElements.length > 0) {
            visibleElements[0].classList.add("first-child-visible");
            visibleElements[visibleElements.length - 1].classList.add("last-child-visible");
            listAlert?.classList.add("d-none");
        } else {
            listAlert?.classList.remove("d-none");
        }
    });
};

export default initListSearchFilter;
