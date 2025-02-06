import { getTenantName } from '@orchardcore/frontend/helpers/globals';
import Cookies from 'js-cookie';

let isCompactExplicit = false;

const setCompactExplicit = (value: boolean) => {
    isCompactExplicit = value;
}

const getAdminPreferenceKey = () => getTenantName() + '-adminPreferences';

const getAdminPreferences = () => {
    const storedValue = localStorage.getItem(getAdminPreferenceKey());

    if (!storedValue) {
        return {};
    }

    try {
        return JSON.parse(storedValue);
    } catch (ex) {
        console.error('Error retrieving admin preferences', ex);
        return {};
    }
};

const setAdminPreferences = (adminPreferences) => {
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

export {
    isCompactExplicit,
    setCompactExplicit,
    getAdminPreferences,
    setAdminPreferences
}

