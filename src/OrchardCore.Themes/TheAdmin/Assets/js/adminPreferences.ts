import { getTenantName } from '@orchardcore/bloom/helpers/globals';
import Cookies from 'js-cookie';

export interface AdminPreferences {
    leftSidebarCompact: boolean;
    isCompactExplicit: boolean;
    actionGroups?: Record<string, string>;
    actionBarCollapsed?: boolean;
}

let isCompactExplicit = false;

const setCompactExplicit = (value: boolean) => {
    isCompactExplicit = value;
}

const getAdminPreferenceKey = () => getTenantName() + '-adminPreferences';

const getAdminPreferences = (): AdminPreferences => {
    const storedValue = localStorage.getItem(getAdminPreferenceKey());

    if (!storedValue) {
        return {} as AdminPreferences;
    }

    try {
        return JSON.parse(storedValue) as AdminPreferences;
    } catch (ex) {
        console.error('Error retrieving admin preferences', ex);
        return {} as AdminPreferences;
    }
};

const setAdminPreferences = (adminPreferences: AdminPreferences) => {
    if (adminPreferences == null) {
        console.error('Error setting admin preferences, argument is null');
        return;
    }

    const key = getAdminPreferenceKey();

    try {
        localStorage.setItem(key, JSON.stringify(adminPreferences));
        Cookies.set(key, JSON.stringify(adminPreferences), { expires: 360 });
    } catch (ex) {
        console.error('Error setting admin preferences', ex);
    }
};

// Snapshots the current UI state and writes it to storage.
// Called whenever the user changes a persistent preference (sidebar, action bar, action groups).
const persistAdminPreferences = () => {
    setTimeout(() => {
        const adminPreferences = getAdminPreferences();
        adminPreferences.leftSidebarCompact = document.body.classList.contains('left-sidebar-compact');
        adminPreferences.isCompactExplicit = isCompactExplicit;
        setAdminPreferences(adminPreferences);
    }, 200);
}

export {
    isCompactExplicit,
    setCompactExplicit,
    getAdminPreferences,
    setAdminPreferences,
    persistAdminPreferences,
}
