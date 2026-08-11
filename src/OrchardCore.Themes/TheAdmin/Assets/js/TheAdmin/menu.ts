import { isCompactExplicit, setCompactExplicit } from '../constants';
import { getTenantName, getAdminPrefix } from '@orchardcore/bloom/helpers/globals';
import { persistAdminPreferences } from './userPreferencesPersistor';

let leftNav: HTMLElement | null = null;
let menuInitialized = false;

const getSelectedNavHashStorageKey = () => `${getTenantName()}-selectedNavHash`;

const persistSelectedNavHash = (hash: string | null) => {
    try {
        if (hash === null) {
            sessionStorage.removeItem(getSelectedNavHashStorageKey());
        } else {
            sessionStorage.setItem(getSelectedNavHashStorageKey(), hash);
        }
    } catch (error) {
        console.error('Error storing selected navigation hash', error);
    }
};

const getSelectedNavHashFromDom = (nav: HTMLElement): string | null => {
    const deepestLi = Array
        .from(nav.querySelectorAll<HTMLLIElement>("li.active"))
        .at(-1);

    return deepestLi
        ?.querySelector<HTMLAnchorElement>("a[data-admin-hash]")
        ?.dataset.adminHash ?? null;
};

const applySelectedNavLink = (nav: HTMLElement, selectedLink: HTMLAnchorElement) => {
    nav.querySelectorAll('li.active').forEach(li => li.classList.remove('active'));
    nav.querySelectorAll<HTMLElement>('ul.collapse.show').forEach(ul => ul.classList.remove('show'));
    nav.querySelectorAll<HTMLElement>('.item-label[data-bs-toggle="collapse"][aria-expanded="true"]')
        .forEach(label => label.setAttribute('aria-expanded', 'false'));

    let currentItem = selectedLink.closest('li');

    while (currentItem) {
        currentItem.classList.add('active');

        const childMenu = currentItem.querySelector<HTMLElement>(':scope > figure > ul.collapse');
        if (childMenu) {
            childMenu.classList.add('show');
        }

        const toggle = currentItem.querySelector<HTMLElement>(':scope > figure > figcaption > .item-label[data-bs-toggle="collapse"]');
        if (toggle) {
            toggle.setAttribute('aria-expanded', 'true');
        }

        currentItem = currentItem.parentElement?.closest('li') ?? null;
    }
};

const applySelectedNavFromSessionStorage = () => {
    // Don't apply stored selection if we're at the admin root path.
    // This handles both single-tenant (/admin) and multi-tenant (/tenant-prefix/admin) scenarios.
    const adminPrefix = getAdminPrefix().toLowerCase();
    const currentPath = window.location.pathname.toLowerCase();

    if (currentPath === adminPrefix || currentPath === adminPrefix + '/') {
        persistSelectedNavHash(null);
        return true;
    }

    let selectedNavHash: string | null;

    try {
        selectedNavHash = sessionStorage.getItem(getSelectedNavHashStorageKey());
    } catch (error) {
        console.error('Error reading selected navigation hash', error);
        return true;
    }

    // No persisted selection yet: keep the server-selected menu state.
    if (!selectedNavHash) {
        return true;
    }

    const nav = document.getElementById('left-nav');

    if (!nav) {
        return document.readyState === 'complete';
    }

    const navLinks = nav.querySelectorAll<HTMLAnchorElement>('a[data-admin-hash]');

    if (navLinks.length === 0) {
        return document.readyState === 'complete';
    }

    const hasServerSelection = nav.querySelector('li.active a[data-admin-hash]') !== null;

    // Wait until the server-side active state is present (or loading has finished)
    // so we normalize once instead of repeatedly re-applying during incremental render.
    if (!hasServerSelection && document.readyState !== 'complete') {
        return false;
    }

    const selectedLink = Array.from(navLinks)
        .find(link => link.dataset.adminHash === selectedNavHash);

    if (!selectedLink) {
        return document.readyState === 'complete';
    }

    applySelectedNavLink(nav, selectedLink);

    return true;
};

const initializeMenu = () => {
    if (menuInitialized) {
        return;
    }

    menuInitialized = true;

    // When we load compact status from preferences we need to do some other tasks besides adding the class to the body.
    // UserPreferencesLoader has already added the needed class.
    $(function () {
        // We set leftbar to compact if :
        // 1. That preference was stored by the user the last time he was on the page
        // 2. Or it's the first time on page and page is small.
        if ($('body').hasClass('left-sidebar-compact')
            || (($('body').hasClass('no-admin-preferences') && $(window).width() < 768))) {
            setCompactStatus(false);
        }
    });

    $('span.title').each(function () {
        $(this).prev('.icon').prop('title', $(this).text());
    });

    $('.leftbar-compactor').on('click', function () {
        $('body').hasClass('left-sidebar-compact') ? unSetCompactStatus() : setCompactStatus(true);
    });

    $('#left-nav li.has-items').on('click', function () {
        $('#left-nav li.has-items').removeClass("visible");
        $(this).addClass("visible");
    });

    $('#left-nav').on('click', 'a[data-admin-hash][href^="/"]', function () {
        persistSelectedNavHash(String($(this).data('admin-hash')));
    });

    $(document).on("click", function (event) {
        var $trigger = $("#left-nav li.has-items");
        if ($trigger !== event.target && !$trigger.has(event.target).length) {
            $('#left-nav li.has-items').removeClass("visible");
        }
    });

    leftNav = document.getElementById("left-nav");

    // create an Observer instance
    const resizeObserver = new ResizeObserver(() => {
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
    });

    // start observing a DOM node
    if (leftNav != null) {
        resizeObserver.observe(leftNav);

        // If no selected nav hash is stored, try to get it from the DOM and persist it.
        let selectedNavHash: string | null = null;

        try {
            selectedNavHash = sessionStorage.getItem(getSelectedNavHashStorageKey());
        } catch (error) {
            console.error('Error reading selected navigation hash', error);
        }

        if (!selectedNavHash) {
            selectedNavHash = getSelectedNavHashFromDom(leftNav);

            if (selectedNavHash) {
                persistSelectedNavHash(selectedNavHash);
            }
        }
    }
};

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

    if (leftNav) {
        leftNav.scrollTop = savedScroll;
    }

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
};

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
    if (menuAdmin) {
        menuAdmin.scrollTop = savedScroll;
    }

    setCompactExplicit(false);
    persistAdminPreferences();
};

export {
    applySelectedNavFromSessionStorage,
    initializeMenu,
    setCompactStatus,
    unSetCompactStatus,
};
