urlRewritingAdmin = function () {

    const initialize = (url, errorLabel, selectedLabel) => {

        // Create the sortable UI.
        sortingListManager.create('#rewrite-rules-sortable-list', url, errorLabel);

        let searchBox = document.getElementById('search-box');
        let searchAlert = document.getElementById('list-alert');

        const filterElements = document.querySelectorAll('[data-filter-value]');

        // If the user press Enter, don't submit.
        searchBox.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
            }
        });

        searchBox.addEventListener('keyup', e => {

            var search = e.target.value.toLowerCase();
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

                    var text = filter.getAttribute('data-filter-value');

                    if (!text) {
                        filter.classList.add("d-none");
                        continue;
                    }

                    var found = text.indexOf(search) > -1;

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

        var actions = document.getElementById('actions');
        var items = document.getElementById('items');
        var filters = document.querySelectorAll('.filter');
        var selectAllCtrl = document.getElementById('select-all');
        var selectedItems = document.getElementById('selected-items');
        var itemsCheckboxes = document.querySelectorAll("input[type='checkbox'][name='ruleIds']");

        function displayActionsOrFilters() {
            // Select all checked checkboxes with name 'ruleIds'
            var checkedCheckboxes = document.querySelectorAll("input[type='checkbox'][name='ruleIds']:checked");

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

        var dropdownItems = document.querySelectorAll(".dropdown-menu .dropdown-item");

        // Add click event listeners to each dropdown item
        dropdownItems.forEach(function (item) {
            // Check if the item has a data-action attribute
            if (item.dataset.action) {
                item.addEventListener("click", function () {
                    // Get all checked checkboxes
                    var checkedCheckboxes = document.querySelectorAll("input[type='checkbox'][name='ruleIds']:checked");

                    // Check if more than one checkbox is checked
                    if (checkedCheckboxes.length > 1) {
                        // Get data attributes from the clicked item
                        var actionData = Object.assign({}, item.dataset);

                        confirmDialog({
                            ...actionData,
                            callback: function (r) {
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
            }
        });

        selectAllCtrl.addEventListener("click", function () {
            itemsCheckboxes.forEach(function (checkbox) {
                if (checkbox !== selectAllCtrl) {
                    checkbox.checked = selectAllCtrl.checked; // Set the checked state of all checkboxes
                }
            });

            // Update the selected items text
            updateSelectedItemsText();
            displayActionsOrFilters();
        });

        // Event listener for individual checkboxes
        itemsCheckboxes.forEach(function (checkbox) {
            checkbox.addEventListener("click", function () {
                var itemsCount = itemsCheckboxes.length;
                var selectedItemsCount = document.querySelectorAll("input[type='checkbox'][name='ruleIds']:checked").length;

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
            var selectedCount = document.querySelectorAll("input[type='checkbox'][name='ruleIds']:checked").length;
            selectedItems.textContent = selectedCount + ' ' + selectedLabel;
        }
    }

    return {
        initialize: initialize
    }
}();
