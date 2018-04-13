// When we load compact status from preferences we need to do some other tasks besides adding the class to the body.
// UserPreferencesLoader has already added the needed class.
$(function () {
    ps = new PerfectScrollbar('#left-nav');

    if ($('body').hasClass('left-sidebar-compact')) {
        setCompactStatus();
    }
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

    // When leftbar is expanded  all ul tags are collapsed.
    // When leftbar is compacted we don't want the first level collapsed. 
    // We want it expanded so that hovering over the root buttons shows the full submenu
    $('#left-nav ul.menu-admin > li > ul').removeClass('collapse');
    // When hovering, don't want toggling when clicking on label
    $('#left-nav ul.menu-admin > li > label').attr('data-toggle', '');
    $('#left-nav').removeClass('ps');
    $('#left-nav').removeClass('ps--active-y'); // need this too because of Edge IE11
}



function unSetCompactStatus() {
    $('body').removeClass('left-sidebar-compact');

    // resetting what we disabled for compact state
    $('#left-nav ul.menu-admin > li > ul').addClass('collapse');    
    $('#left-nav ul.menu-admin > li > label').attr('data-toggle', 'collapse');
    $('#left-nav').addClass('ps');
}
