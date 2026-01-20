AdminIndexList = function () {

    const initialize = (selectedLabel) => {

        let searchBox = document.getElementById('search-box');

        const filterElements = document.querySelectorAll('[data-filter-value]');

        // If the user press Enter, don't submit.
        if (searchBox) {
            let searchAlert = document.getElementById('list-alert');

            searchBox.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                }
            });

            searchBox.addEventListener('keyup', e => {

                let search = e.target.value.toLowerCase();
                // On ESC, clear the search box and display all rules.
                if (e.key == 'Escape' || search == '') {
                    searchAlert.classList.add('d-none');
                    searchBox.value = '';
                    for (let i = 0; i < filterElements.length; i++) {
                        filterElements[i].classList.remove("d-none");
                        filterElements[i].classList.remove("first-child-visible");
                        filterElements[i].classList.remove("last-child-visible");
                    }

                    if (filterElements.length > 0) {
                        filterElements[0].classList.add('first-child-visible');
                        filterElements[filterElements.length - 1].classList.add('last-child-visible');
                    }
                } else {
                    let visibleElements = [];
                    for (let i = 0; i < filterElements.length; i++) {
                        let filter = filterElements[i];

                        let text = filter.getAttribute('data-filter-value');

                        if (!text) {
                            filter.classList.add("d-none");
                            continue;
                        }

                        let found = text.indexOf(search) > -1;

                        if (found) {
                            filter.classList.remove("d-none");
                            filter.classList.remove("first-child-visible");
                            filter.classList.remove("last-child-visible");
                            visibleElements.push(filter);
                        } else {
                            filter.classList.add("d-none");
                        }
                    }

                    if (visibleElements.length > 0) {
                        visibleElements[0].classList.add('first-child-visible');
                        visibleElements[visibleElements.length - 1].classList.add('last-child-visible');
                        searchAlert.classList.add('d-none');
                    } else {
                        searchAlert.classList.remove('d-none');
                    }
                }
            });
        }

        let actions = document.getElementById('actions');
        let items = document.getElementById('items');
        let filters = document.querySelectorAll('.filter');
        let selectedItems = document.getElementById('selected-items');

        function displayActionsOrFilters() {
            // Select all checked checkboxes with name 'itemIds'
            let checkedCheckboxes = document.querySelectorAll("input[type='checkbox'][name='itemIds']:checked");

            if (checkedCheckboxes.length > 1) {
                actions.classList.remove('d-none');
                for (let i = 0; i < filters.length; i++) {
                    filters[i].classList.add('d-none');
                }
                selectedItems.classList.remove('d-none');
                items.classList.add('d-none');
            } else {
                actions.classList.add('d-none');

                for (let i = 0; i < filters.length; i++) {
                    filters[i].classList.remove('d-none');
                }
                selectedItems.classList.add('d-none');
                items.classList.remove('d-none');
            }
        }

        let dropdownItems = document.querySelectorAll(".dropdown-menu .dropdown-item");

        // Add click event listeners to each dropdown item
        dropdownItems.forEach((item) => {
            // Check if the item has a data-action attribute
            if (!item.dataset.action) {
                return;
            }

            item.addEventListener("click", () => {
                // Get all checked checkboxes
                let checkedCheckboxes = document.querySelectorAll("input[type='checkbox'][name='itemIds']:checked");

                // Check if more than one checkbox is checked
                if (checkedCheckboxes.length > 1) {
                    // Get data attributes from the clicked item
                    let actionData = Object.assign({}, item.dataset);

                    confirmDialog({
                        ...actionData,
                        callback: (r) => {
                            if (r) {
                                // Set the value of the BulkAction option
                                document.querySelector("[name='Options.BulkAction']").value = actionData.action;
                                // Trigger the submit action
                                document.querySelector("[name='submit.BulkAction']").click();
                            }
                        }
                    });
                }
            });

        });

        let selectAllCtrl = document.getElementById('select-all');
        let itemsCheckboxes = document.querySelectorAll("input[type='checkbox'][name='itemIds']");

        if (selectAllCtrl) {
            selectAllCtrl.addEventListener("click", () => {
                itemsCheckboxes.forEach((checkbox) => {
                    if (checkbox !== selectAllCtrl) {
                        checkbox.checked = selectAllCtrl.checked; // Set the checked state of all checkboxes
                    }
                });

                // Update the selected items text
                updateSelectedItemsText();
                displayActionsOrFilters();
            });
        }

        // Event listener for individual checkboxes
        itemsCheckboxes.forEach((checkbox) => {
            checkbox.addEventListener("click", () => {
                let itemsCount = itemsCheckboxes.length;
                let selectedItemsCount = document.querySelectorAll("input[type='checkbox'][name='itemIds']:checked").length;

                // Update selectAllCtrl state
                selectAllCtrl.checked = selectedItemsCount === itemsCount;
                selectAllCtrl.indeterminate = selectedItemsCount > 0 && selectedItemsCount < itemsCount;

                // Update the selected items text
                updateSelectedItemsText();
                displayActionsOrFilters();
            });
        });

        // Function to update selected items text
        function updateSelectedItemsText() {
            let selectedCount = document.querySelectorAll("input[type='checkbox'][name='itemIds']:checked").length;
            selectedItems.textContent = selectedCount + ' ' + selectedLabel;
        }
    }

    return {
        initialize: initialize
    }
}();
