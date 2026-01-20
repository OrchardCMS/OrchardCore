// We add some classes to the body tag to restore the sidebar to the state is was before reload.
// That state was saved to localstorage by userPreferencesPersistor.js
// We need to apply the classes BEFORE the page is rendered.
// That is why we use a MutationObserver instead of document.Ready().
import { getAdminPreferences, setCompactExplicit } from '../constants';

const userPreferencesLoader = () => {
    const observer = new MutationObserver(function (mutations) {
        for (const mutation of mutations) {
            for (const node of mutation.addedNodes) {
                if (node instanceof HTMLElement && node.tagName === 'BODY') {
                    const body = node;
                    const adminPreferences = getAdminPreferences();

                    if (adminPreferences) {
                        try {
                            setCompactExplicit(adminPreferences.isCompactExplicit);
                            if (adminPreferences.leftSidebarCompact) {
                                body.classList.add('left-sidebar-compact');
                            }
                        } catch (error) {
                            console.error('Error while loading user preferences:', error);
                        }
                    }

                    observer.disconnect();
                    break;
                }
            }
        }
    });

    observer.observe(document.documentElement, {
        childList: true,
        subtree: true
    });
}

userPreferencesLoader();