// We add some classes to the body tag to restore the sidebar to the state it was in before reload.
// That state was saved to localStorage by adminPreferences.ts.
// We need to apply the classes BEFORE the page is rendered.
// That is why we use a MutationObserver instead of document.Ready().
//
// A single observer handles all three preference-restore concerns so that the browser
// only has to run one mutation-watching loop for the head bundle.
import { getAdminPreferences, setCompactExplicit, AdminPreferences } from '../adminPreferences';
import { applyActionGroupStateToNode } from '../TheAdmin/actionGroup';
import { applyActionBarCollapseStateToNode } from '../TheAdmin/actionBarCollapseLoader';

const adminPreferences = getAdminPreferences() as AdminPreferences;

const observer = new MutationObserver(mutations => {
    for (const mutation of mutations) {
        for (const node of mutation.addedNodes) {
            if (node instanceof HTMLElement && node.tagName === 'BODY') {
                try {
                    setCompactExplicit(adminPreferences.isCompactExplicit);
                    if (adminPreferences.leftSidebarCompact) {
                        node.classList.add('left-sidebar-compact');
                    }
                } catch (error) {
                    console.error('Error while loading user preferences:', error);
                }
            }

            applyActionBarCollapseStateToNode(node, adminPreferences);
            applyActionGroupStateToNode(node, adminPreferences);
        }
    }
});

observer.observe(document.documentElement, { childList: true, subtree: true });
document.addEventListener('DOMContentLoaded', () => observer.disconnect());