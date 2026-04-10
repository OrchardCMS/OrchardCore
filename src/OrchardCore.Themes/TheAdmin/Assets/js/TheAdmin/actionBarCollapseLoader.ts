import { getAdminPreferences } from '../constants';

// Restores the action bar collapse state before the first paint to avoid a flicker.
// Called from TheAdmin-main.ts (head bundle) via a MutationObserver so that
// the toggle's aria-expanded is already correct when CSS first evaluates :has().
// Lives in its own file so that mobileActionBar.ts (which contains a DOM-dependent IIFE)
// is never pulled into the head bundle by the module bundler.
export const actionBarCollapseLoader = () => {
    const adminPreferences = getAdminPreferences();
    // Default is collapsed — only act when the user explicitly left it expanded.
    if (adminPreferences?.actionBarCollapsed !== false) return;

    const applyExpanded = (toggle: HTMLElement) => {
        toggle.setAttribute('aria-expanded', 'true');
        toggle.classList.remove('collapsed');
    };

    const processNode = (node: Node) => {
        if (!(node instanceof HTMLElement)) return;
        if (node.matches('.action-bar-toggle')) applyExpanded(node);
        node.querySelectorAll<HTMLElement>('.action-bar-toggle').forEach(applyExpanded);
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
