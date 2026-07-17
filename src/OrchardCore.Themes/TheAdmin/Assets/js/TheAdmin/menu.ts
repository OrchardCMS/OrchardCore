import { isCompactExplicit, setCompactExplicit, getAdminPreferences, setAdminPreferences } from '../constants';
import { persistAdminPreferences } from './userPreferencesPersistor';
// When we load compact status from preferences we need to do some other tasks besides adding the class to the body.
// UserPreferencesLoader has already added the needed class.
$(function () {


    // We set leftbar to compact if :
    // 1. That preference was stored by the user the last time he was on the page
    // 2. Or it's the first time on page and page is small.
    //
    if ($('body').hasClass('left-sidebar-compact')
        || (($('body').hasClass('no-admin-preferences') && $(window).width() < 768))) {
        setCompactStatus(false);
    }
});

$('span.title').each(function () {
    $(this).prev('.icon').prop('title', $(this).text());
});

$('.leftbar-compactor').click(function () {
    $('body').hasClass('left-sidebar-compact') ? unSetCompactStatus() : setCompactStatus(true);
});

$('#left-nav li.has-items').click(function () {
    $('#left-nav li.has-items').removeClass("visible");
    $(this).addClass("visible");
});

// Remember which nav item was clicked. The server selects the active item deterministically
// from the URL, but it can't tell apart two items that resolve to the same rank (e.g. two
// links with the same href). This records the clicked item so we can break that tie below.
$('#left-nav').on('click', 'a[data-admin-hash][href^="/"]', function () {
    const prefs = getAdminPreferences() as Record<string, unknown>;
    prefs.selectedNavHash = String($(this).data('admin-hash'));
    setAdminPreferences(prefs);
});

// The server picks the active item deterministically from the URL, preferring the owner a page
// declares (e.g. a content type list while editing an item) over a menu item that merely links
// to the same URL. If this tab actually clicked a link pointing at the current page, promote
// that link instead - the one piece of intent the server can't see. Runs only when the clicked
// link truly targets the current page, so a stale selection from another page is left alone.
const promoteClickedLink = () => {
    const prefs = getAdminPreferences() as Record<string, unknown>;
    const clickedHash = prefs.selectedNavHash ? String(prefs.selectedNavHash) : '';

    if (!clickedHash) {
        return;
    }

    const menu = document.getElementById('adminMenu');
    const target = menu?.querySelector<HTMLAnchorElement>(`a[data-admin-hash="${CSS.escape(clickedHash)}"]`);

    if (!menu || !target || target.pathname.toLowerCase() !== window.location.pathname.toLowerCase()) {
        return;
    }

    const targetItem = target.closest('li');

    if (!targetItem || targetItem.classList.contains('active')) {
        return;
    }

    // Clear the server's selection, then activate the clicked item and expand its ancestors.
    menu.querySelectorAll('li.active').forEach(li => li.classList.remove('active'));
    menu.querySelectorAll('ul.show').forEach(ul => ul.classList.remove('show'));

    for (let el: HTMLElement | null = targetItem; el && el !== menu; el = el.parentElement) {
        if (el.tagName === 'LI') {
            el.classList.add('active');
        } else if (el.tagName === 'UL') {
            el.classList.add('show');
        }
    }
};

$(promoteClickedLink);

$(document).on("click", function (event) {
    var $trigger = $("#left-nav li.has-items");
    if ($trigger !== event.target && !$trigger.has(event.target).length) {
        $('#left-nav li.has-items').removeClass("visible");
    }
});

var subMenuArray = new Array();

const setCompactStatus = (explicit) => {
    // This if is to avoid that when sliding from expanded to compact the
    // underliyng ul is visible while shrinking. It is ugly.
    if (!$('body').hasClass('left-sidebar-compact')) {
        var labels = $('#left-nav ul.menu-admin > li > figure > figcaption > .item-label');
        labels.css('background-color', 'transparent');
        setTimeout(function () {
            labels.css('background-color', '');
        }, 200);
    }

    // Transfer scroll position from expanded scroller (.menu-admin) to compact scroller (#left-nav)
    const menuAdmin = document.querySelector<HTMLElement>('#left-nav ul.menu-admin');
    const savedScroll = menuAdmin ? menuAdmin.scrollTop : 0;

    $('body').addClass('left-sidebar-compact');

    if (leftNav) leftNav.scrollTop = savedScroll;

    // When leftbar is expanded  all ul tags are collapsed.
    // When leftbar is compacted we don't want the first level collapsed. 
    // We want it expanded so that hovering over the root buttons shows the full submenu
    $('#left-nav ul.menu-admin > li > figure > ul').removeClass('collapse');
    // When hovering, don't want toggling when clicking on label
    $('#left-nav ul.menu-admin > li > figure > figcaption > .item-label').attr('data-bs-toggle', '');
    $('#left-nav li.has-items').removeClass("visible");

    //after menu has collapsed we set the transitions to none so that we don't do any transition
    //animation when open a sub-menu
    setTimeout(function () {
        $('#left-nav > ul > li').css("transition", "none");
    }, 200);

    if (explicit == true) {
        setCompactExplicit(true);
    }
    persistAdminPreferences();
}

const unSetCompactStatus = () => {
    // Transfer scroll position from compact scroller (#left-nav) to expanded scroller (.menu-admin)
    const savedScroll = leftNav ? leftNav.scrollTop : 0;

    $('body').removeClass('left-sidebar-compact');

    // resetting what we disabled for compact state
    $('#left-nav ul.menu-admin > li > figure > ul').addClass('collapse');
    $('#left-nav ul.menu-admin > li > figure > figcaption > button.item-label').attr('data-bs-toggle', 'collapse');
    $('#left-nav li.has-items').removeClass("visible");
    $('#left-nav > ul > li').css("transition", "");

    const menuAdmin = document.querySelector<HTMLElement>('#left-nav ul.menu-admin');
    if (menuAdmin) menuAdmin.scrollTop = savedScroll;

    setCompactExplicit(false);
    persistAdminPreferences();
}

var leftNav = document.getElementById("left-nav");

// create an Observer instance
const resizeObserver = new ResizeObserver(entries => {
    if (isCompactExplicit) {
        if (leftNav && (leftNav.scrollHeight > leftNav.clientHeight)) {
            document.body.classList.add("scroll");
        }
        else {
            document.body.classList.remove("scroll");
        }
    }
    else {
        document.body.classList.remove("scroll");
    }
})

// start observing a DOM node
if (leftNav != null) {
    resizeObserver.observe(leftNav)
}

export {
    setCompactStatus,
    unSetCompactStatus,
}