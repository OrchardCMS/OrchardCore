import { isCompactExplicit, setCompactExplicit, getAdminPreferences, setAdminPreferences } from '../constants';
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
    const activeItems = Array.from(nav.querySelectorAll<HTMLLIElement>("li.active"));
    const deepestLi = activeItems[activeItems.length - 1];

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
        if (document.body.classList.contains('left-sidebar-compact')) {
            unSetCompactStatus();
        } else {
            setCompactStatus(true);
        }
    });

    document.querySelectorAll('#left-nav li.has-items').forEach((item) => {
        item.addEventListener('click', function (this: Element) {
            document.querySelectorAll('#left-nav li.has-items').forEach((el) => el.classList.remove('visible'));
            this.classList.add('visible');
        });
    });

    // When navigating via a real nav link, persist the selected item hash inside the
    // existing admin preferences cookie so the server can restore the correct selection,
    // and inside session storage so the hash survives an in-page (Turbo-style) navigation.
    document.getElementById('left-nav')?.addEventListener('click', (event) => {
        const link = (event.target as Element)?.closest<HTMLElement>('a[data-admin-hash][href^="/"]');
        if (!link) {
            return;
        }
        const prefs = getAdminPreferences() as Record<string, unknown>;
        prefs.selectedNavHash = String(link.dataset.adminHash);
        setAdminPreferences(prefs);
        persistSelectedNavHash(String(link.dataset.adminHash));
    });

    document.addEventListener('click', (event) => {
        const target = event.target as Element;
        const triggers = document.querySelectorAll('#left-nav li.has-items');
        const clickedInsideTrigger = Array.from(triggers).some((el) => el === target || el.contains(target));
        if (!clickedInsideTrigger) {
            triggers.forEach((el) => el.classList.remove('visible'));
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

    if (leftNav) {
        leftNav.scrollTop = savedScroll;
    }

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
};

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
