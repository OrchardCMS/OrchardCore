// When we load compact status from preferences we need to do some other tasks besides adding the class to the body.
// UserPreferencesLoader has already added the needed class.
$(function () {
    ps = new PerfectScrollbar('#left-nav');

    if ($('body').hasClass('left-sidebar-compact')) {
        setCompactStatus();
    }
});



$('#leftbar-toggler').click(function () {
    $('body').toggleClass('left-sidebar-hidden');

    if ($('body').hasClass('left-sidebar-hidden')) {
        $('body').removeClass('leftbar-visible-on-small');
    }

    persistAdminPreferences();
});



$('#small-screen-leftbar-toggler').click(function () {
    $('body').toggleClass('leftbar-visible-on-small');

    if ($('body').hasClass('leftbar-visible-on-small')) {
        $('body').removeClass('left-sidebar-hidden');
        // we dont want compact when in small screen
        $('body').removeClass('left-sidebar-compact');
        $('#left-nav').addClass('ps');
        $('#left-nav ul.menu-admin > li > ul').addClass('collapse');
    }
    persistAdminPreferences();
});



$('.leftbar-compactor').click(function () {
    $('body').hasClass('left-sidebar-compact') ? unSetCompactStatus() : setCompactStatus();

    persistAdminPreferences();
});




function setCompactStatus() {
    // This if is to avoid that when sliding from expanded to compact the 
    // underliyng ul is visible while shrinking. It is ugly.    
    if (!$('body').hasClass('left-sidebar-compact')) {
        var labels = $('#left-nav ul.menu-admin > li > label');
        labels.css('background-color', 'transparent');
        setTimeout(function () {
            labels.css('background-color', '');
        }, 200);
    }

    $('body').addClass('left-sidebar-compact');
    $('body').removeClass('leftbar-visible-on-small');

    // When leftbar is expanded  all ul tags are collapsed.
    // When leftbar is compacted we don't want the first level collapsed. 
    // We want it expanded so that hovering over the root buttons shows the full submenu
    $('#left-nav ul.menu-admin > li > ul').removeClass('collapse');
    $('#left-nav').removeClass('ps');
    $('#left-nav').removeClass('ps--active-y'); // need this too because of Edge IE11


    // Fixing a positioning issue on the button on IE/Edge
    if (isIE()) {
       $('.leftbar-compactor').css('margin-top', '-100px');       
    }
}



function unSetCompactStatus() {
    $('body').removeClass('left-sidebar-compact');

    $('#left-nav ul.menu-admin > li > ul').addClass('collapse');
    $('#left-nav').addClass('ps');

    // Fixing a positioning issue on the button on IE/Edge
    if (isIE()) {
        $('.leftbar-compactor').css('margin-top', '0px');
    }
}


// This detector is only required in order to fix a positioning issue of the leftbar button compactor on IE/Edege
// We can safely remove it if the issue is fixed in other way.
function isIE() {
    var ua = window.navigator.userAgent;

    var msie = ua.indexOf('MSIE ');
    if (msie > 0) {
        // IE 10 or older => return version number
        return parseInt(ua.substring(msie + 5, ua.indexOf('.', msie)), 10);
    }

    var trident = ua.indexOf('Trident/');
    if (trident > 0) {
        // IE 11 => return version number
        var rv = ua.indexOf('rv:');
        return parseInt(ua.substring(rv + 3, ua.indexOf('.', rv)), 10);
    }

    var edge = ua.indexOf('Edge/');
    if (edge > 0) {
        // Edge (IE 12+) => return version number
        return parseInt(ua.substring(edge + 5, ua.indexOf('.', edge)), 10);
    }

    // other browser
    return false;
}





// After adding the minimizable leftbar these two functions are not needed anymore
// They can be safely deleted in the future.
// Just keeping them here for a while.

//$(function () {
//    $('#navbar-toggler').on('click', function () {
//        var nav = $('#ta-left-sidebar');
//        if (nav.hasClass('in')) {
//            nav.animate({ 'left': '-260px' }, 300);
//            nav.removeClass('in');
//        }
//        else {
//            nav.animate({ 'left': '0px' }, 300);
//            nav.addClass('in');
//        }
//    });

//    $(window).on('resize', function(){
//        var win = $(this); //this = window
//        if (win.width() >= 768) { 
//            $('#ta-left-sidebar').removeAttr('style');
//        }
//    });
//});