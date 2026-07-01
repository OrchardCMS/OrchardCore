export {};

declare const Sortable: {
    create(el: HTMLElement, options: Record<string, unknown>): unknown;
};

interface CellSize {
    width: number;
    height: number;
    gap: { width: number; height: number };
}

interface Size {
    width: number;
    height: number;
}

interface Snap {
    x: number;
    y: number;
}

declare global {
    interface Window {
        initDashboard: (updateUrl: string) => void;
    }
}

const HELPER_CLASS = "dashboard-resize-helper";
const HANDLE_TYPES = ["s", "e", "se"] as const;

function snapTo(original: Size, updated: Size, gridSize: CellSize): Snap {
    let snapX = original.width < updated.width ? Math.ceil(updated.width / gridSize.width) : Math.floor(updated.width / gridSize.width);
    if (snapX === 0) {
        snapX = 1;
    }

    let snapY = original.height < updated.height ? Math.ceil(updated.height / gridSize.height) : Math.floor(updated.height / gridSize.height);
    if (snapY === 0) {
        snapY = 1;
    }

    return { x: snapX, y: snapY };
}

window.initDashboard = function initDashboard(updateUrl: string) {
    const container = document.getElementById("container");

    if (!container || typeof Sortable === "undefined") {
        return;
    }

    // Bootstrap dropdown menus are detached to <body> and absolutely positioned
    // under their toggle while open, so they aren't clipped by the dashboard
    // grid's own overflow/scroll containers.
    let openMenu: HTMLElement | null = null;
    let openMenuParent: Element | null = null;

    // Starts at a reasonable default and is only ever recalculated on window
    // resize (matching the previous jQuery UI setup), not on initial load.
    let cellSize: CellSize = { width: 240, height: 240, gap: { width: 16, height: 16 } };

    function calculateCellSize() {
        const styles = window.getComputedStyle(container!);
        const rows = styles.getPropertyValue("grid-template-rows");
        const columns = styles.getPropertyValue("grid-template-columns");
        const rowGap = parseFloat(styles.getPropertyValue("grid-row-gap"));
        const columnGap = parseFloat(styles.getPropertyValue("grid-column-gap"));

        cellSize = {
            width: parseFloat(columns.split(" ")[0]),
            height: parseFloat(rows.split(" ")[0]),
            gap: { width: columnGap, height: rowGap },
        };
    }

    function wrappers() {
        return Array.from(container!.querySelectorAll<HTMLElement>(".dashboard-wrapper"));
    }

    function setUndoValues() {
        wrappers().forEach((wrapper, index) => {
            const id = wrapper.id;
            (document.getElementById(`undo_${id}_Position`) as HTMLInputElement | null)!.value = String(index);
            (document.getElementById(`undo_${id}_Height`) as HTMLInputElement | null)!.value = wrapper.style.getPropertyValue("--dashboard-height");
            (document.getElementById(`undo_${id}_Width`) as HTMLInputElement | null)!.value = wrapper.style.getPropertyValue("--dashboard-width");
        });
    }

    function getCurrentValues() {
        return wrappers().map((wrapper, index) => ({
            ContentItemId: wrapper.id,
            Position: index,
            Width: wrapper.style.getPropertyValue("--dashboard-width"),
            Height: wrapper.style.getPropertyValue("--dashboard-height"),
        }));
    }

    function update(metadata: ReturnType<typeof getCurrentValues>) {
        const token = (document.querySelector<HTMLInputElement>("[name='__RequestVerificationToken']"))!.value;
        const body = new URLSearchParams({ __RequestVerificationToken: token });
        metadata.forEach((part, index) => {
            (Object.keys(part) as Array<keyof typeof part>).forEach((key) => {
                body.append(`parts[${index}].${key}`, String(part[key]));
            });
        });

        fetch(updateUrl, { method: "POST", body })
            .then((response) => {
                if (!response.ok) {
                    throw new Error("Request failed");
                }

                document.getElementById("dashboard-undo-message")!.classList.remove("d-none");
            })
            .catch(() => {
                document.getElementById("dashboard-undo-message")!.classList.add("d-none");
            });
    }

    document.getElementById("dashboard-undo-message")!.querySelector("a")?.addEventListener("click", (e) => {
        e.preventDefault();
        (document.getElementById("dashboard-undo-form") as HTMLFormElement).submit();
    });

    Sortable.create(container, {
        handle: ".dashboard-handle",
        animation: 150,
        // The ghost keeps the dragged item's own --dashboard-width/-height (it's
        // the real element, left in place at its current slot), so it naturally
        // spans the right number of grid cells; only its look needs to change to
        // an empty dashed placeholder box (see .sortable-ghost in dashboard.scss).
        onStart: () => {
            if (openMenu) {
                document.dispatchEvent(new MouseEvent("click", { bubbles: true }));
            }

            setUndoValues();
        },
        onEnd: (evt: { item: HTMLElement }) => {
            const previousPosition = (document.getElementById(`undo_${evt.item.id}_Position`) as HTMLInputElement).value;
            const newPosition = wrappers().indexOf(evt.item);

            if (Number(previousPosition) !== newPosition) {
                update(getCurrentValues());
            }
        },
    });

    function beginResize(wrapper: HTMLElement, type: (typeof HANDLE_TYPES)[number], startEvent: PointerEvent) {
        startEvent.preventDefault();

        if (openMenu) {
            document.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        }

        setUndoValues();

        const startX = startEvent.clientX;
        const startY = startEvent.clientY;
        const rect = wrapper.getBoundingClientRect();
        const original: Size = { width: rect.width, height: rect.height };

        const helper = document.createElement("div");
        helper.className = HELPER_CLASS;
        helper.style.position = "fixed";
        helper.style.top = `${rect.top}px`;
        helper.style.left = `${rect.left}px`;
        helper.style.width = `${rect.width}px`;
        helper.style.height = `${rect.height}px`;
        document.body.appendChild(helper);

        let snap: Snap = snapTo(original, original, cellSize);

        const onMove = (moveEvent: PointerEvent) => {
            const updated: Size = {
                width: type === "s" ? original.width : original.width + (moveEvent.clientX - startX),
                height: type === "e" ? original.height : original.height + (moveEvent.clientY - startY),
            };

            snap = snapTo(original, updated, cellSize);

            helper.style.setProperty("--dashboard-width", String(snap.x));
            helper.style.setProperty("--dashboard-height", String(snap.y));
            helper.style.width = `${(snap.x * cellSize.width) + ((snap.x - 1) * cellSize.gap.width)}px`;
            helper.style.height = `${(snap.y * cellSize.height) + ((snap.y - 1) * cellSize.gap.height)}px`;
        };

        const onUp = () => {
            document.removeEventListener("pointermove", onMove);
            document.removeEventListener("pointerup", onUp);
            helper.remove();

            wrapper.style.setProperty("--dashboard-width", String(snap.x));
            wrapper.style.setProperty("--dashboard-height", String(snap.y));

            update(getCurrentValues());
        };

        document.addEventListener("pointermove", onMove);
        document.addEventListener("pointerup", onUp);
    }

    wrappers().forEach((wrapper) => {
        HANDLE_TYPES.forEach((type) => {
            const handle = document.createElement("div");
            handle.className = `dashboard-resize-handle dashboard-resize-handle-${type}`;
            handle.addEventListener("pointerdown", (e) => beginResize(wrapper, type, e));
            wrapper.appendChild(handle);
        });
    });

    window.addEventListener("show.bs.dropdown", (e) => {
        const target = e.target as HTMLElement;
        const menu = target.querySelector<HTMLElement>(".dropdown-menu");

        if (!menu) {
            return;
        }

        openMenu = menu;
        openMenuParent = menu.parentElement;
        document.body.appendChild(menu);

        const targetRect = target.getBoundingClientRect();
        menu.style.display = "block";
        menu.style.position = "absolute";
        menu.style.top = `${targetRect.bottom + window.scrollY}px`;
        menu.style.left = `${targetRect.left + window.scrollX}px`;
    });

    window.addEventListener("hide.bs.dropdown", () => {
        if (openMenu && openMenuParent) {
            openMenuParent.appendChild(openMenu);
            openMenu.style.display = "none";
        }

        openMenu = null;
        openMenuParent = null;
    });

    window.addEventListener("resize", calculateCellSize);
};
