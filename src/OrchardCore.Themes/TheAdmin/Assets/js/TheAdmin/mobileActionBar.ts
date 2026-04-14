import { getAdminPreferences, setAdminPreferences } from '../adminPreferences';

// Manages the mobile action bar at runtime.
//
// This file must remain in the foot bundle (TheAdmin.ts) and must NOT be imported
// from the head bundle (TheAdmin-main.ts). It contains a DOM-dependent IIFE that
// queries live elements, so it requires the DOM to already exist. Its observers
// also serve the opposite purpose from the head-bundle observer: they *save* state
// changes made by the user, whereas the head-bundle observer *restores* saved state
// before the first paint.
(function () {

    const secondActionBar = document.querySelector<HTMLElement>('.second-action-bar');
    if (secondActionBar) {
        new ResizeObserver(() => {
            document.documentElement.style.setProperty('--oc-second-action-bar-height', secondActionBar.offsetHeight + 'px');
        }).observe(secondActionBar);
    }

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
            const prefs = getAdminPreferences();
            prefs.actionBarCollapsed = toggle.getAttribute('aria-expanded') !== 'true';
            setAdminPreferences(prefs);
        }).observe(toggle, { attributes: true, attributeFilter: ['aria-expanded'] });
    }
})();


