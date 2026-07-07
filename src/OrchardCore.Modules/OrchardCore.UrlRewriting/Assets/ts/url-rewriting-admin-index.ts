import initBulkSelectList from "@orchardcore/bloom/components/bulk-select-list";
import initListSearchFilter from "@orchardcore/bloom/components/list-search-filter";

const root = document.querySelector<HTMLElement>(".bulk-select-list");

if (root) {
    initBulkSelectList(root);

    const sortUrl = root.dataset.sortUrl;
    const sortErrorMessage = root.dataset.sortErrorMessage;

    if (sortUrl) {
        sortingListManager.create(`#${root.id}`, sortUrl, sortErrorMessage);
    }
}

const searchBox = document.querySelector<HTMLInputElement>("#search-box");

if (searchBox) {
    initListSearchFilter(searchBox);
}
