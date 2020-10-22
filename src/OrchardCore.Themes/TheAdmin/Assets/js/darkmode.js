var darkmode = (darkmode === undefined) ? false : darkmode;

$(function () {
    if ($('body').hasClass('darkmode'))
    {
        $('#admin-css').attr('href', $('#admin-css').attr('href').replace('/admin', '/admin-dark'));
    }

    $("#btn-darkmode").click(function() {
        if(darkmode)
        {
            darkmode = false;
            $('#admin-css').attr('href', $('#admin-css').attr('href').replace('/admin-dark', '/admin'));
        }
        else
        {
            darkmode = true;
            $('#admin-css').attr('href', $('#admin-css').attr('href').replace('/admin', '/admin-dark'));
        }

        persistAdminPreferences();
    });
});