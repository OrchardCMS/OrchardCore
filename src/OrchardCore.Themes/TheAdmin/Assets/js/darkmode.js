$('#btn-darkmode').click(function() {
    if($('#admin-darkmode').attr('media') === 'all')
    {
        $('#admin-default').attr('media', 'all');
        $('#admin-darkmode').attr('media', 'not all');
        $('html').attr('data-theme', 'default');
        $(this).children(':first').removeClass('fa-sun');
        $(this).children(':first').addClass('fa-moon');
    }
    else
    {
        $('#admin-default').attr('media', 'not all');
        $('#admin-darkmode').attr('media', 'all');
        $('html').attr('data-theme', 'darkmode');
        $(this).children(':first').removeClass('fa-moon');
        $(this).children(':first').addClass('fa-sun');
    }

    persistAdminPreferences();
});
