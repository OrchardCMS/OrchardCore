import removeDiacritics from "@orchardcore/bloom/helpers/removeDiacritics";

const modalConditions = document.getElementById("modalConditions");

if (modalConditions) {
    modalConditions.addEventListener("show.bs.modal", (event) => {
        const button = (event as { relatedTarget?: HTMLElement }).relatedTarget;
        const conditionGroupId = button?.dataset.conditionGroupId ?? "";

        document.querySelectorAll<HTMLElement>(".condition-thumbnail").forEach((thumbnail) => {
            thumbnail.addEventListener(
                "click",
                function (this: HTMLElement) {
                    window.location.href = `${this.dataset.targetUrl}&conditionGroupId=${conditionGroupId}`;
                },
                { once: true },
            );
        });
    });

    modalConditions.addEventListener("shown.bs.modal", () => {
        document.getElementById("search-box")?.focus();
    });
}

const searchBox = document.getElementById("search-box") as HTMLInputElement | null;

if (searchBox) {
    searchBox.addEventListener("keyup", function (this: HTMLInputElement) {
        const search = removeDiacritics(this.value.toLowerCase());

        if (search === "") {
            searchBox.value = "";
            document.querySelectorAll<HTMLElement>(".item").forEach((item) => {
                item.style.display = "";
            });
        } else {
            document.querySelectorAll<HTMLElement>(".item").forEach((item) => {
                let titleElement: Element | null | undefined;

                Array.from(item.children).some((child) => {
                    titleElement = child.firstElementChild;
                    return !!titleElement;
                });

                const title = titleElement?.innerHTML ?? "";
                const filter = removeDiacritics(title.toLowerCase());

                item.style.display = filter.indexOf(search) > -1 ? "" : "none";
            });
        }
    });
}
