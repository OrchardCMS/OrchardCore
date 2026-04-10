import { getAdminPreferences, setAdminPreferences } from '../constants';
import { AdminPreferences } from './userPreferencesPersistor';

// Keeps --oc-action-bar-height in sync with the actual rendered height
// of the fixed mobile action bar so that the edit container always has
// enough bottom padding to prevent content from being covered.
(function () {
    const actionBar = document.querySelector<HTMLElement>('.action-bar');
    if (!actionBar) return;

    new ResizeObserver(() => {
        document.documentElement.style.setProperty('--oc-action-bar-height', actionBar.offsetHeight + 'px');
    }).observe(actionBar);

    // Persist the collapse state to admin preferences so it survives page navigation.
    // Bootstrap updates aria-expanded on the toggle synchronously on every show/hide,
    // so a MutationObserver on that single attribute is the most reliable hook.
    const toggle = actionBar.querySelector<HTMLButtonElement>('.action-bar-toggle');
    if (toggle) {
        // If actionBarCollapseLoader (head bundle) pre-expanded the toggle, Bootstrap's
        // Collapse plugin has no matching .show class on the targets yet — so its first
        // click would call show() instead of hide(), wasting one click. Sync the state now.
        if (toggle.getAttribute('aria-expanded') === 'true') {
            actionBar.querySelectorAll<HTMLElement>('.action-bar-collapsible').forEach(el => {
                el.classList.add('show');
            });
        }

        new MutationObserver(() => {
            const prefs = getAdminPreferences() as AdminPreferences;
            prefs.actionBarCollapsed = toggle.getAttribute('aria-expanded') !== 'true';
            setAdminPreferences(prefs);
        }).observe(toggle, { attributes: true, attributeFilter: ['aria-expanded'] });
    }
})();


