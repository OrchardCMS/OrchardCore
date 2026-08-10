// We add some classes to the body tag to restore the sidebar to the state it was in before reload.
// That state was saved to localStorage by adminPreferences.ts.
// We need to apply the classes BEFORE the page is rendered.
// That is why we use a MutationObserver instead of document.Ready().
<<<<<<< HEAD
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
=======
import { getAdminPreferences, setCompactExplicit } from '../constants';
import { applySelectedNavFromSessionStorage } from '../TheAdmin/menu';

const userPreferencesLoader = () => {
    let bodyInitialized = false;
    let selectedNavApplied = false;

    const applyState = () => {
        if (!bodyInitialized && document.body) {
            const adminPreferences = getAdminPreferences();

            if (adminPreferences) {
                try {
                    setCompactExplicit(adminPreferences.isCompactExplicit);
                    if (adminPreferences.leftSidebarCompact) {
                        document.body.classList.add('left-sidebar-compact');
>>>>>>> origin/main
                    }
                } catch (error) {
                    console.error('Error while loading user preferences:', error);
                }
            }

<<<<<<< HEAD
            applyActionBarCollapseStateToNode(node, adminPreferences);
            applyActionGroupStateToNode(node, adminPreferences);
=======
            bodyInitialized = true;
        }

        if (!selectedNavApplied) {
            selectedNavApplied = applySelectedNavFromSessionStorage();
        }

        return bodyInitialized && selectedNavApplied;
    };

    if (applyState()) {
        return;
    }

    const observer = new MutationObserver(() => {
        if (applyState()) {
            observer.disconnect();
>>>>>>> origin/main
        }
    }
});

<<<<<<< HEAD
observer.observe(document.documentElement, { childList: true, subtree: true });
document.addEventListener('DOMContentLoaded', () => observer.disconnect());
=======
    observer.observe(document.documentElement, {
        childList: true,
        subtree: true,
    });
}

userPreferencesLoader();
>>>>>>> origin/main
