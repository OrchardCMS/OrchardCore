// When we load compact status from preferences we need to do some other tasks besides adding the class to the body.
// UserPreferencesLoader has already added the needed class.
$(function () {
    ps = new PerfectScrollbar('#left-nav');

    // We set leftbar to compact if :
    // 1. That preference was stored by the user the last time he was on the page
    // 2. Or it's the first time on page and page is small.
    //
    if ($('body').hasClass('left-sidebar-compact')
        || (($('body').hasClass('no-admin-preferences') && $(window).width() < 768))){
        setCompactStatus();
    }
});


$('.leftbar-compactor').click(function () {
    $('body').hasClass('left-sidebar-compact') ? unSetCompactStatus() : setCompactStatus(true);
});

$('#left-nav li.has-items').click(function () {
    $('#left-nav li.has-items').removeClass("visible");
    $(this).addClass("visible");
});

$(document).on("click", function (event) {
    var $trigger = $("#left-nav li.has-items");
    if ($trigger !== event.target && !$trigger.has(event.target).length) {
        $('#left-nav li.has-items').removeClass("visible");
    }
});


var isCompactExplicit = (isCompactExplicit === undefined) ? false : isCompactExplicit ;

function setCompactStatus(explicit) {
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
    $('#left-nav li.has-items').removeClass("visible");

    if (explicit == true) {
        isCompactExplicit = explicit;
    }
    persistAdminPreferences();
}



function unSetCompactStatus() {
    $('body').removeClass('left-sidebar-compact');

    // resetting what we disabled for compact state
    $('#left-nav ul.menu-admin > li > ul').addClass('collapse');    
    $('#left-nav ul.menu-admin > li > label').attr('data-toggle', 'collapse');
    $('#left-nav').addClass('ps');
    $('#left-nav li.has-items').removeClass("visible");

    isCompactExplicit = false;
    persistAdminPreferences();
}