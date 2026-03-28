(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.btn-group[data-action-group]').forEach(initActionGroup);
    });

    function initActionGroup(group) {
        var storageKey = group.dataset.actionGroup;
        var mainBtn = group.querySelector('button[type="submit"]');
        var dropdownItem = group.querySelector('.dropdown-item');

        if (!mainBtn || !dropdownItem) return;

        var actionDefault = group.dataset.actionDefault;
        var labelDefault = group.dataset.labelDefault;
        var actionAlt = group.dataset.actionAlt;
        var labelAlt = group.dataset.labelAlt;
        // Short label for the dropdown item when showing the alt choice (e.g. "and continue")
        var labelAltShort = dropdownItem.textContent.trim();

        function applyState(altIsActive) {
            if (altIsActive) {
                mainBtn.value = actionAlt;
                mainBtn.textContent = labelAlt;
                dropdownItem.value = actionDefault;
                dropdownItem.textContent = labelDefault;
            } else {
                mainBtn.value = actionDefault;
                mainBtn.textContent = labelDefault;
                dropdownItem.value = actionAlt;
                dropdownItem.textContent = labelAltShort;
            }
        }

        // Restore stored choice on page load
        applyState(localStorage.getItem(storageKey) === actionAlt);

        // On dropdown item click: store the new default, then let the form submit naturally
        dropdownItem.addEventListener('click', function () {
            localStorage.setItem(storageKey, dropdownItem.value);
        });
    }
})();
