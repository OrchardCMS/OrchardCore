/**
 * Nested drag-and-drop hierarchy editor shared by OrchardCore.Menu (menu items)
 * and OrchardCore.Taxonomies (taxonomy terms), built on SortableJS (no jQuery UI).
 *
 * The whole tree is rendered as ONE FLAT list of <li data-depth="N"> siblings
 * (see MenuItem.Admin.cshtml / Content.TermAdmin.cshtml) rather than as actual
 * nested <ol> elements per item. This is what lets a single SortableJS instance
 * handle the entire tree: dragging vertically reorders/reparents freely across
 * the whole tree (moving an item from one parent's children to a different
 * parent's children is just "drag it near the new spot"), while dragging
 * sideways past a threshold changes nesting depth explicitly - right to nest
 * under whichever item ends up directly before it, left to move up a level.
 * When a dragged item has its own descendants, they're temporarily detached
 * for the duration of the drag and reinserted right after it, at the same
 * relative depths, once it's dropped - so the whole subtree moves as a unit.
 *
 * The tree is serialized into the hidden "Hierarchy" field on every change,
 * using the { index, children } shape the server-side MenuPart / TaxonomyPart
 * display drivers both parse identically, reconstructed from the flat depth
 * sequence.
 *
 * SortableJS is loaded as a global <script> via the ResourceManager (declared
 * as a "Sortable" resource dependency in each consuming view), not bundled
 * here, so it's referenced as a global rather than imported.
 */

declare const Sortable: {
    create(el: HTMLElement, options: Record<string, unknown>): unknown;
};

interface TreeNode {
    index: string;
    children?: TreeNode[];
}

const DRAGGING_CLASS = "menu-dragging";
const ITEM_SELECTOR = "li.menu-item";
const PENDING_SHIFT_CLASS = "menu-item-pending-shift";
const PENDING_PARENT_CLASS = "menu-item-pending-parent";
const FALLBACK_CLONE_SELECTOR = ".sortable-fallback";
const INDENT_REM = 1.5;
// How far (in px) the pointer must move horizontally, relative to where the
// drag started, before it's treated as an intentional request to indent or
// outdent rather than incidental drift during an up/down reorder.
const INDENT_THRESHOLD = 40;

export default function initSortableMenu(): void {
    const menu = document.getElementById("menu");

    if (!menu || typeof Sortable === "undefined") {
        return;
    }

    const hierarchyInput = document.getElementById(menu.dataset.hierarchyInput ?? "") as HTMLInputElement | null;
    const leaveMessage = menu.dataset.leaveMessage ?? "";
    let confirmLeave = false;
    let dragStartX: number | null = null;
    let pointerX: number | null = null;
    let draggedItem: HTMLElement | null = null;
    let draggedOriginalDepth = 0;
    let detachedBlock: HTMLElement[] = [];

    const depthOf = (item: HTMLElement) => parseInt(item.dataset.depth ?? "0", 10) || 0;

    const applyDepth = (item: HTMLElement, depth: number) => {
        item.dataset.depth = String(depth);
        item.style.marginLeft = `${depth * INDENT_REM}rem`;
    };

    // Walking backward from an item, the nearest preceding item shallower than
    // "depth" is its parent at that depth - the flat sequence never skips more
    // than one level per step (indent/outdent only ever move one level at a
    // time), so the first shallower item found is exactly one level up.
    const findAncestorAt = (item: HTMLElement, depth: number) => {
        let el = item.previousElementSibling as HTMLElement | null;

        while (el) {
            if (depthOf(el) < depth) {
                return el;
            }

            el = el.previousElementSibling as HTMLElement | null;
        }

        return null;
    };

    // The deepest depth a valid drop at this position could ever be: at most
    // one level deeper than whatever item now immediately precedes it.
    const maxDepthAfter = (precedingItem: HTMLElement | null) => (precedingItem ? depthOf(precedingItem) + 1 : 0);

    // Combines the item's original depth (clamped to whatever's still valid at
    // its current position) with any explicit sideways indent/outdent request.
    const computeTargetDepth = (precedingItem: HTMLElement | null, deltaX: number) => {
        const maxDepth = maxDepthAfter(precedingItem);
        const baseDepth = Math.min(draggedOriginalDepth, maxDepth);
        const adjustment = deltaX > INDENT_THRESHOLD ? 1 : deltaX < -INDENT_THRESHOLD ? -1 : 0;

        return Math.max(0, Math.min(baseDepth + adjustment, maxDepth));
    };

    const clearPendingPreview = () => {
        document.querySelectorAll(`.${PENDING_SHIFT_CLASS}`).forEach((el) => {
            el.classList.remove(PENDING_SHIFT_CLASS);
            (el as HTMLElement).style.removeProperty("--pending-shift");
        });
        document.querySelectorAll(`.${PENDING_PARENT_CLASS}`).forEach((el) => {
            el.classList.remove(PENDING_PARENT_CLASS);
        });
    };

    // Shows, in real time as the pointer moves during a drag, what letting go
    // right now would do: shift the dragged item (and its drag clone, which is
    // what's actually visible while forceFallback is in effect) sideways and
    // highlight whichever item would become its new parent, mirroring how
    // jQuery UI's nestedSortable gave live feedback while dragging.
    const updatePendingPreview = () => {
        if (!draggedItem || dragStartX === null || pointerX === null) {
            return;
        }

        clearPendingPreview();

        const deltaX = pointerX - dragStartX;
        const targetDepth = computeTargetDepth(draggedItem.previousElementSibling as HTMLElement | null, deltaX);

        if (targetDepth === draggedOriginalDepth) {
            return;
        }

        const shift = `translateX(${(targetDepth - draggedOriginalDepth) * INDENT_REM}rem)`;
        const clone = document.querySelector(FALLBACK_CLONE_SELECTOR) as HTMLElement | null;

        draggedItem.style.setProperty("--pending-shift", shift);
        draggedItem.classList.add(PENDING_SHIFT_CLASS);

        if (clone) {
            clone.style.setProperty("--pending-shift", shift);
            clone.classList.add(PENDING_SHIFT_CLASS);
        }

        const newParent = targetDepth > 0 ? findAncestorAt(draggedItem, targetDepth) : null;

        if (newParent) {
            newParent.classList.add(PENDING_PARENT_CLASS);
        }
    };

    const trackPointer = (e: MouseEvent | TouchEvent | DragEvent) => {
        const point = "touches" in e && e.touches.length > 0 ? e.touches[0] : (e as MouseEvent | DragEvent);
        pointerX = point.clientX;
        updatePendingPreview();
    };

    // Recorded on the actual mousedown/touchstart that precedes a drag, rather
    // than read back from `pointerX` inside Sortable's onStart: onStart only
    // fires once Sortable's own threshold-crossing check has processed some
    // number of queued mousemove events, which can lag behind the real start
    // of the gesture under load - reading `pointerX` at that point could pick
    // up a position well past where the drag actually began, undercounting
    // the horizontal distance dragged and silently dropping indent/outdent
    // requests. Capturing it here, at the gesture's actual origin, keeps that
    // distance accurate regardless of how long Sortable takes to catch up.
    const trackPointerDown = (e: MouseEvent | TouchEvent) => {
        const point = "touches" in e && e.touches.length > 0 ? e.touches[0] : (e as MouseEvent);
        dragStartX = point.clientX;
    };

    document.addEventListener("mousemove", trackPointer);
    document.addEventListener("touchmove", trackPointer);
    document.addEventListener("dragover", trackPointer);
    document.addEventListener("mousedown", trackPointerDown, true);
    document.addEventListener("touchstart", trackPointerDown, true);

    // Walks the flat <li data-depth> sequence and rebuilds [{ index, children? }],
    // where "index" is each item's data-index attribute (a stable pointer back to
    // its original position server-side, unrelated to its new position/depth
    // here) and nesting is reconstructed purely from the depth values.
    const serialize = (): TreeNode[] => {
        const root: TreeNode[] = [];
        const stack: { children: TreeNode[]; depth: number }[] = [{ children: root, depth: -1 }];

        menu.querySelectorAll(ITEM_SELECTOR).forEach((el) => {
            const item = el as HTMLElement;
            const depth = depthOf(item);
            const node: TreeNode = { index: item.dataset.index ?? "", children: [] };

            while (stack.length > 1 && stack[stack.length - 1].depth >= depth) {
                stack.pop();
            }

            stack[stack.length - 1].children.push(node);
            stack.push({ children: node.children as TreeNode[], depth });
        });

        const prune = (nodes: TreeNode[]) => {
            nodes.forEach((node) => {
                if (node.children && node.children.length > 0) {
                    prune(node.children);
                } else {
                    delete node.children;
                }
            });
        };

        prune(root);

        return root;
    };

    const onChange = () => {
        confirmLeave = true;

        if (hierarchyInput) {
            hierarchyInput.value = JSON.stringify(serialize());
        }
    };

    Sortable.create(menu, {
        handle: ".menu-item-title",
        draggable: ITEM_SELECTOR,
        // Keep links, buttons and form controls clickable while still allowing
        // a drag to start from the item title.
        filter: "a, button, input, select, textarea, [data-bs-toggle]",
        preventOnFilter: false,
        // Force the mouse/touch-simulated drag rather than native HTML5 DnD:
        // native drag stops firing "mousemove" for the page, which is needed
        // to track horizontal movement for the indent/outdent gesture below.
        forceFallback: true,
        fallbackOnBody: true,
        animation: 150,
        onStart: (evt: { item: HTMLElement }) => {
            document.body.classList.add(DRAGGING_CLASS);
            draggedItem = evt.item;
            draggedOriginalDepth = depthOf(draggedItem);
            detachedBlock = [];

            // Detach the dragged item's descendants (contiguous following
            // siblings deeper than it) for the duration of the drag, so they
            // don't end up scattered by the reordering animation - they're
            // reinserted, at the same relative depths, right after wherever
            // the item ends up once dropped.
            let sibling = draggedItem.nextElementSibling as HTMLElement | null;

            while (sibling && depthOf(sibling) > draggedOriginalDepth) {
                const next = sibling.nextElementSibling as HTMLElement | null;
                detachedBlock.push(sibling);
                sibling.remove();
                sibling = next;
            }
        },
        onEnd: () => {
            document.body.classList.remove(DRAGGING_CLASS);
            clearPendingPreview();

            const deltaX = pointerX !== null && dragStartX !== null ? pointerX - dragStartX : 0;
            dragStartX = null;

            if (!draggedItem) {
                return;
            }

            const targetDepth = computeTargetDepth(draggedItem.previousElementSibling as HTMLElement | null, deltaX);
            const depthShift = targetDepth - draggedOriginalDepth;

            applyDepth(draggedItem, targetDepth);

            let insertionPoint: HTMLElement = draggedItem;

            detachedBlock.forEach((item) => {
                applyDepth(item, depthOf(item) + depthShift);
                insertionPoint.after(item);
                insertionPoint = item;
            });

            draggedItem = null;
            detachedBlock = [];

            onChange();
        },
    });

    window.addEventListener("beforeunload", (e) => {
        if (confirmLeave) {
            e.preventDefault();
            e.returnValue = leaveMessage;
        }
    });

    const form = menu.closest("form");

    if (form) {
        form.addEventListener("submit", () => {
            confirmLeave = false;
        });
    }
}
