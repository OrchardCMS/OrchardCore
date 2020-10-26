var darkmode = (darkmode === undefined) ? false : darkmode;

var adminPreferences = JSON.parse(localStorage.getItem('adminPreferences'));
var persistedDarkmode = adminPreferences?.darkmode;

if (typeof persistedDarkmode !== 'undefined') {
    darkmode = persistedDarkmode;
}

if(document.getElementById('admin-darkmode'))
{
    // Automatically sets darkmode based on OS preferences
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        if (typeof persistedDarkmode === 'undefined') {
            document.getElementById('admin-darkmode').setAttribute('media', 'all');
            document.getElementById('admin-default').setAttribute('media', 'not all');
        }
    }

    if (darkmode)
    {
        document.getElementById('admin-darkmode').setAttribute('media', 'all');
        document.getElementById('admin-default').setAttribute('media', 'not all');
    }
}
