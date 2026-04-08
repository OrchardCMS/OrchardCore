import { getAdminPreferences, setAdminPreferences } from '../constants';
import { AdminPreferences } from './userPreferencesPersistor';

// Persists the last-used action of a [data-action-group] button group so that
// the main button switches to the alternative action on the next page load.
// The data attributes are expected to be set by module views; if they are absent
// the group is silently skipped.
// State is stored inside the tenant-scoped admin preferences managed by userPreferencesPersistor.

// Called from TheAdmin-main.ts (head bundle) to apply the stored state before the first
// paint, avoiding a visible flicker when the alt action was last used.
export const actionGroupLoader = () => {
    const adminPreferences = getAdminPreferences();
    if (!adminPreferences?.actionGroups) return;

    const applyActionGroupState = (group: HTMLElement) => {
        const storageKey = group.dataset.actionGroup!;
        const storedAction = (adminPreferences.actionGroups as Record<string, string>)[storageKey];
        if (storedAction !== group.dataset.actionAlt) return;

        const mainBtn = group.querySelector<HTMLButtonElement>('button[type="submit"]');
        const dropdownItem = group.querySelector<HTMLButtonElement>('.dropdown-item');
        if (!mainBtn || !dropdownItem) return;

        mainBtn.value = group.dataset.actionAlt!;
        mainBtn.textContent = group.dataset.labelAlt!;
        dropdownItem.value = group.dataset.actionDefault!;
        dropdownItem.textContent = group.dataset.labelDefault!;
    };

    const processNode = (node: Node) => {
        if (!(node instanceof HTMLElement)) return;
        if (node.matches('.btn-group[data-action-group]')) {
            applyActionGroupState(node);
        }
        node.querySelectorAll<HTMLElement>('.btn-group[data-action-group]').forEach(applyActionGroupState);
    };

    const observer = new MutationObserver(mutations => {
        for (const mutation of mutations) {
            for (const node of mutation.addedNodes) {
                processNode(node);
            }
        }
    });

    observer.observe(document.documentElement, { childList: true, subtree: true });
    document.addEventListener('DOMContentLoaded', () => observer.disconnect());
};

// Wire up click handlers at DOMContentLoaded. The guard prevents double-initialisation
// when both the head bundle (TheAdmin-main) and the foot bundle (TheAdmin) include this module.
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll<HTMLElement>('.btn-group[data-action-group]').forEach(initActionGroup);
});

function initActionGroup(group: HTMLElement) {
    if (group.dataset.actionGroupInit !== undefined) return;
    group.dataset.actionGroupInit = '';

    const storageKey = group.dataset.actionGroup!;
    const dropdownItem = group.querySelector<HTMLButtonElement>('.dropdown-item');
    if (!dropdownItem) return;

    // On dropdown item click: store the new default, then let the form submit naturally
    dropdownItem.addEventListener('click', () => {
        const prefs = getAdminPreferences() as AdminPreferences;
        prefs.actionGroups = prefs.actionGroups ?? {};
        prefs.actionGroups[storageKey] = dropdownItem.value;
        setAdminPreferences(prefs);
    });
}
