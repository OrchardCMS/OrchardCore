// We add some classes to the body tag to restore the sidebar to the state it was in before reload.
// That state was saved to localStorage by adminPreferences.ts.
// We need to apply the classes BEFORE the page is rendered.
// That is why we use a MutationObserver instead of document.Ready().
import { getAdminPreferences, setCompactExplicit, AdminPreferences } from '../adminPreferences';
import { applyActionGroupStateToNode } from '../TheAdmin/actionGroup';
import { applyActionBarCollapseStateToNode } from '../TheAdmin/actionBarCollapseLoader';
import { applySelectedNavFromSessionStorage } from '../TheAdmin/sidebar';

const userPreferencesLoader = () => {
    let bodyInitialized = false;
    let selectedNavApplied = false;

    const applyState = () => {
        if (!bodyInitialized && document.body) {
            const adminPreferences = getAdminPreferences() as AdminPreferences;

            if (adminPreferences) {
                try {
                    setCompactExplicit(adminPreferences.isCompactExplicit);
                    if (adminPreferences.leftSidebarCompact) {
                        document.body.classList.add('left-sidebar-compact');
                    }
                } catch (error) {
                    console.error('Error while loading user preferences:', error);
                }
            }

            applyActionBarCollapseStateToNode(document.body, adminPreferences);
            applyActionGroupStateToNode(document.body, adminPreferences);

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
        }
    });

    observer.observe(document.documentElement, {
        childList: true,
        subtree: true,
    });
}

userPreferencesLoader();
