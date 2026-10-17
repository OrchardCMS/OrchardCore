// Filters the admin menu. Matching items are revealed together with the groups holding
// them, whatever their collapsed state, which is restored once the filter is cleared.
const hiddenClass = "nav-filtered-out";

let expandedStateBeforeFilter: Map<string, boolean> | null = null;

const getFilterInput = () => document.getElementById("filter") as HTMLInputElement | null;

const getMenu = () => document.getElementById("adminMenu");

const setExpanded = (collapsible: HTMLElement, expanded: boolean) => {
    collapsible.classList.toggle("show", expanded);

    if (!collapsible.id) {
        return;
    }

    document.querySelectorAll(`[data-bs-target="#${collapsible.id}"]`)
        .forEach((toggle) => toggle.setAttribute("aria-expanded", String(expanded)));
};

const captureExpandedState = (menu: HTMLElement) => {
    if (expandedStateBeforeFilter) {
        return;
    }

    expandedStateBeforeFilter = new Map<string, boolean>();

    menu.querySelectorAll<HTMLElement>("ul.collapse[id]")
        .forEach((list) => expandedStateBeforeFilter?.set(list.id, list.classList.contains("show")));
};

const restoreExpandedState = (menu: HTMLElement) => {
    if (!expandedStateBeforeFilter) {
        return;
    }

    menu.querySelectorAll<HTMLElement>("ul.collapse[id]")
        .forEach((list) => setExpanded(list, expandedStateBeforeFilter?.get(list.id) ?? false));

    expandedStateBeforeFilter = null;
};

const getTitle = (label: Element) => (label.querySelector(".title")?.textContent ?? label.textContent ?? "").toUpperCase();

const applyFilter = (term: string) => {
    const menu = getMenu();

    if (!menu) {
        return;
    }

    const filter = term.trim().toUpperCase();

    if (filter.length === 0) {
        menu.querySelectorAll("li").forEach((item) => item.classList.remove(hiddenClass));
        restoreExpandedState(menu);

        return;
    }

    captureExpandedState(menu);

    menu.querySelectorAll("li").forEach((item) => item.classList.add(hiddenClass));

    menu.querySelectorAll(".item-label").forEach((label) => {
        if (!getTitle(label).includes(filter)) {
            return;
        }

        const item = label.closest("li");

        if (!item) {
            return;
        }

        item.classList.remove(hiddenClass);

        // A matching group or item brings the items it holds along.
        item.querySelectorAll("li").forEach((child) => child.classList.remove(hiddenClass));

        let ancestor = item.parentElement?.closest("li") ?? null;

        while (ancestor) {
            ancestor.classList.remove(hiddenClass);
            ancestor = ancestor.parentElement?.closest("li") ?? null;
        }
    });

    // Matches must be visible even when they sit inside a collapsed group or item.
    menu.querySelectorAll<HTMLElement>("ul.collapse").forEach((list) => setExpanded(list, true));
};

document.addEventListener("keydown", (e) => {
    const isFilterShortcut = e.key ? e.key.toUpperCase() === "F" : e.which === 70;

    if (e.ctrlKey && e.shiftKey && isFilterShortcut) {
        getFilterInput()?.focus();
        e.preventDefault();
        e.stopPropagation();
    }
});

const filterInput = getFilterInput();

filterInput?.addEventListener("input", () => applyFilter(filterInput.value));

filterInput?.addEventListener("keydown", (e) => {
    if (e.key !== "Escape") {
        return;
    }

    filterInput.value = "";
    applyFilter("");
    e.preventDefault();
    e.stopPropagation();
});
