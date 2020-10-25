var darkmode = (darkmode === undefined) ? false : darkmode;

$(function () {
    
    var adminPreferences = JSON.parse(localStorage.getItem('adminPreferences'));
    var persistedDarkmode = adminPreferences?.darkmode;

    if (typeof persistedDarkmode !== 'undefined') {
        darkmode = persistedDarkmode;
    }
    
    // Automatically sets darkmode based on OS preferences
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        if (typeof persistedDarkmode === 'undefined') {
            $('#admin-default').attr('media', 'not all');
            $('#admin-darkmode').attr('media', 'all');
        }
    }
    
    if (darkmode)
    {
        $('#admin-default').attr('media', 'not all');
        $('#admin-darkmode').attr('media', 'all');
    }

    $("#btn-darkmode").click(function() {
        if($('#admin-darkmode').attr('media') == 'all')
        {
            $('#admin-default').attr('media', 'all');
            $('#admin-darkmode').attr('media', 'not all');
            darkmode = false;
        }
        else
        {
            $('#admin-default').attr('media', 'not all');
            $('#admin-darkmode').attr('media', 'all');
            darkmode = true;
        }

        persistAdminPreferences();
    });
});