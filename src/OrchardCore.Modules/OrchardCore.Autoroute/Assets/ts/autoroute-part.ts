import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

// Scoped to the toggle's own editor (its id carries the editor's prefix), so that it also works for
// editors rendered more than once on a page or added later, e.g. in a Bag part.
observeAndInit("[data-autoroute-disabled-toggle]", (element) => {
    const toggle = element as HTMLInputElement;

    toggle.addEventListener("change", () => {
        document.querySelectorAll<HTMLElement>(`[data-autoroute-disabled-for="${CSS.escape(toggle.id)}"]`).forEach((target) => {
            target.style.display = toggle.checked ? "none" : "";
        });
    });
});
