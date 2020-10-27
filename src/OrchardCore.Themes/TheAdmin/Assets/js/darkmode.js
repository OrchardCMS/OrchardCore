$("#btn-darkmode").click(function() {
    if($('#admin-darkmode').attr('media') === 'all')
    {
        $('#admin-default').attr('media', 'all');
        $('#admin-darkmode').attr('media', 'not all');
        $(this).children(':first').addClass('fa-moon');
        darkMode = false;
    }
    else
    {
        $('#admin-default').attr('media', 'not all');
        $('#admin-darkmode').attr('media', 'all');
        $(this).children(':first').addClass('fa-sun');
        darkMode = true;
    }

    persistAdminPreferences();
});
