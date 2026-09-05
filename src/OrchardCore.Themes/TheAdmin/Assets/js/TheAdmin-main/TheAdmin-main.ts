// We add some classes to the body tag to restore the sidebar to the state it was in before reload.
// That state was saved to localStorage by adminPreferences.ts.
// We need to apply the classes BEFORE the page is rendered.
// That is why we use a MutationObserver instead of document.Ready().
import { getAdminPreferences, setCompactExplicit, AdminPreferences } from '../adminPreferences';
import { applyActionGroupStateToNode } from '../TheAdmin/actionGroup';
import { applyActionBarCollapseStateToNode } from '../TheAdmin/actionBarCollapseLoader';
import { applySelectedNavFromSessionStorage } from '../TheAdmin/sidebar';

const userPreferencesLoader = () => {
    const adminPreferences = getAdminPreferences() as AdminPreferences;
    const shouldApplyActionBarState = adminPreferences?.actionBarCollapsed === false;
    const shouldApplyActionGroupState = !!adminPreferences?.actionGroups && Object.keys(adminPreferences.actionGroups).length > 0;

    let bodyInitialized = false;
    let actionBarStateApplied = !shouldApplyActionBarState;
    let actionGroupStateApplied = !shouldApplyActionGroupState;
    let selectedNavApplied = false;

    const tryInitializeBody = () => {
        if (bodyInitialized || !document.body) {
            return;
        }

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

        bodyInitialized = true;
    };

    const applyStateToNode = (node: Node) => {
        if (shouldApplyActionBarState) {
            applyActionBarCollapseStateToNode(node, adminPreferences);
        }

        if (shouldApplyActionGroupState) {
            applyActionGroupStateToNode(node, adminPreferences);
        }
    };

    const isDone = () => {
        if (!actionBarStateApplied && document.readyState === 'complete') {
            actionBarStateApplied = true;
        }

        if (!actionGroupStateApplied && document.readyState === 'complete') {
            actionGroupStateApplied = true;
        }

        if (!selectedNavApplied) {
            selectedNavApplied = applySelectedNavFromSessionStorage();
        }

        return bodyInitialized && actionBarStateApplied && actionGroupStateApplied && selectedNavApplied;
    };

    const tryApplyGlobalState = () => {
        tryInitializeBody();

        if (document.body) {
            applyStateToNode(document.body);
        }

        return isDone();
    };

    if (tryApplyGlobalState()) {
        return;
    }

    const observer = new MutationObserver((mutations) => {
        tryInitializeBody();

        for (const mutation of mutations) {
            for (const addedNode of mutation.addedNodes) {
                applyStateToNode(addedNode);
            }
        }

        if (isDone()) {
            observer.disconnect();
            document.removeEventListener('readystatechange', onReadyStateChange);
        }
    });

    const onReadyStateChange = () => {
        if (document.readyState !== 'complete') {
            return;
        }

        if (document.body) {
            applyStateToNode(document.body);
        }

        if (isDone()) {
            observer.disconnect();
            document.removeEventListener('readystatechange', onReadyStateChange);
        }
    };

    document.addEventListener('readystatechange', onReadyStateChange);

    observer.observe(document.documentElement, {
        childList: true,
        subtree: true,
    });
}

userPreferencesLoader();
