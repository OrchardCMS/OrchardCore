// Shared by every admin list page with select-all + per-row checkboxes + a bulk-action
// dropdown (ContentsAdminList, UsersAdminList, NotificationsAdminList, Sitemaps' 2 lists,
// Placements, Shortcodes, Tenants' 2 lists). All of them wire the exact same fixed ids/classes
// (#actions, #items, #selected-items, #select-all, .filter, .dropdown-menu .dropdown-item) -
// the only per-view variance is the checkbox name, the bulk-action field name, and the
// localized "selected" text, all read from data-* attributes on the root element.
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

const initBulkSelectList = (root: HTMLElement) => {
    const checkboxName = root.dataset.checkboxName ?? "itemIds";
    const bulkActionName = root.dataset.bulkActionName ?? "Options.BulkAction";
    const selectedText = root.dataset.selectedText ?? "";

    const selectAllCtrl = document.getElementById("select-all") as HTMLInputElement | null;

    if (!selectAllCtrl) {
        return;
    }

    const submitFilterButton = document.querySelector<HTMLElement>("[name='submit.Filter']");
    const submitBulkActionButton = document.querySelector<HTMLElement>("[name='submit.BulkAction']");
    const bulkActionInput = document.querySelector<HTMLInputElement>(`[name='${bulkActionName}']`);
    const actions = document.getElementById("actions");
    const items = document.getElementById("items");
    const filters = document.querySelectorAll<HTMLElement>(".filter");
    const selectedItems = document.getElementById("selected-items");
    const itemsCheckboxes = document.querySelectorAll<HTMLInputElement>(
        `input[type='checkbox'][name='${checkboxName}']`,
    );

    const selectedItemsCount = () =>
        document.querySelectorAll(`input[type='checkbox'][name='${checkboxName}']:checked`).length;

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

    document.querySelectorAll<HTMLElement>(".selectpicker:not(.nosubmit)").forEach((element) => {
        element.addEventListener("changed.bs.select", () => submitFilterButton?.click());
    });

    document.querySelectorAll<HTMLElement>(".dropdown-menu .dropdown-item").forEach((item) => {
        if (!item.dataset.action) {
            return;
        }

        item.addEventListener("click", function (this: HTMLElement) {
            if (selectedItemsCount() > 1) {
                confirmDialog({
                    ...this.dataset,
                    callback: (response: boolean) => {
                        if (response && bulkActionInput) {
                            bulkActionInput.value = this.dataset.action ?? "";
                            submitBulkActionButton?.click();
                        }
                    },
                });
            }
        });
    });

    selectAllCtrl.addEventListener("click", () => {
        itemsCheckboxes.forEach((checkbox) => {
            if (checkbox !== selectAllCtrl) {
                checkbox.checked = selectAllCtrl.checked;
            }
        });
        if (selectedItems) {
            selectedItems.textContent = `${selectedItemsCount()} ${selectedText}`;
        }
        displayActionsOrFilters();
    });

    itemsCheckboxes.forEach((checkbox) => {
        checkbox.addEventListener("click", () => {
            const itemsCount = itemsCheckboxes.length;
            const count = selectedItemsCount();

            selectAllCtrl.checked = count === itemsCount;
            selectAllCtrl.indeterminate = count > 0 && count < itemsCount;

            if (selectedItems) {
                selectedItems.textContent = `${count} ${selectedText}`;
            }
            displayActionsOrFilters();
        });
    });
};

export default initBulkSelectList;
