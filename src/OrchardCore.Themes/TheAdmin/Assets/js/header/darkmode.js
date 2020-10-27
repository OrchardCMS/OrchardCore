var darkMode = (darkMode === undefined) ? false : darkMode;

var adminPreferences = JSON.parse(localStorage.getItem('adminPreferences'));
var persistedDarkMode = adminPreferences?.darkMode;

if (typeof persistedDarkMode !== 'undefined') {
    darkMode = persistedDarkMode;
}

if(document.getElementById('admin-darkMode'))
{
    // Automatically sets darkMode based on OS preferences
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        if (typeof persistedDarkMode === 'undefined') {
            document.getElementById('admin-darkMode').setAttribute('media', 'all');
            document.getElementById('admin-default').setAttribute('media', 'not all');
        }
    }

    if (darkMode)
    {
        document.getElementById('admin-darkMode').setAttribute('media', 'all');
        document.getElementById('admin-default').setAttribute('media', 'not all');
    }
}
