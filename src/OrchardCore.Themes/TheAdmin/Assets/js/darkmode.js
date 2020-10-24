var darkmode = (darkmode === undefined) ? false : darkmode;

$(function () {
    if ($('body').hasClass('darkmode') || darkmode)
    {
        $('#admin-css').attr('href', $('#admin-css').attr('href').replace('/TheAdmin.css', '/TheAdmin-dark.css'));
    }

    $("#btn-darkmode").click(function() {
        if(darkmode)
        {
            $('#admin-css').attr('href', $('#admin-css').attr('href').replace('/TheAdmin-dark.css', '/TheAdmin.css'));
            darkmode = false;
        }
        else
        {
            $('#admin-css').attr('href', $('#admin-css').attr('href').replace('/TheAdmin.css', '/TheAdmin-dark.css'));
            darkmode = true;
        }

        persistAdminPreferences();
    });
});