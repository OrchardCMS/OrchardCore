import { isCompactExplicit, getAdminPreferences, setAdminPreferences } from '../constants';

// Each time the sidebar status is modified, that is persisted to localStorage.
// When the page is loaded again, userPreferencesLoader.js will read that info to 
// restore the sidebar to the previous state.
export const persistAdminPreferences = () => {
    setTimeout(function () {
        var adminPreferences = getAdminPreferences() as AdminPreferences;
        adminPreferences.leftSidebarCompact = document.body.classList.contains('left-sidebar-compact') ? true : false;
        adminPreferences.isCompactExplicit = isCompactExplicit;
        setAdminPreferences(adminPreferences);
    }, 200);
}

export interface AdminPreferences {
    leftSidebarCompact: boolean;
    isCompactExplicit: boolean;
    actionGroups?: Record<string, string>;
    actionBarCollapsed?: boolean;
}