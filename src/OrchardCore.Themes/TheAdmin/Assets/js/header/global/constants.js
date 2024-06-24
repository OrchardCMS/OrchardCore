themeStoreKeySuffix = 'admintheme';
const getAdminPreferenceKey = () => getTenantName() + '-adminPreferences';
const getAdminPreferences = () => JSON.parse(localStorage.getItem(getAdminPreferenceKey()));
const setAdminPreferences = (adminPreferences) => {
    const key = getAdminPreferenceKey();

    localStorage.setItem(key, JSON.stringify(adminPreferences));
    Cookies.set(key, JSON.stringify(adminPreferences), { expires: 360 });
};

