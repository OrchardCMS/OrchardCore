$(function () {
    $('#navbar-toggler').on('click', function () {
        var nav = $('#ta-left-sidebar');
        if (nav.hasClass('in')) {
            nav.animate({ 'left': '-260px' }, 300);
            nav.removeClass('in');
        }
        else {
            nav.animate({ 'left': '0px' }, 300);
            nav.addClass('in');
        }
    });

    $(window).on('resize', function(){
        var win = $(this); //this = window
        if (win.width() >= 768) { 
            $('#ta-left-sidebar').removeAttr('style');
        }
    });
});