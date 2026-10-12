let menuItemId: string | undefined;

document.getElementById("modalMenuItems")?.addEventListener("show.bs.modal", (event) => {
    const button = (event as Event & { relatedTarget: HTMLElement | null }).relatedTarget;
    menuItemId = button?.dataset.menuitemid;
});

document.addEventListener("click", (event) => {
    const link = (event.target as HTMLElement).closest<HTMLAnchorElement>(".thumbnail-link-create");
    if (!link) {
        return;
    }

    if (menuItemId) {
        const href = link.getAttribute("href");
        link.setAttribute("href", `${href}&menuitemid=${menuItemId}`);
    }
});

export {};
