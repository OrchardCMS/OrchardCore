import initBulkSelectList from "@orchardcore/bloom/components/bulk-select-list";

const root = document.querySelector<HTMLElement>(".bulk-select-list");

if (root) {
    initBulkSelectList(root);
}
