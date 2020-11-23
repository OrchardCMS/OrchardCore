$('#btn-darkmode').click(function() {
    if($('html').data('theme') === 'darkmode')
    {
        $('html').data('theme', 'default');
        $('html').attr('data-theme', 'default');
        $(this).children(':first').removeClass('fa-sun');
        $(this).children(':first').addClass('fa-moon');
    }
    else
    {
        $('html').data('theme', 'darkmode');
        $('html').attr('data-theme', 'darkmode');
        $(this).children(':first').removeClass('fa-moon');
        $(this).children(':first').addClass('fa-sun');
    }

    persistAdminPreferences();
});
