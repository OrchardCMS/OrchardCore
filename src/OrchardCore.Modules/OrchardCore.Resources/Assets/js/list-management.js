(function (window, document) {
    'use strict';

    const hiddenClass = 'd-none';

    const defaultOptions = {
        actionsSelector: '#actions',
        bulkActionInputName: 'Options.BulkAction',
        clientSideSearch: false,
        clientFiltersSelector: '[data-list-filter]',
        emptyAlertSelector: '#list-empty',
        filterLinksSelector: '[data-list-filter-link]',
        filterChangeSelector: '.filter-options select, .filter-options input',
        filterSelector: '.filter',
        filterSubmit: false,
        groupCheckboxSelector: '',
        groupLabelSelector: '.list-group-select-all-label',
        groupSelectAllSelector: '.list-group-select-all',
        groupTextAttribute: 'data-select-all-text',
        groupTextCountToken: '__COUNT__',
        groupTextSelector: '.list-group-select-all-text',
        groupToggleContainerSelector: '.list-group-select-all-container',
        itemInputName: 'itemIds',
        itemsSelector: '#items',
        minSelectedItems: 2,
        noResultsActionQueryName: 'suggestion',
        noResultsActionSelector: '#btnCreate',
        normalizeSearch: false,
        searchAlertSelector: '#list-alert',
        searchBoxSelector: '#search-box',
        searchDomSelector: '.list-item-search-text',
        searchFirstElementClasses: '',
        searchGroupSelector: '.list-management-group',
        searchResultSelector: '[data-filter-value]',
        searchSummarySelector: '#list-summary',
        searchSummaryTextAttribute: 'data-summary-text',
        searchSummaryTotalAttribute: 'data-total-count',
        searchTextAttribute: 'data-filter-value',
        selectAllSelector: '#select-all',
        selectedItemsSelector: '#selected-items',
        selectedLabel: 'selected',
        selectionEnabled: true,
        singleResultActionMode: 'single',
        singleResultActionSelector: '.list-item-action',
        submitBulkActionName: 'submit.BulkAction',
        submitFilterName: 'submit.Filter'
    };

    function onReady(callback) {
        if (document.readyState !== 'loading') {
            callback();
            return;
        }

        document.addEventListener('DOMContentLoaded', callback);
    }

    function toBoolean(value, defaultValue) {
        if (value === undefined) {
            return defaultValue;
        }

        return value !== 'false';
    }

    function toNumber(value, defaultValue) {
        if (value === undefined || value === '') {
            return defaultValue;
        }

        const number = Number(value);

        return Number.isNaN(number) ? defaultValue : number;
    }

    function attributeSelector(name, value) {
        return '[' + name + '="' + value.replace(/"/g, '\\"') + '"]';
    }

    function hasValue(value) {
        return value !== undefined && value !== '';
    }

    function getOptions(element, options) {
        const dataset = element.dataset;
        const config = Object.assign({}, defaultOptions, options || {});

        config.actionsSelector = dataset.actionsSelector || config.actionsSelector;
        config.bulkActionInputName = dataset.bulkActionInputName || config.bulkActionInputName;
        config.clientSideSearch = toBoolean(dataset.clientSideSearch, config.clientSideSearch);
        config.clientFiltersSelector = dataset.clientFiltersSelector || config.clientFiltersSelector;
        config.emptyAlertSelector = dataset.emptyAlertSelector || config.emptyAlertSelector;
        config.filterChangeSelector = dataset.filterChangeSelector || config.filterChangeSelector;
        config.filterLinksSelector = dataset.filterLinksSelector || config.filterLinksSelector;
        config.filterSelector = dataset.filterSelector || config.filterSelector;
        config.filterSubmit = toBoolean(dataset.filterSubmit, config.filterSubmit);
        config.groupCheckboxSelector = dataset.groupCheckboxSelector || config.groupCheckboxSelector;
        config.groupLabelSelector = dataset.groupLabelSelector || config.groupLabelSelector;
        config.groupSelectAllSelector = dataset.groupSelectAllSelector || config.groupSelectAllSelector;
        config.groupTextAttribute = dataset.groupTextAttribute || config.groupTextAttribute;
        config.groupTextCountToken = dataset.groupTextCountToken || config.groupTextCountToken;
        config.groupTextSelector = dataset.groupTextSelector || config.groupTextSelector;
        config.groupToggleContainerSelector = dataset.groupToggleContainerSelector || config.groupToggleContainerSelector;
        config.itemInputName = dataset.itemInputName || config.itemInputName;
        config.itemsSelector = dataset.itemsSelector || config.itemsSelector;
        config.minSelectedItems = toNumber(dataset.minSelectedItems, config.minSelectedItems);
        config.noResultsActionQueryName = dataset.noResultsActionQueryName || config.noResultsActionQueryName;
        config.noResultsActionSelector = dataset.noResultsActionSelector || config.noResultsActionSelector;
        config.normalizeSearch = toBoolean(dataset.normalizeSearch, config.normalizeSearch);
        config.searchAlertSelector = dataset.searchAlertSelector || config.searchAlertSelector;
        config.searchBoxSelector = dataset.searchBoxSelector || config.searchBoxSelector;
        config.searchDomSelector = dataset.searchDomSelector || config.searchDomSelector;
        config.searchFirstElementClasses = dataset.searchFirstElementClasses || config.searchFirstElementClasses;
        config.searchResultSelector = dataset.searchResultSelector || config.searchResultSelector;
        config.searchGroupSelector = dataset.searchGroupSelector || config.searchGroupSelector;
        config.searchGroupVisibleSelector = dataset.searchGroupVisibleSelector || config.searchGroupVisibleSelector || config.searchResultSelector;
        config.searchSummarySelector = dataset.searchSummarySelector || config.searchSummarySelector;
        config.searchSummaryTextAttribute = dataset.searchSummaryTextAttribute || config.searchSummaryTextAttribute;
        config.searchSummaryTotalAttribute = dataset.searchSummaryTotalAttribute || config.searchSummaryTotalAttribute;
        config.searchTextAttribute = dataset.searchTextAttribute || config.searchTextAttribute;
        config.selectAllSelector = dataset.selectAllSelector || config.selectAllSelector;
        config.selectedItemsSelector = dataset.selectedItemsSelector || config.selectedItemsSelector;
        config.selectedLabel = dataset.selectedLabel || config.selectedLabel;
        config.selectionEnabled = toBoolean(dataset.selectionEnabled, config.selectionEnabled);
        config.singleResultActionSelector = dataset.singleResultActionSelector || config.singleResultActionSelector;
        config.singleResultActionMode = dataset.singleResultActionMode || config.singleResultActionMode;
        config.submitBulkActionName = dataset.submitBulkActionName || config.submitBulkActionName;
        config.submitFilterName = dataset.submitFilterName || config.submitFilterName;

        if (!hasValue(dataset.bulkActionInputName)) {
            const bulkActionInput = Array.from(element.querySelectorAll("input[type='hidden'][name]")).find((input) => input.name === 'BulkAction' || input.name.endsWith('.BulkAction'));

            if (bulkActionInput) {
                config.bulkActionInputName = bulkActionInput.name;
            }
        }

        config.bulkActionInputSelector = attributeSelector('name', config.bulkActionInputName);
        config.submitBulkActionSelector = attributeSelector('name', config.submitBulkActionName);
        config.submitFilterSelector = attributeSelector('name', config.submitFilterName);

        const selectionControlsExist = !!(
            query(element, config.selectAllSelector) ||
            query(element, config.actionsSelector) ||
            query(element, config.selectedItemsSelector) ||
            query(element, config.submitBulkActionSelector)
        );

        if (!hasValue(dataset.itemInputName) && selectionControlsExist) {
            const itemCheckbox = Array.from(element.querySelectorAll("input[type='checkbox'][name]")).find((checkbox) => !checkbox.matches(config.selectAllSelector));

            if (itemCheckbox) {
                config.itemInputName = itemCheckbox.name;
            }
        }

        config.checkedItemSelector = "input[type='checkbox']" + attributeSelector('name', config.itemInputName) + ':checked';
        config.itemSelector = "input[type='checkbox']" + attributeSelector('name', config.itemInputName);

        if (!hasValue(dataset.selectionEnabled) && !hasValue(options && options.selectionEnabled)) {
            config.selectionEnabled = selectionControlsExist || !!query(element, config.itemSelector);
        }

        if (!hasValue(dataset.filterSubmit) && !hasValue(options && options.filterSubmit)) {
            config.filterSubmit = !!query(element, config.submitFilterSelector);
        }

        if (!hasValue(dataset.groupCheckboxSelector) && query(element, config.groupSelectAllSelector)) {
            config.groupCheckboxSelector = config.itemSelector;
        }

        return config;
    }

    function query(root, selector) {
        return selector ? root.querySelector(selector) : null;
    }

    function queryAll(root, selector) {
        return selector ? root.querySelectorAll(selector) : [];
    }

    function show(element) {
        if (element) {
            element.classList.remove(hiddenClass);
        }
    }

    function hide(element) {
        if (element) {
            element.classList.add(hiddenClass);
        }
    }

    function isVisible(element) {
        return !element.classList.contains(hiddenClass) && !!(element.offsetWidth || element.offsetHeight || element.getClientRects().length);
    }

    function getClassList(classes) {
        return classes ? classes.split(' ').filter((className) => className) : [];
    }

    function addClasses(element, classes) {
        if (classes.length > 0) {
            element.classList.add(...classes);
        }
    }

    function removeClasses(element, classes) {
        if (classes.length > 0) {
            element.classList.remove(...classes);
        }
    }

    function normalizeValue(value, config) {
        const text = String(value || '').toLowerCase();

        if (!config.normalizeSearch) {
            return text;
        }

        return text.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    }

    function getSearchText(element, config) {
        const sources = [];

        if (config.searchTextAttribute === 'textContent') {
            sources.push(element.textContent);
        } else {
            sources.push(element.getAttribute(config.searchTextAttribute));
        }

        if (config.searchDomSelector) {
            for (const searchElement of element.querySelectorAll(config.searchDomSelector)) {
                sources.push(searchElement.textContent);
            }
        }

        return normalizeValue(sources.join(' '), config);
    }

    function getActiveClientFilters(root, config) {
        const filters = [];

        for (const element of queryAll(root, config.clientFiltersSelector)) {
            const selectedOptions = element instanceof HTMLSelectElement
                ? Array.from(element.selectedOptions)
                : [element];

            for (const option of selectedOptions) {
                if (!option.value || option.value === 'all') {
                    continue;
                }

                filters.push({
                    attribute: option.dataset.listFilterAttribute || element.dataset.listFilterAttribute,
                    mode: option.dataset.listFilterMode || element.dataset.listFilterMode || 'include',
                    value: option.dataset.listFilterValue || option.value
                });
            }
        }

        for (const link of queryAll(root, config.filterLinksSelector + '.active')) {
            const value = link.dataset.listFilterValue || '';

            if (!value || value === 'all') {
                continue;
            }

            filters.push({
                attribute: link.dataset.listFilterAttribute,
                mode: link.dataset.listFilterMode || 'include',
                value: value
            });
        }

        return filters.filter((filter) => filter.attribute);
    }

    function matchesClientFilters(element, filters) {
        for (const filter of filters) {
            const value = element.getAttribute(filter.attribute) || '';
            const matches = value === filter.value;

            if ((filter.mode === 'exclude' && matches) || (filter.mode !== 'exclude' && !matches)) {
                return false;
            }
        }

        return true;
    }

    function updateSearchSummary(root, config, visibleCount) {
        const summary = query(root, config.searchSummarySelector);

        if (!summary) {
            return;
        }

        const summaryText = summary.getAttribute(config.searchSummaryTextAttribute) || '';
        const totalCount = summary.getAttribute(config.searchSummaryTotalAttribute) || '0';

        summary.textContent = summaryText
            .replace('__VISIBLE__', visibleCount.toString())
            .replace('__TOTAL__', totalCount);
    }

    function matchesSearchText(text, search) {
        if (search === '') {
            return true;
        }

        if (text.indexOf(search) > -1) {
            return true;
        }

        const keywords = search.split(/\s+/).filter((keyword) => keyword);

        return keywords.length > 1 && keywords.every((keyword) => text.indexOf(keyword) > -1);
    }

    function updateSearchGroups(root, config) {
        if (!config.searchGroupSelector) {
            return;
        }

        for (const group of queryAll(root, config.searchGroupSelector)) {
            const hasVisibleResult = Array.from(group.querySelectorAll(config.searchGroupVisibleSelector)).some((result) => !result.classList.contains(hiddenClass));

            group.classList.toggle(hiddenClass, !hasVisibleResult);
        }
    }

    function initializeSearch(root, config) {
        const searchBox = query(root, config.searchBoxSelector);

        if (!searchBox && !query(root, config.clientFiltersSelector) && !query(root, config.filterLinksSelector)) {
            return;
        }

        const searchAlert = query(root, config.searchAlertSelector);
        const submitFilterButton = query(root, config.submitFilterSelector);
        const emptyAlert = query(root, config.emptyAlertSelector);
        const extraFirstElementClasses = getClassList(config.searchFirstElementClasses);
        const filterElements = queryAll(root, config.searchResultSelector);

        function getSearchTerm() {
            return normalizeValue(searchBox ? searchBox.value.trim() : '', config);
        }

        function navigateToSearchAction() {
            const search = getSearchTerm();

            if (!search) {
                return;
            }

            const visibleResults = Array.from(queryAll(root, config.searchResultSelector)).filter(isVisible);

            if (config.singleResultActionSelector) {
                const useFirstResult = config.singleResultActionMode === 'first';
                const result = useFirstResult ? visibleResults[0] : visibleResults.length === 1 ? visibleResults[0] : null;
                const action = result ? result.querySelector(config.singleResultActionSelector) : null;

                if (action) {
                    window.location = action.getAttribute('href');
                    return;
                }
            }

            if (visibleResults.length === 0 && config.noResultsActionSelector) {
                const action = query(root, config.noResultsActionSelector);
                const href = action ? action.getAttribute('href') : null;

                if (href) {
                    window.location = href + '?' + encodeURIComponent(config.noResultsActionQueryName) + '=' + encodeURIComponent(search);
                }
            }
        }

        function update() {
            if (!config.clientSideSearch) {
                return;
            }

            const search = getSearchTerm();
            const activeFilters = getActiveClientFilters(root, config);
            const visibleElements = [];

            if (search === '' && activeFilters.length === 0) {
                hide(searchAlert);
                show(emptyAlert);
                if (searchBox) {
                    searchBox.value = '';
                }

                for (const element of filterElements) {
                    element.classList.remove(hiddenClass, 'first-child-visible', 'last-child-visible');
                    removeClasses(element, extraFirstElementClasses);
                    visibleElements.push(element);
                }
            } else {
                for (const element of filterElements) {
                    const text = getSearchText(element, config);
                    const matchesSearch = matchesSearchText(text, search);
                    const matchesFilters = matchesClientFilters(element, activeFilters);

                    element.classList.remove('first-child-visible', 'last-child-visible');
                    removeClasses(element, extraFirstElementClasses);

                    if (matchesSearch && matchesFilters) {
                        element.classList.remove(hiddenClass);
                        visibleElements.push(element);
                    } else {
                        element.classList.add(hiddenClass);
                    }
                }
            }

            if (visibleElements.length > 0) {
                visibleElements[0].classList.add('first-child-visible');
                addClasses(visibleElements[0], extraFirstElementClasses);
                visibleElements[visibleElements.length - 1].classList.add('last-child-visible');
                hide(searchAlert);
            } else if (search !== '' || activeFilters.length > 0) {
                hide(emptyAlert);
                show(searchAlert);
            }

            updateSearchGroups(root, config);
            updateSearchSummary(root, config, visibleElements.length);
            root.dispatchEvent(new CustomEvent('listmanagement:updated', { detail: { visibleCount: visibleElements.length } }));
        }

        if (searchBox) {
            searchBox.addEventListener('keydown', (event) => {
                if (event.key !== 'Enter') {
                    return;
                }

                event.preventDefault();

                if (config.clientSideSearch) {
                    navigateToSearchAction();
                    return;
                }

                if (submitFilterButton) {
                    submitFilterButton.click();
                }
            });

            searchBox.addEventListener('keyup', (event) => {
                if (event.key === 'Escape') {
                    searchBox.value = '';
                }

                update();
            });

            searchBox.addEventListener('input', update);
            searchBox.addEventListener('search', update);
        }

        for (const element of queryAll(root, config.clientFiltersSelector)) {
            element.addEventListener('change', update);
            element.addEventListener('changed.bs.select', update);
        }

        for (const link of queryAll(root, config.filterLinksSelector)) {
            link.addEventListener('click', (event) => {
                event.preventDefault();

                const attribute = link.dataset.listFilterAttribute;

                for (const item of queryAll(root, config.filterLinksSelector)) {
                    if (item.dataset.listFilterAttribute === attribute) {
                        item.classList.remove('active');
                    }
                }

                link.classList.add('active');
                update();
            });
        }

        root.addEventListener('listmanagement:update', update);
        update();
    }

    function initializeFilterSubmit(root, config) {
        if (!config.filterSubmit) {
            return;
        }

        const submitFilterButton = query(root, config.submitFilterSelector);

        if (!submitFilterButton) {
            return;
        }

        for (const element of queryAll(root, config.filterChangeSelector)) {
            element.addEventListener('change', () => submitFilterButton.click());
        }

        for (const element of queryAll(root, '.selectpicker:not(.nosubmit)')) {
            element.addEventListener('changed.bs.select', () => submitFilterButton.click());
        }
    }

    function initializeSelection(root, config) {
        if (!config.selectionEnabled) {
            return;
        }

        const actions = query(root, config.actionsSelector);
        const items = query(root, config.itemsSelector);
        const filters = queryAll(root, config.filterSelector);
        const selectAll = query(root, config.selectAllSelector);
        const selectedItems = query(root, config.selectedItemsSelector);
        const bulkActionInput = query(root, config.bulkActionInputSelector);
        const submitBulkActionButton = query(root, config.submitBulkActionSelector);

        function getItemCheckboxes() {
            return Array.from(queryAll(root, config.itemSelector));
        }

        function getSelectedCount() {
            return queryAll(root, config.checkedItemSelector).length;
        }

        function hasActiveSelection() {
            return getSelectedCount() >= config.minSelectedItems;
        }

        function updateSelection() {
            const itemCheckboxes = getItemCheckboxes();
            const selectedCount = getSelectedCount();
            const activeSelection = hasActiveSelection();

            if (selectAll) {
                selectAll.checked = itemCheckboxes.length > 0 && selectedCount === itemCheckboxes.length;
                selectAll.indeterminate = selectedCount > 0 && selectedCount < itemCheckboxes.length;
            }

            if (selectedItems) {
                selectedItems.textContent = selectedCount + ' ' + config.selectedLabel;
            }

            if (activeSelection) {
                show(actions);
                hide(items);
                show(selectedItems);

                for (const filter of filters) {
                    hide(filter);
                }
            } else {
                hide(actions);
                show(items);
                hide(selectedItems);

                for (const filter of filters) {
                    show(filter);
                }
            }
        }

        function submitBulkAction(action) {
            if (!bulkActionInput || !submitBulkActionButton) {
                return;
            }

            bulkActionInput.value = action;
            submitBulkActionButton.click();
        }

        for (const item of queryAll(root, '.dropdown-menu .dropdown-item[data-action]')) {
            item.addEventListener('click', (event) => {
                event.preventDefault();

                if (!hasActiveSelection()) {
                    return;
                }

                const actionData = Object.assign({}, item.dataset);

                if (actionData.message && typeof window.confirmDialog === 'function') {
                    window.confirmDialog(Object.assign({}, actionData, {
                        callback: (confirmed) => {
                            if (confirmed) {
                                submitBulkAction(actionData.action);
                            }
                        }
                    }));

                    return;
                }

                submitBulkAction(actionData.action);
            });
        }

        if (selectAll) {
            selectAll.addEventListener('change', () => {
                for (const checkbox of getItemCheckboxes()) {
                    if (checkbox !== selectAll) {
                        checkbox.checked = selectAll.checked;
                    }
                }

                updateSelection();
            });
        }

        for (const checkbox of getItemCheckboxes()) {
            checkbox.addEventListener('change', updateSelection);
        }

        updateSelection();
    }

    function initializeGroupSelection(root, config) {
        if (!config.groupSelectAllSelector || !config.searchGroupSelector || !config.groupCheckboxSelector) {
            return;
        }

        function getVisibleGroupCheckboxes(group) {
            return Array.from(group.querySelectorAll(config.groupCheckboxSelector)).filter((checkbox) => {
                const item = checkbox.closest(config.searchResultSelector);

                return item && !item.classList.contains(hiddenClass);
            });
        }

        function updateGroup(group) {
            const groupSelectAll = group.querySelector(config.groupSelectAllSelector);
            const groupToggleContainer = config.groupToggleContainerSelector ? group.querySelector(config.groupToggleContainerSelector) : null;

            if (!groupSelectAll) {
                return;
            }

            const visibleCheckboxes = getVisibleGroupCheckboxes(group);
            const hasVisibleCheckboxes = visibleCheckboxes.length > 0;
            const allVisibleChecked = hasVisibleCheckboxes && visibleCheckboxes.every((checkbox) => checkbox.checked);
            const anyVisibleChecked = visibleCheckboxes.some((checkbox) => checkbox.checked);

            if (groupToggleContainer) {
                groupToggleContainer.classList.toggle(hiddenClass, !hasVisibleCheckboxes);

                let nextListItem = groupToggleContainer.closest('li')?.nextElementSibling;
                while (nextListItem && nextListItem.tagName !== 'LI') {
                    nextListItem = nextListItem.nextElementSibling;
                }

                if (nextListItem) {
                    nextListItem.classList.toggle('first-child-visible', !hasVisibleCheckboxes);
                }
            }

            groupSelectAll.disabled = !hasVisibleCheckboxes;
            groupSelectAll.checked = allVisibleChecked;
            groupSelectAll.indeterminate = anyVisibleChecked && !allVisibleChecked;

            const groupLabel = config.groupLabelSelector ? group.querySelector(config.groupLabelSelector) : null;
            const groupText = config.groupTextSelector ? group.querySelector(config.groupTextSelector) : null;

            if (groupLabel && groupText) {
                const text = groupLabel.getAttribute(config.groupTextAttribute) || '';
                groupText.textContent = text.replace(config.groupTextCountToken, visibleCheckboxes.length.toString());
            }
        }

        function updateGroups() {
            for (const group of queryAll(root, config.searchGroupSelector)) {
                updateGroup(group);
            }
        }

        for (const groupSelectAll of queryAll(root, config.groupSelectAllSelector)) {
            groupSelectAll.addEventListener('change', () => {
                const group = groupSelectAll.closest(config.searchGroupSelector);

                if (!group) {
                    return;
                }

                for (const checkbox of getVisibleGroupCheckboxes(group)) {
                    checkbox.checked = groupSelectAll.checked;
                }

                updateGroup(group);
            });
        }

        for (const group of queryAll(root, config.searchGroupSelector)) {
            group.addEventListener('change', (event) => {
                const target = event.target;

                if (!(target instanceof HTMLInputElement) || !target.matches(config.groupCheckboxSelector)) {
                    return;
                }

                updateGroup(group);
            });
        }

        root.addEventListener('listmanagement:updated', updateGroups);
        updateGroups();
    }

    function initialize(element, options) {
        const config = getOptions(element, options);

        initializeSearch(element, config);
        initializeFilterSubmit(element, config);
        initializeSelection(element, config);
        initializeGroupSelection(element, config);
    }

    window.listManagement = {
        initialize: initialize
    };

    onReady(() => {
        for (const element of document.querySelectorAll('[data-list-management]')) {
            initialize(element);
        }
    });
})(window, document);
