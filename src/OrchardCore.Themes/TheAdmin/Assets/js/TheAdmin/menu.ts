import { isCompactExplicit, setCompactExplicit, getAdminPreferences, setAdminPreferences } from '../constants';
import { persistAdminPreferences } from './userPreferencesPersistor';
// When we load compact status from preferences we need to do some other tasks besides adding the class to the body.
// UserPreferencesLoader has already added the needed class.
document.addEventListener('DOMContentLoaded', () => {
    // We set leftbar to compact if :
    // 1. That preference was stored by the user the last time he was on the page
    // 2. Or it's the first time on page and page is small.
    //
    if (document.body.classList.contains('left-sidebar-compact')
        || (document.body.classList.contains('no-admin-preferences') && window.innerWidth < 768)) {
        setCompactStatus(false);
    }
});

document.querySelectorAll('span.title').forEach((el) => {
    const icon = el.previousElementSibling;
    if (icon?.classList.contains('icon')) {
        (icon as HTMLElement).title = el.textContent ?? '';
    }
});

document.querySelector('.leftbar-compactor')?.addEventListener('click', () => {
    document.body.classList.contains('left-sidebar-compact') ? unSetCompactStatus() : setCompactStatus(true);
});

document.querySelectorAll('#left-nav li.has-items').forEach((item) => {
    item.addEventListener('click', function (this: Element) {
        document.querySelectorAll('#left-nav li.has-items').forEach((el) => el.classList.remove('visible'));
        this.classList.add('visible');
    });
});

// When navigating via a real nav link, persist the selected item hash inside the
// existing admin preferences cookie so the server can restore the correct selection.
document.getElementById('left-nav')?.addEventListener('click', (event) => {
    const link = (event.target as Element)?.closest<HTMLElement>('a[data-admin-hash][href^="/"]');
    if (!link) {
        return;
    }
    const prefs = getAdminPreferences() as Record<string, unknown>;
    prefs.selectedNavHash = String(link.dataset.adminHash);
    setAdminPreferences(prefs);
});

document.addEventListener('click', (event) => {
    const target = event.target as Element;
    const triggers = document.querySelectorAll('#left-nav li.has-items');
    const clickedInsideTrigger = Array.from(triggers).some((el) => el === target || el.contains(target));
    if (!clickedInsideTrigger) {
        triggers.forEach((el) => el.classList.remove('visible'));
    }
});

var subMenuArray = new Array();

const setCompactStatus = (explicit: boolean) => {
    // This if is to avoid that when sliding from expanded to compact the
    // underliyng ul is visible while shrinking. It is ugly.
    if (!document.body.classList.contains('left-sidebar-compact')) {
        const labels = document.querySelectorAll<HTMLElement>('#left-nav ul.menu-admin > li > figure > figcaption > .item-label');
        labels.forEach((label) => label.style.backgroundColor = 'transparent');
        setTimeout(function () {
            labels.forEach((label) => label.style.backgroundColor = '');
        }, 200);
    }

    // Transfer scroll position from expanded scroller (.menu-admin) to compact scroller (#left-nav)
    const menuAdmin = document.querySelector<HTMLElement>('#left-nav ul.menu-admin');
    const savedScroll = menuAdmin ? menuAdmin.scrollTop : 0;

    document.body.classList.add('left-sidebar-compact');

    if (leftNav) leftNav.scrollTop = savedScroll;

    // When leftbar is expanded  all ul tags are collapsed.
    // When leftbar is compacted we don't want the first level collapsed.
    // We want it expanded so that hovering over the root buttons shows the full submenu
    document.querySelectorAll('#left-nav ul.menu-admin > li > figure > ul').forEach((el) => el.classList.remove('collapse'));
    // When hovering, don't want toggling when clicking on label
    document.querySelectorAll('#left-nav ul.menu-admin > li > figure > figcaption > .item-label').forEach((el) => el.setAttribute('data-bs-toggle', ''));
    document.querySelectorAll('#left-nav li.has-items').forEach((el) => el.classList.remove('visible'));

    //after menu has collapsed we set the transitions to none so that we don't do any transition
    //animation when open a sub-menu
    setTimeout(function () {
        document.querySelectorAll<HTMLElement>('#left-nav > ul > li').forEach((el) => el.style.transition = 'none');
    }, 200);

    if (explicit == true) {
        setCompactExplicit(true);
    }
    persistAdminPreferences();
}

const unSetCompactStatus = () => {
    // Transfer scroll position from compact scroller (#left-nav) to expanded scroller (.menu-admin)
    const savedScroll = leftNav ? leftNav.scrollTop : 0;

    document.body.classList.remove('left-sidebar-compact');

    // resetting what we disabled for compact state
    document.querySelectorAll('#left-nav ul.menu-admin > li > figure > ul').forEach((el) => el.classList.add('collapse'));
    document.querySelectorAll('#left-nav ul.menu-admin > li > figure > figcaption > button.item-label').forEach((el) => el.setAttribute('data-bs-toggle', 'collapse'));
    document.querySelectorAll('#left-nav li.has-items').forEach((el) => el.classList.remove('visible'));
    document.querySelectorAll<HTMLElement>('#left-nav > ul > li').forEach((el) => el.style.transition = '');

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
