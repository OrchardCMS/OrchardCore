var darkmode = (darkmode === undefined) ? false : darkmode;

$(function () {
    if ($('body').hasClass('darkmode') || darkmode)
    {
        $('#admin-css').attr('href', $('#admin-css').attr('href').replace('/TheAdmin.css', '/TheAdmin-darkmode.css'));
    }

    $("#btn-darkmode").click(function() {
        if(darkmode)
        {
            $('#admin-css').attr('href', $('#admin-css').attr('href').replace('/TheAdmin-darkmode.css', '/TheAdmin.css'));
            darkmode = false;
        }
        else
        {
            $('#admin-css').attr('href', $('#admin-css').attr('href').replace('/TheAdmin.css', '/TheAdmin-darkmode.css'));
            darkmode = true;
        }

        persistAdminPreferences();
    });
});