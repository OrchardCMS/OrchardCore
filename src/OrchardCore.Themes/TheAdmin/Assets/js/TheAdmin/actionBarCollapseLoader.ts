import { AdminPreferences } from '../adminPreferences';

// Restores the action bar collapse state before the first paint to avoid a flicker.
// Called from TheAdmin-main.ts via the shared MutationObserver so that the toggle's
// aria-expanded is already correct when CSS first evaluates :has().
// Lives in its own file so that mobileActionBar.ts (which contains a DOM-dependent IIFE)
// is never pulled into the head bundle by the module bundler.

const applyExpanded = (toggle: HTMLElement) => {
    toggle.setAttribute('aria-expanded', 'true');
    toggle.classList.remove('collapsed');
};

export const applyActionBarCollapseStateToNode = (node: Node, adminPreferences: AdminPreferences) => {
    // Default is collapsed — only act when the user explicitly left it expanded.
    if (adminPreferences?.actionBarCollapsed !== false) return;
    if (!(node instanceof HTMLElement)) return;
    if (node.matches('.action-bar-toggle')) applyExpanded(node);
    node.querySelectorAll<HTMLElement>('.action-bar-toggle').forEach(applyExpanded);
};
