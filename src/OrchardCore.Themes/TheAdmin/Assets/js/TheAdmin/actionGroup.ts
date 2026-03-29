// Persists the last-used action of a [data-action-group] button group so that
// the main button switches to the alternative action on the next page load.
// The data attributes are expected to be set by module views; if they are absent
// the group is silently skipped.
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll<HTMLElement>('.btn-group[data-action-group]').forEach(initActionGroup);
    });

    function initActionGroup(group: HTMLElement) {
        const storageKey = group.dataset.actionGroup!;
        const mainBtn = group.querySelector<HTMLButtonElement>('button[type="submit"]');
        const dropdownItem = group.querySelector<HTMLButtonElement>('.dropdown-item');

        if (!mainBtn || !dropdownItem) return;

        const actionDefault = group.dataset.actionDefault!;
        const labelDefault = group.dataset.labelDefault!;
        const actionAlt = group.dataset.actionAlt!;
        const labelAlt = group.dataset.labelAlt!;
        // Short label for the dropdown item when showing the alt choice (e.g. "and continue")
        const labelAltShort = dropdownItem.textContent!.trim();

        function applyState(altIsActive: boolean) {
            if (altIsActive) {
                mainBtn!.value = actionAlt;
                mainBtn!.textContent = labelAlt;
                dropdownItem!.value = actionDefault;
                dropdownItem!.textContent = labelDefault;
            } else {
                mainBtn!.value = actionDefault;
                mainBtn!.textContent = labelDefault;
                dropdownItem!.value = actionAlt;
                dropdownItem!.textContent = labelAltShort;
            }
        }

        // Restore stored choice on page load
        applyState(localStorage.getItem(storageKey) === actionAlt);

        // On dropdown item click: store the new default, then let the form submit naturally
        dropdownItem.addEventListener('click', function () {
            localStorage.setItem(storageKey, dropdownItem!.value);
        });
    }
})();
