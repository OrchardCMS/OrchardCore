// Shared by Workflow/Index and WorkflowType/Index - both are bulk-select-list pages, but with
// two real differences from the generic bulk-select-list component (Wave 5): they build the
// confirmDialog payload from just {title, message} rather than the full dataset spread, and
// WorkflowType skips confirmation entirely for its "Export" bulk action. Kept as its own small
// component rather than extending the widely-used generic one, to avoid any risk to its other
// 9 consumers for a shape only these 2 files need.
const show = (element: HTMLElement | null) => {
    if (element) {
        element.style.display = "";
    }
};

const hide = (element: HTMLElement | null) => {
    if (element) {
        element.style.display = "none";
    }
};

export interface WorkflowBulkSelectListOptions {
    selectedText: string;
    onDropdownItemClick: (item: HTMLElement, setBulkAction: (action: string) => void) => void;
}

const initWorkflowBulkSelectList = (options: WorkflowBulkSelectListOptions) => {
    const selectAllCtrl = document.getElementById("select-all") as HTMLInputElement | null;

    if (!selectAllCtrl) {
        return;
    }

    const submitFilterButton = document.querySelector<HTMLElement>("[name='submit.Filter']");
    const bulkActionInput = document.querySelector<HTMLInputElement>("[name='Options.BulkAction']");
    const submitBulkActionButton = document.querySelector<HTMLElement>("[name='submit.BulkAction']");
    const actions = document.getElementById("actions");
    const items = document.getElementById("items");
    const filters = document.querySelectorAll<HTMLElement>(".filter");
    const selectedItems = document.getElementById("selected-items");
    const itemsCheckboxes = document.querySelectorAll<HTMLInputElement>("input[type='checkbox'][name='itemIds']");

    const selectedItemsCount = () =>
        document.querySelectorAll("input[type='checkbox'][name='itemIds']:checked").length;

    const setBulkAction = (action: string) => {
        if (bulkActionInput && submitBulkActionButton) {
            bulkActionInput.value = action;
            submitBulkActionButton.click();
        }
    };

    const displayActionsOrFilters = () => {
        if (selectedItemsCount() > 1) {
            show(actions);
            filters.forEach(hide);
            show(selectedItems);
            hide(items);
        } else {
            hide(actions);
            filters.forEach(show);
            hide(selectedItems);
            show(items);
        }
    };

    document.querySelectorAll<HTMLElement>(".selectpicker").forEach((element) => {
        element.addEventListener("changed.bs.select", () => submitFilterButton?.click());
    });

    document.querySelectorAll<HTMLElement>(".dropdown-menu .dropdown-item").forEach((item) => {
        if (!item.dataset.action) {
            return;
        }

        item.addEventListener("click", () => {
            if (selectedItemsCount() > 1) {
                options.onDropdownItemClick(item, setBulkAction);
            }
        });
    });

    selectAllCtrl.addEventListener("click", () => {
        itemsCheckboxes.forEach((checkbox) => {
            checkbox.checked = selectAllCtrl.checked;
        });
        if (selectedItems) {
            selectedItems.textContent = `${selectedItemsCount()} ${options.selectedText}`;
        }
        displayActionsOrFilters();
    });

    itemsCheckboxes.forEach((checkbox) => {
        checkbox.addEventListener("click", () => {
            const itemsCount = document.querySelectorAll("input[type='checkbox'][name='itemIds']").length;
            const selectedCount = selectedItemsCount();

            selectAllCtrl.checked = selectedCount === itemsCount;
            selectAllCtrl.indeterminate = selectedCount > 0 && selectedCount < itemsCount;

            if (selectedItems) {
                selectedItems.textContent = `${selectedCount} ${options.selectedText}`;
            }
            displayActionsOrFilters();
        });
    });
};

export default initWorkflowBulkSelectList;
