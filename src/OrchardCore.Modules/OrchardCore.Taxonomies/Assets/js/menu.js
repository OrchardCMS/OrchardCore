/*
 * Taxonomy term tree admin editor: nested drag-and-drop using SortableJS (no jQuery UI).
 *
 * The whole tree is rendered as ONE FLAT list of <li data-depth="N"> siblings
 * (see Content.TermAdmin.cshtml) rather than as actual nested <ol> elements per
 * item. This is what lets a single SortableJS instance handle the entire tree:
 * dragging vertically reorders/reparents freely across the whole tree (moving
 * an item from one parent's children to a different parent's children is just
 * "drag it near the new spot"), while dragging sideways past a threshold
 * changes nesting depth explicitly - right to nest under whichever item ends
 * up directly before it, left to move up a level. When a dragged item has its
 * own descendants, they're temporarily detached for the duration of the drag
 * and reinserted right after it, at the same relative depths, once it's
 * dropped - so the whole subtree moves as a unit.
 *
 * The tree is serialized into the hidden "Hierarchy" field on every change,
 * using the same { index, children } shape the server-side TaxonomyPart
 * display driver parses, reconstructed from the flat depth sequence.
 */
(function () {
    'use strict';

    var DRAGGING_CLASS = 'menu-dragging';
    var ITEM_SELECTOR = 'li.menu-item';
    var PENDING_SHIFT_CLASS = 'menu-item-pending-shift';
    var PENDING_PARENT_CLASS = 'menu-item-pending-parent';
    var FALLBACK_CLONE_SELECTOR = '.sortable-fallback';
    var INDENT_REM = 1.5;
    // How far (in px) the pointer must move horizontally, relative to where the
    // drag started, before it's treated as an intentional request to indent or
    // outdent rather than incidental drift during an up/down reorder.
    var INDENT_THRESHOLD = 40;

    var menu = document.getElementById('menu');

    if (!menu || typeof window.Sortable === 'undefined') {
        return;
    }

    var hierarchyInput = document.getElementById(menu.dataset.hierarchyInput);
    var leaveMessage = menu.dataset.leaveMessage;
    var confirmLeave = false;
    var dragStartX = null;
    var pointerX = null;
    var draggedItem = null;
    var draggedOriginalDepth = null;
    var detachedBlock = [];

    function depthOf(item) {
        return parseInt(item.dataset.depth, 10) || 0;
    }

    function applyDepth(item, depth) {
        item.dataset.depth = depth;
        item.style.marginLeft = (depth * INDENT_REM) + 'rem';
    }

    // Walking backward from an item, the nearest preceding item shallower than
    // "depth" is its parent at that depth - the flat sequence never skips more
    // than one level per step (indent/outdent only ever move one level at a
    // time), so the first shallower item found is exactly one level up.
    function findAncestorAt(item, depth) {
        var el = item.previousElementSibling;

        while (el) {
            if (depthOf(el) < depth) {
                return el;
            }

            el = el.previousElementSibling;
        }

        return null;
    }

    // The deepest depth a valid drop at this position could ever be: at most
    // one level deeper than whatever item now immediately precedes it.
    function maxDepthAfter(precedingItem) {
        return precedingItem ? depthOf(precedingItem) + 1 : 0;
    }

    // Combines the item's original depth (clamped to whatever's still valid at
    // its current position) with any explicit sideways indent/outdent request.
    function computeTargetDepth(precedingItem, deltaX) {
        var maxDepth = maxDepthAfter(precedingItem);
        var baseDepth = Math.min(draggedOriginalDepth, maxDepth);
        var adjustment = deltaX > INDENT_THRESHOLD ? 1 : (deltaX < -INDENT_THRESHOLD ? -1 : 0);

        return Math.max(0, Math.min(baseDepth + adjustment, maxDepth));
    }

    function clearPendingPreview() {
        Array.prototype.forEach.call(document.querySelectorAll('.' + PENDING_SHIFT_CLASS), function (el) {
            el.classList.remove(PENDING_SHIFT_CLASS);
            el.style.removeProperty('--pending-shift');
        });
        Array.prototype.forEach.call(document.querySelectorAll('.' + PENDING_PARENT_CLASS), function (el) {
            el.classList.remove(PENDING_PARENT_CLASS);
        });
    }

    // Shows, in real time as the pointer moves during a drag, what letting go
    // right now would do: shift the dragged item (and its drag clone, which is
    // what's actually visible while forceFallback is in effect) sideways and
    // highlight whichever item would become its new parent, mirroring how
    // jQuery UI's nestedSortable gave live feedback while dragging.
    function updatePendingPreview() {
        if (!draggedItem || dragStartX === null || pointerX === null) {
            return;
        }

        clearPendingPreview();

        var deltaX = pointerX - dragStartX;
        var targetDepth = computeTargetDepth(draggedItem.previousElementSibling, deltaX);

        if (targetDepth === draggedOriginalDepth) {
            return;
        }

        var shift = 'translateX(' + ((targetDepth - draggedOriginalDepth) * INDENT_REM) + 'rem)';
        var clone = document.querySelector(FALLBACK_CLONE_SELECTOR);

        draggedItem.style.setProperty('--pending-shift', shift);
        draggedItem.classList.add(PENDING_SHIFT_CLASS);

        if (clone) {
            clone.style.setProperty('--pending-shift', shift);
            clone.classList.add(PENDING_SHIFT_CLASS);
        }

        var newParent = targetDepth > 0 ? findAncestorAt(draggedItem, targetDepth) : null;

        if (newParent) {
            newParent.classList.add(PENDING_PARENT_CLASS);
        }
    }

    function trackPointer(e) {
        var point = e.touches ? e.touches[0] : e;
        pointerX = point.clientX;
        updatePendingPreview();
    }

    document.addEventListener('mousemove', trackPointer);
    document.addEventListener('touchmove', trackPointer);
    document.addEventListener('dragover', trackPointer);

    // Walks the flat <li data-depth> sequence and rebuilds [{ index, children? }],
    // where "index" is each item's data-index attribute (a stable pointer back to
    // its original position server-side, unrelated to its new position/depth
    // here) and nesting is reconstructed purely from the depth values.
    function serialize() {
        var root = [];
        var stack = [{ children: root, depth: -1 }];

        Array.prototype.forEach.call(menu.querySelectorAll(ITEM_SELECTOR), function (item) {
            var depth = depthOf(item);
            var node = { index: item.dataset.index, children: [] };

            while (stack.length > 1 && stack[stack.length - 1].depth >= depth) {
                stack.pop();
            }

            stack[stack.length - 1].children.push(node);
            stack.push({ children: node.children, depth: depth });
        });

        (function prune(nodes) {
            nodes.forEach(function (node) {
                if (node.children.length > 0) {
                    prune(node.children);
                } else {
                    delete node.children;
                }
            });
        })(root);

        return root;
    }

    function onChange() {
        confirmLeave = true;

        if (hierarchyInput) {
            hierarchyInput.value = JSON.stringify(serialize());
        }
    }

    window.Sortable.create(menu, {
        handle: '.menu-item-title',
        draggable: ITEM_SELECTOR,
        // Keep links, buttons and form controls clickable while still allowing
        // a drag to start from the item title.
        filter: 'a, button, input, select, textarea, [data-bs-toggle]',
        preventOnFilter: false,
        // Force the mouse/touch-simulated drag rather than native HTML5 DnD:
        // native drag stops firing "mousemove" for the page, which is needed
        // to track horizontal movement for the indent/outdent gesture below.
        forceFallback: true,
        fallbackOnBody: true,
        animation: 150,
        onStart: function (evt) {
            document.body.classList.add(DRAGGING_CLASS);
            dragStartX = pointerX;
            draggedItem = evt.item;
            draggedOriginalDepth = depthOf(draggedItem);
            detachedBlock = [];

            // Detach the dragged item's descendants (contiguous following
            // siblings deeper than it) for the duration of the drag, so they
            // don't end up scattered by the reordering animation - they're
            // reinserted, at the same relative depths, right after wherever
            // the item ends up once dropped.
            var sibling = draggedItem.nextElementSibling;

            while (sibling && depthOf(sibling) > draggedOriginalDepth) {
                var next = sibling.nextElementSibling;
                detachedBlock.push(sibling);
                sibling.remove();
                sibling = next;
            }
        },
        onEnd: function () {
            document.body.classList.remove(DRAGGING_CLASS);
            clearPendingPreview();

            var deltaX = (pointerX !== null && dragStartX !== null) ? pointerX - dragStartX : 0;
            dragStartX = null;

            var targetDepth = computeTargetDepth(draggedItem.previousElementSibling, deltaX);
            var depthShift = targetDepth - draggedOriginalDepth;

            applyDepth(draggedItem, targetDepth);

            var insertionPoint = draggedItem;

            detachedBlock.forEach(function (item) {
                applyDepth(item, depthOf(item) + depthShift);
                insertionPoint.after(item);
                insertionPoint = item;
            });

            draggedItem = null;
            draggedOriginalDepth = null;
            detachedBlock = [];

            onChange();
        }
    });

    window.addEventListener('beforeunload', function (e) {
        if (confirmLeave) {
            e.preventDefault();
            e.returnValue = leaveMessage;
            return leaveMessage;
        }
    });

    var form = menu.closest('form');

    if (form) {
        form.addEventListener('submit', function () {
            confirmLeave = false;
        });
    }
})();
