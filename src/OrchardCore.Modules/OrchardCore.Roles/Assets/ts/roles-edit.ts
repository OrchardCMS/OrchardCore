const setVisible = (element: HTMLElement, visible: boolean) => {
    element.style.display = visible ? "" : "none";
};

const isVisible = (element: HTMLElement) =>
    !!(element.offsetWidth || element.offsetHeight || element.getClientRects().length);

const searchBox = document.querySelector<HTMLInputElement>("#search-box");

if (searchBox) {
    searchBox.addEventListener("keyup", (event) => {
        const search = searchBox.value.toLowerCase();

        if (event.key === "Escape" || search === "") {
            searchBox.value = "";
            document.querySelectorAll<HTMLElement>(".permissions-list").forEach((list) => {
                setVisible(list, true);
            });
            document.querySelectorAll<HTMLElement>(".permissions-list > table > tbody > tr").forEach((row) => {
                setVisible(row, true);
            });
        } else {
            document.querySelectorAll<HTMLElement>(".permissions-list > table > tbody > tr").forEach((row) => {
                const text = (row.dataset.text ?? "").toLowerCase();
                const found = text.indexOf(search) > -1;

                setVisible(row, found);

                if (found) {
                    const list = row.closest<HTMLElement>(".permissions-list");

                    if (list) {
                        setVisible(list, true);
                    }
                }
            });

            document.querySelectorAll<HTMLElement>(".permissions-list").forEach((list) => {
                const hasVisiblePermissions = Array.from(
                    list.querySelectorAll<HTMLElement>("table > tbody > tr"),
                ).some(isVisible);

                setVisible(list, hasVisiblePermissions);
            });
        }
    });
}

// Forces module scope so this file's top-level declarations don't collide (at the type-checker
// level only - Parcel already bundles each entry file standalone) with roles-index.ts's, since
// neither file otherwise has an import/export of its own.
export {};
