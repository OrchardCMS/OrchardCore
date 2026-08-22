const searchBox = document.querySelector<HTMLInputElement>("#search-box");

searchBox?.addEventListener("keyup", (event) => {
    const search = searchBox.value.toLowerCase();
    const elementsToFilter = document.querySelectorAll<HTMLElement>("[data-cacheentry]");

    if (event.key === "Escape" || search === "") {
        searchBox.value = "";
        elementsToFilter.forEach((element) => {
            element.classList.remove("d-none", "first-child-visible", "last-child-visible");
        });
    } else {
        document.querySelectorAll<HTMLElement>("#cache-entries > li").forEach((element) => {
            const cacheEntry = (element.dataset.cacheentry ?? "").toLowerCase();
            const found = cacheEntry.indexOf(search) > -1;

            if (found) {
                element.classList.remove("d-none", "first-child-visible", "last-child-visible");
            } else {
                element.classList.add("d-none");
            }
        });

        const visibleElements = Array.from(elementsToFilter).filter(
            (element) => !element.classList.contains("d-none"),
        );

        if (visibleElements.length) {
            visibleElements[0].classList.add("first-child-visible");
            visibleElements[visibleElements.length - 1].classList.add("last-child-visible");
        }

        if (document.querySelectorAll("#cache-entries > li:not(.d-none)").length === 0) {
            document.getElementById("search-alert")?.classList.remove("d-none");
            document.getElementById("none-alert")?.classList.add("d-none");
        } else {
            document.getElementById("search-alert")?.classList.add("d-none");
        }
    }
});
