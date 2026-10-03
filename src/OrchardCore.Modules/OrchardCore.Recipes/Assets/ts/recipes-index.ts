const isVisible = (element: HTMLElement) =>
    !!(element.offsetWidth || element.offsetHeight || element.getClientRects().length);

const searchBox = document.querySelector<HTMLInputElement>("#search-box");

if (searchBox) {
    searchBox.addEventListener("keyup", (event) => {
        const search = searchBox.value.toLowerCase();
        const elementsToFilter = document.querySelectorAll<HTMLElement>("[data-filter-value]");

        if (event.key === "Escape" || search === "") {
            searchBox.value = "";
            elementsToFilter.forEach((element) => {
                element.style.display = "";
            });
        } else {
            elementsToFilter.forEach((element) => {
                const text = (element.dataset.filterValue ?? "").toLowerCase();
                const found = text.indexOf(search) > -1;

                element.style.display = found ? "" : "none";
            });

            const visible = Array.from(document.querySelectorAll<HTMLElement>(".recipe-group > ul > li")).filter(
                isVisible,
            );
            const listAlert = document.getElementById("list-alert");

            if (listAlert) {
                listAlert.classList.toggle("d-none", visible.length !== 0);
            }
        }
    });
}
