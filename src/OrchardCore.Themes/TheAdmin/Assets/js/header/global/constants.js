const darkThemeName = 'dark';
const lightThemeName = 'light';
const getTenantName = () => document.documentElement.getAttribute('data-tenant') || '';
const getStoredTheme = () => localStorage.getItem(getTenantName() + '-admintheme');
const setStoredTheme = theme => localStorage.setItem(getTenantName() + '-admintheme', theme);
const isDarkMedia = () => window.matchMedia('(prefers-color-scheme: dark)').matches;
const getPreferredTheme = () => {
    const storedTheme = getStoredTheme()
    if (storedTheme) {
        return storedTheme;
    }

    return isDarkMedia() ? darkThemeName : lightThemeName;
}
const setTheme = theme => {
    if (theme === 'auto') {
        document.documentElement.setAttribute('data-bs-theme', isDarkMedia() ? darkThemeName : lightThemeName);
    } else {
        document.documentElement.setAttribute('data-bs-theme', theme);
    }
}
const getAdminPreferenceKey = () => getTenantName() + '-adminPreferences';
const getAdminPreferences = () => JSON.parse(localStorage.getItem(getAdminPreferenceKey()));
const setAdminPreferences = (adminPreferences) => {
    const key = getAdminPreferenceKey();

    localStorage.setItem(key, JSON.stringify(adminPreferences));
    Cookies.set(key, JSON.stringify(adminPreferences), { expires: 360 });
};
