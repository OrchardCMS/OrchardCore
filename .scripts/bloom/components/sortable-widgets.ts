/**
 * Cross-container widget drag-and-drop shared by OrchardCore.Flows' FlowPart/BagPart
 * editors and OrchardCore.Widgets' WidgetsListPart editor: each renders its widgets
 * into a "widget-template-placeholder" container, and widgets can be dragged not just
 * to reorder within one container but into any OTHER compatible container on the page
 * (e.g. moving a widget between two Flow fields, or between two zones of a
 * WidgetsListPart). Built on SortableJS (no jQuery UI): containers sharing the same
 * `groupName` are automatically connected by the library, mirroring jQuery UI
 * Sortable's `connectWith`.
 *
 * The two consumers need different side effects when a widget actually moves to a
 * different container - Flows rewrites GUID-based form field id/name prefixes so
 * ASP.NET Core's model binder maps posted fields onto the right nested collection;
 * Widgets just updates a hidden "zone" field - so those are left to the caller via
 * `onReparented` rather than baked in here. Likewise, only Flows restricts drops by
 * content type (`accepts`); Widgets accepts any widget into any zone.
 *
 * SortableJS is loaded as a global <script> via the ResourceManager (declared as a
 * "Sortable" resource dependency in each consuming view), not bundled here, so it's
 * referenced as a global rather than imported.
 */

declare const Sortable: {
    create(el: HTMLElement, options: Record<string, unknown>): unknown;
};

interface SortableWidgetsOptions {
    groupName: string;
    accepts?: (target: HTMLElement, draggedItem: HTMLElement) => boolean;
    onReparented?: (item: HTMLElement, from: HTMLElement, to: HTMLElement) => void;
}

const DROPZONE_HINT_CLASS = "widget-dropzone-hint";
// Toggled on the dragged widget's own card, mirroring the jQuery UI setup's
// highlight on the item being moved.
const DRAGGED_CLASS = "border-primary";

export default function initSortableWidgets(containerId: string, options: SortableWidgetsOptions): void {
    const container = document.getElementById(containerId);

    if (!container || typeof Sortable === "undefined") {
        return;
    }

    let sourceContainer: HTMLElement | null = null;
    let hintContainer: HTMLElement | null = null;

    const setHint = (target: HTMLElement | null) => {
        if (hintContainer === target) {
            return;
        }

        hintContainer?.classList.remove(DROPZONE_HINT_CLASS);
        hintContainer = target;
        hintContainer?.classList.add(DROPZONE_HINT_CLASS);
    };

    Sortable.create(container, {
        handle: ".widget-editor-handle",
        group: options.groupName,
        animation: 150,
        onStart: (evt: { item: HTMLElement; from: HTMLElement }) => {
            sourceContainer = evt.from;
            evt.item.querySelector(".card")?.classList.add(DRAGGED_CLASS);
        },
        // Gates which containers will even accept the drop (SortableJS's own
        // cross-group mechanism has no type-awareness), and highlights whichever
        // one the pointer is currently over, mirroring jQuery UI's over/out pair.
        onMove: (evt: { to: HTMLElement; dragged: HTMLElement }) => {
            if (options.accepts && !options.accepts(evt.to, evt.dragged)) {
                return false;
            }

            setHint(evt.to !== sourceContainer ? evt.to : null);

            return true;
        },
        onEnd: (evt: { item: HTMLElement; to: HTMLElement }) => {
            evt.item.querySelector(".card")?.classList.remove(DRAGGED_CLASS);
            setHint(null);

            if (sourceContainer && evt.to !== sourceContainer) {
                options.onReparented?.(evt.item, sourceContainer, evt.to);
                document.dispatchEvent(new CustomEvent("contentpreview:render"));
            }

            sourceContainer = null;
        },
    });
}
