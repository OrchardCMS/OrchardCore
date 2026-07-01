/*
 * Taxonomy term tree admin editor: nested drag-and-drop using SortableJS (no jQuery UI).
 *
 * Mirrors how the original jQuery UI nestedSortable felt to use: dragging up/down
 * reorders an item within its current list (each list is its own independent
 * Sortable instance - dragging never reparents by hovering into a nested list,
 * which turned out to be an unreliable target no matter how it was sized/tuned).
 * Dragging sideways past a threshold changes nesting depth instead: drag right to
 * nest the item under whichever item it now sits below, drag left to move it back
 * out to its parent's level. The tree is serialized into the hidden "Hierarchy"
 * field on every change, using the same { index, children } shape the
 * server-side TaxonomyPart display driver parses.
 */
(function () {
    'use strict';

    var DRAGGING_CLASS = 'menu-dragging';
    var ITEM_SELECTOR = 'li.menu-item';
    var LIST_SELECTOR = 'ol.menu-item-links';
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

    function trackPointer(e) {
        var point = e.touches ? e.touches[0] : e;
        pointerX = point.clientX;
    }

    document.addEventListener('mousemove', trackPointer);
    document.addEventListener('touchmove', trackPointer);
    document.addEventListener('dragover', trackPointer);

    function directItems(list) {
        return Array.prototype.filter.call(list.children, function (child) {
            return child.matches(ITEM_SELECTOR);
        });
    }

    // Ensures every item exposes a direct child <ol> so that nesting another
    // item under it has somewhere to go. Empty lists are collapsed via CSS.
    function ensureChildList(item) {
        var list = item.querySelector(':scope > ' + LIST_SELECTOR);

        if (!list) {
            list = document.createElement('ol');
            list.className = 'menu-item menu-item-links';
            item.appendChild(list);
            attach(list);
        }

        return list;
    }

    // Walks the nested <ol>/<li> DOM and emits [{ index, children? }], where
    // "index" is each item's data-index attribute. Items without children omit
    // the children array. The original data-index values are preserved; the new
    // order and nesting are conveyed purely by tree position.
    function serialize(list) {
        return directItems(list).map(function (item) {
            var node = { index: item.dataset.index };
            var childList = item.querySelector(':scope > ' + LIST_SELECTOR);

            if (childList && directItems(childList).length > 0) {
                node.children = serialize(childList);
            }

            return node;
        });
    }

    function onChange() {
        confirmLeave = true;

        if (hierarchyInput) {
            hierarchyInput.value = JSON.stringify(serialize(menu));
        }
    }

    // Nests the item under the item immediately above it (in whatever list it
    // currently sits in). No-op - and returns false - if there's no such item.
    function indent(item) {
        var previous = item.previousElementSibling;

        if (!previous || !previous.matches(ITEM_SELECTOR)) {
            return false;
        }

        ensureChildList(previous).appendChild(item);

        return true;
    }

    // Moves the item out to its parent's level, right after the parent. No-op
    // - and returns false - if the item is already at the root level.
    function outdent(item) {
        var parentItem = item.parentElement.closest(ITEM_SELECTOR);

        if (!parentItem) {
            return false;
        }

        parentItem.parentElement.insertBefore(item, parentItem.nextElementSibling);

        return true;
    }

    function attach(list) {
        window.Sortable.create(list, {
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
            onStart: function () {
                document.body.classList.add(DRAGGING_CLASS);
                dragStartX = pointerX;
            },
            onEnd: function (evt) {
                document.body.classList.remove(DRAGGING_CLASS);

                var deltaX = (pointerX !== null && dragStartX !== null) ? pointerX - dragStartX : 0;
                dragStartX = null;

                var reparented = false;

                if (deltaX > INDENT_THRESHOLD) {
                    reparented = indent(evt.item);
                } else if (deltaX < -INDENT_THRESHOLD) {
                    reparented = outdent(evt.item);
                }

                if (reparented || evt.oldIndex !== evt.newIndex) {
                    onChange();
                }
            }
        });
    }

    Array.prototype.forEach.call(menu.querySelectorAll(ITEM_SELECTOR), ensureChildList);

    attach(menu);
    Array.prototype.forEach.call(menu.querySelectorAll(LIST_SELECTOR), attach);

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
