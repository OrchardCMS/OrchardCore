export {};

/**
 * Drag-and-drop for the Admin Menu node tree, built on SortableJS (no jQuery
 * UI / nestedSortable). Unlike Menu/Taxonomies' flat depth-tagged list, this
 * tree is rendered as genuinely nested <ol>/<li> markup (MenuItem.TreeSummary.
 * cshtml recurses into a child <ol class="menu-item-links"> per node, always
 * present even when empty) - so one SortableJS instance per <ol>, all sharing
 * the same group, is enough: dragging a node moves its whole DOM subtree
 * (and therefore its descendants) along with it, and dropping it into any
 * other node's child list reparents it there directly.
 *
 * Persistence is a single AJAX call per move (unlike Menu/Taxonomies' one
 * hidden field synced on form submit): the server is authoritative, so a
 * successful move just reloads the page rather than trying to reconcile
 * client-side state.
 */

declare const Sortable: {
    create(el: HTMLElement, options: Record<string, unknown>): unknown;
};

(function initAdminMenuNodeList() {
    const root = document.getElementById("menu");
    const treeId = root?.dataset.treeId;
    const moveNodeUrl = root?.dataset.moveNodeUrl;

    if (!root || !treeId || !moveNodeUrl || typeof Sortable === "undefined") {
        return;
    }

    function moveNode(nodeToMoveId: string, destinationNodeId: string, position: number) {
        const token = document.querySelector<HTMLInputElement>("input[name='__RequestVerificationToken']")!.value;
        const body = new URLSearchParams({
            __RequestVerificationToken: token,
            treeId: treeId!,
            nodeToMoveId,
            destinationNodeId,
            position: String(position),
        });

        fetch(moveNodeUrl!, { method: "POST", body })
            .then((response) => {
                if (!response.ok) {
                    throw new Error("Request failed");
                }

                location.reload();
            })
            .catch(() => {
                alert(document.getElementById("move-error-message")?.dataset.message);
            });
    }

    const lists = [root, ...Array.from(root.querySelectorAll<HTMLElement>(".menu-item-links"))];

    lists.forEach((list) => {
        Sortable.create(list, {
            handle: ".menu-item-title",
            draggable: "li.menu-item",
            group: "admin-menu-tree",
            animation: 150,
            onEnd: (evt: { item: HTMLElement; to: HTMLElement; newIndex?: number }) => {
                const nodeToMoveId = evt.item.dataset.treenodeId;

                if (!nodeToMoveId) {
                    return;
                }

                // The destination node is whichever [data-treenode-id] the drop
                // target list belongs to - itself for the root <ol id="menu">
                // (whose own data-treenode-id is the "content-preset" sentinel the
                // server treats as "no destination", i.e. the root), or the
                // enclosing <li> for a nested per-node child list.
                const destination = evt.to.closest<HTMLElement>("[data-treenode-id]");
                const destinationNodeId = destination?.dataset.treenodeId ?? "";

                moveNode(nodeToMoveId, destinationNodeId, evt.newIndex ?? 0);
            },
        });
    });
})();
