var leftMenuPS;
// When we load compact status from preferences we need to do some other tasks besides adding the class to the body.
// UserPreferencesLoader has already added the needed class.
$(function () {
    leftMenuPS = new PerfectScrollbar('#left-nav', { suppressScrollX: true });

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
var subMenuArray = new Array();

function setCompactStatus(explicit) {
    // This if is to avoid that when sliding from expanded to compact the 
    // underliyng ul is visible while shrinking. It is ugly.    
    if (!$('body').hasClass('left-sidebar-compact')) {
        var labels = $('#left-nav ul.menu-admin > li > .item-label');
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
    $('#left-nav li.has-items').removeClass("visible");

    //after menu has collapsed we set the transitions to none so that we don't do any transition
    //animation when open a sub-menu
    setTimeout(function () {
        $('#left-nav > ul > li').css("transition", "none");
    }, 200); 
    
    //$('#left-nav').scrollTop = 0;
    //leftMenuPS.update();

    if (leftMenuPS) {
        leftMenuPS.destroy();
        leftMenuPS = null; // to make sure garbages are collected
    }

    //set PerfectScrollBar on sub-menu items.
    var submenus = $('#left-nav > ul > li > [id^="m"]');
    submenus.each(function (index) {
        subMenuArray[index] = new PerfectScrollbar(this, { suppressScrollX: true });
    });

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
    $('#left-nav li.has-items').removeClass("visible");
    $('#left-nav > ul > li').css("transition", "");

    if (leftMenuPS == null) {
        leftMenuPS = new PerfectScrollbar('#left-nav', { suppressScrollX: true });
    }
    else {
        leftMenuPS.destroy();
        leftMenuPS = null; // to make sure garbages are collected
        leftMenuPS = new PerfectScrollbar('#left-nav', { suppressScrollX: true });
    }

    //remove PerfectScrollBar on sub-menu items
    subMenuArray.forEach(function (ps) {
        ps.destroy();
        ps = null; // to make sure garbages are collected
    });

    isCompactExplicit = false;
    persistAdminPreferences();
}