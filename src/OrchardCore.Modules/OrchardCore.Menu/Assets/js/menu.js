/*
 * Menu admin editor: nested drag-and-drop using SortableJS (no jQuery UI).
 *
 * Builds the menu tree with one Sortable instance per nested <ol>, all sharing a
 * single group so items can be reordered and reparented to any depth. The tree
 * is serialized into the hidden "Hierarchy" field on every move, using the same
 * { index, children } shape the server-side MenuPart display driver parses.
 */
(function () {
    'use strict';

    var DRAGGING_CLASS = 'menu-dragging';
    var ITEM_SELECTOR = 'li.menu-item';
    var LIST_SELECTOR = 'ol.menu-item-links';
    var GROUP = 'menu-items';

    var menu = document.getElementById('menu');

    if (!menu || typeof window.Sortable === 'undefined') {
        return;
    }

    var hierarchyInput = document.getElementById(menu.dataset.hierarchyInput);
    var leaveMessage = menu.dataset.leaveMessage;
    var confirmLeave = false;
    var sortables = [];

    function directItems(list) {
        return Array.prototype.filter.call(list.children, function (child) {
            return child.matches(ITEM_SELECTOR);
        });
    }

    // Ensures every item exposes a direct child <ol> so that leaf items are valid
    // drop parents. Empty lists are collapsed at rest via CSS (see menu.scss).
    function ensureChildList(item) {
        var list = item.querySelector(':scope > ' + LIST_SELECTOR);

        if (!list) {
            list = document.createElement('ol');
            list.className = 'menu-item menu-item-links';
            item.appendChild(list);
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

    function attach(list) {
        sortables.push(window.Sortable.create(list, {
            group: GROUP,
            handle: '.menu-item-title',
            draggable: ITEM_SELECTOR,
            // Keep links, buttons and form controls clickable while still allowing
            // a drag to start from the item title.
            filter: 'a, button, input, select, textarea, [data-bs-toggle]',
            preventOnFilter: false,
            fallbackOnBody: true,
            swapThreshold: 0.65,
            emptyInsertThreshold: 5,
            animation: 150,
            onStart: function () {
                document.body.classList.add(DRAGGING_CLASS);
            },
            onMove: function (evt) {
                // Never allow dropping an item inside its own subtree.
                return !evt.dragged.contains(evt.to);
            },
            onEnd: function (evt) {
                document.body.classList.remove(DRAGGING_CLASS);

                var moved = evt.from !== evt.to || evt.oldIndex !== evt.newIndex;

                if (!moved) {
                    return;
                }

                confirmLeave = true;

                if (hierarchyInput) {
                    hierarchyInput.value = JSON.stringify(serialize(menu));
                }
            }
        }));
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
