document.addEventListener("keydown", (e) => {
    const isFilterShortcut = e.key ? e.key.toUpperCase() === "F" : e.which === 70;

    if (e.ctrlKey && e.shiftKey && isFilterShortcut) {
        document.getElementById("filter")?.focus();
        e.preventDefault();
        e.stopPropagation();
    }
});

function hasChild(list: Element) {
    const filter = (document.getElementById("filter") as HTMLInputElement | null)?.value.toUpperCase() ?? "";

    Array.from(list.children)
        .filter((item) => item.matches("li"))
        .forEach((item) => {
            const hasMatchingSpan = Array.from(item.querySelectorAll("span")).some(
                (span) => (span.textContent ?? "").toUpperCase().indexOf(filter) >= 0,
            );

            if (hasMatchingSpan) {
                const firstNestedListItem = item.querySelector("ul li");

                if (firstNestedListItem?.parentElement) {
                    (item as HTMLElement).style.display = "";
                    hasChild(firstNestedListItem.parentElement);
                } else {
                    (item as HTMLElement).style.display = "";
                }
            } else {
                (item as HTMLElement).style.display = "none";
            }
        });
}

document.getElementById("filter")?.addEventListener("keyup", (e) => {
    const list = document.getElementById("adminMenu");
    const filter = (document.getElementById("filter") as HTMLInputElement | null)?.value;

    if (filter && list) {
        hasChild(list);
    } else {
        list?.querySelectorAll<HTMLElement>("li").forEach((item) => {
            item.style.display = "";
        });
    }
    e.preventDefault();
    e.stopPropagation();
});
