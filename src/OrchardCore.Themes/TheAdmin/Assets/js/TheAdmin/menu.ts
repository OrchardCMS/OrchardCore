import { setCompactExplicit, getAdminPreferences, setAdminPreferences } from '../constants';
import { getTenantName, getAdminPrefix } from '@orchardcore/bloom/helpers/globals';
import { persistAdminPreferences } from './userPreferencesPersistor';

let menuInitialized = false;

// Below this width the menu is rendered as an off canvas drawer instead of a sidebar.
const mobileQuery = '(max-width: 767.98px)';
const sidebarOpenClass = 'ta-sidebar-open';
const compactClass = 'left-sidebar-compact';

const isMobile = () => window.matchMedia(mobileQuery).matches;

const isCompact = () => document.body.classList.contains(compactClass) && !isMobile();

// 'persistent': every section keeps the state the user left it in, from page to page.
// 'focused': only the section of the current page is open, and opening a section closes
// the others at the same level. Chosen in the admin settings.
const isFocused = () => document.getElementById('left-nav')?.dataset.menuBehavior === 'focused';

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

// Shows or hides a collapsible list, keeping the state of the elements toggling it in sync.
const setExpanded = (collapsible: HTMLElement, expanded: boolean) => {
    collapsible.classList.toggle('show', expanded);

    if (!collapsible.id) {
        return;
    }

    document.querySelectorAll(`[data-bs-target="#${collapsible.id}"]`)
        .forEach((toggle) => toggle.setAttribute('aria-expanded', String(expanded)));
};

// Marks the selected item and its ancestors, and opens what holds it so that the
// current page is always visible. When the menu is persistent nothing else changes,
// the rest of the menu stays the way the user left it. When it is focused, every
// other section is closed.
const applySelectedNavLink = (nav: HTMLElement, selectedLink: HTMLElement) => {
    nav.querySelectorAll('li.active').forEach(li => li.classList.remove('active'));
    nav.querySelectorAll('li.current').forEach(li => li.classList.remove('current'));

    if (isFocused()) {
        nav.querySelectorAll<HTMLElement>('ul.collapse.show').forEach(ul => setExpanded(ul, false));
    }

    let currentItem = selectedLink.closest('li');
    let isDeepest = true;

    while (currentItem) {
        currentItem.classList.add('active');

        if (isDeepest) {
            currentItem.classList.add('current');
            isDeepest = false;
        } else {
            const childMenu = currentItem.querySelector<HTMLElement>(':scope > figure > ul.collapse');
            if (childMenu) {
                setExpanded(childMenu, true);
            }
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

const getNavCollapsibles = () =>
    Array.from(document.querySelectorAll<HTMLElement>('#left-nav ul.collapse[id]'));

// The menu is rendered fully collapsed. Opens whatever holds the current page so that
// it is always reachable and, when the menu is persistent, what the user left open.
const applyNavStateFromPreferences = () => {
    const collapsibles = getNavCollapsibles();

    if (collapsibles.length === 0) {
        return document.readyState === 'complete';
    }

    const preferences = getAdminPreferences() as Record<string, unknown>;
    const expandedItems = !isFocused() && Array.isArray(preferences.expandedNavItems)
        ? preferences.expandedNavItems as string[]
        : [];

    collapsibles.forEach(collapsible => setExpanded(
        collapsible,
        expandedItems.includes(collapsible.id) || collapsible.querySelector('li.active') !== null));

    return document.readyState === 'complete';
};

// When focused, opening a section closes the other sections of the list it belongs to.
// Bootstrap does it on its own when a collapsible names that list as its parent.
const applyFocusedAccordion = () => {
    if (!isFocused()) {
        return;
    }

    getNavCollapsibles().forEach((collapsible) => {
        const list = collapsible.closest('li')?.parentElement;

        if (list?.id) {
            collapsible.setAttribute('data-bs-parent', `#${list.id}`);
        }
    });
};

// Records the section the user just opened or closed. It is done when the transition
// starts rather than when it ends, so that following a link right after opening a section
// does not lose it, and only for the section the user toggled, so that the section opened
// on its own for the current page is not remembered as one the user chose to open.
const persistNavState = (event: Event) => {
    // A focused menu forgets what was opened: leaving the stored state alone means a
    // persistent menu gets back exactly what it had, should the behavior be switched back.
    if (isFocused()) {
        return;
    }

    const toggled = (event.target as Element | null)?.id;

    if (!toggled) {
        return;
    }

    const preferences = getAdminPreferences() as Record<string, unknown>;
    const stored = Array.isArray(preferences.expandedNavItems) ? preferences.expandedNavItems as string[] : [];

    // Forget the sections the menu does not hold anymore.
    const existing = new Set(getNavCollapsibles().map(collapsible => collapsible.id));
    const expandedItems = new Set(stored.filter(id => existing.has(id)));

    if (event.type === 'show.bs.collapse') {
        expandedItems.add(toggled);
    } else {
        expandedItems.delete(toggled);
    }

    preferences.expandedNavItems = Array.from(expandedItems);
    setAdminPreferences(preferences);
};

// Off canvas menu, on small screens.
const setSidebarOpen = (open: boolean) => {
    document.body.classList.toggle(sidebarOpenClass, open);

    document.querySelectorAll('.ta-sidebar-toggler')
        .forEach(toggler => toggler.setAttribute('aria-expanded', String(open)));
};

// Compact menu flyouts. They are placed by script because the rail scrolls, which
// would otherwise clip them, and because they must stay inside the viewport.
const closeFlyouts = () => {
    document.querySelectorAll<HTMLElement>('#left-nav > ul > li.visible').forEach((item) => {
        item.classList.remove('visible');

        const flyout = item.querySelector<HTMLElement>(':scope > figure > ul');
        if (flyout) {
            flyout.removeAttribute('style');
        }
    });
};

const positionFlyout = (item: HTMLElement) => {
    const flyout = item.querySelector<HTMLElement>(':scope > figure > ul');
    const sidebar = document.getElementById('ta-left-sidebar');

    if (!flyout || !sidebar) {
        return;
    }

    const margin = 8;
    const sidebarRect = sidebar.getBoundingClientRect();
    const itemRect = item.getBoundingClientRect();
    const maxHeight = window.innerHeight - sidebarRect.top - margin;

    flyout.style.position = 'fixed';
    flyout.style.maxHeight = `${maxHeight}px`;

    if (document.documentElement.dir === 'rtl') {
        flyout.style.right = `${window.innerWidth - sidebarRect.left}px`;
    } else {
        flyout.style.left = `${sidebarRect.right}px`;
    }

    const top = Math.min(itemRect.top, window.innerHeight - margin - flyout.offsetHeight);
    flyout.style.top = `${Math.max(sidebarRect.top, top)}px`;
};

const openFlyout = (item: HTMLElement) => {
    if (item.classList.contains('visible')) {
        positionFlyout(item);
        return;
    }

    closeFlyouts();
    item.classList.add('visible');
    positionFlyout(item);
};

// While compact, clicking a group opens its flyout instead of collapsing it, so the
// collapse toggles are turned off and restored when the menu is expanded again.
const setGroupTogglesEnabled = (enabled: boolean) => {
    const selector = enabled
        ? '#left-nav > ul > li > figure > figcaption > [data-bs-toggle-disabled]'
        : '#left-nav > ul > li > figure > figcaption > [data-bs-toggle="collapse"]';

    document.querySelectorAll(selector).forEach((toggle) => {
        if (enabled) {
            toggle.setAttribute('data-bs-toggle', toggle.getAttribute('data-bs-toggle-disabled') ?? 'collapse');
            toggle.removeAttribute('data-bs-toggle-disabled');
        } else {
            toggle.setAttribute('data-bs-toggle-disabled', toggle.getAttribute('data-bs-toggle') ?? 'collapse');
            toggle.removeAttribute('data-bs-toggle');
        }
    });
};

// Applies the behavior matching the layout currently in use. The compact rail only
// exists on medium screens and up, below that the menu is an off canvas drawer where
// the groups keep collapsing as they do in the expanded sidebar.
const syncCompactState = () => {
    const compact = isCompact();

    setGroupTogglesEnabled(!compact);

    if (!compact) {
        closeFlyouts();
    }
};

const initializeMenu = () => {
    if (menuInitialized) {
        return;
    }

    menuInitialized = true;

    // The compact class is added by the user preferences loader before the page is rendered,
    // the rest of the compact behavior can only be applied once the menu exists.
    document.addEventListener('DOMContentLoaded', syncCompactState);

    document.querySelectorAll('span.title').forEach((el) => {
        const icon = el.previousElementSibling;
        if (icon?.classList.contains('icon')) {
            (icon as HTMLElement).title = el.textContent ?? '';
        }
    });

    document.querySelector('.leftbar-compactor')?.addEventListener('click', () => {
        if (document.body.classList.contains(compactClass)) {
            unSetCompactStatus();
        } else {
            setCompactStatus(true);
        }
    });

    document.querySelectorAll('.ta-sidebar-toggler').forEach((toggler) => {
        toggler.addEventListener('click', () => {
            setSidebarOpen(!document.body.classList.contains(sidebarOpenClass));
        });
    });

    document.querySelector('.ta-sidebar-backdrop')?.addEventListener('click', () => setSidebarOpen(false));

    document.addEventListener('keydown', (event) => {
        // Escape clears the menu filter, it must not close the menu holding it.
        if (event.key === 'Escape' && (event.target as Element)?.id !== 'filter') {
            setSidebarOpen(false);
            closeFlyouts();
        }
    });

    document.querySelectorAll<HTMLElement>('#left-nav > ul > li.has-items').forEach((item) => {
        item.addEventListener('mouseenter', () => {
            if (isCompact()) {
                openFlyout(item);
            }
        });

        item.addEventListener('mouseleave', () => {
            if (isCompact()) {
                closeFlyouts();
            }
        });

        item.addEventListener('focusin', () => {
            if (isCompact()) {
                openFlyout(item);
            }
        });

        item.addEventListener('click', () => {
            if (isCompact()) {
                openFlyout(item);
            }
        });
    });

    document.addEventListener('click', (event) => {
        if (!(event.target as Element)?.closest('#left-nav > ul > li.has-items')) {
            closeFlyouts();
        }
    });

    const leftNav = document.getElementById('left-nav');

    // When navigating via a real nav link, persist the selected item hash inside the
    // existing admin preferences cookie so the server can restore the correct selection,
    // and inside session storage so the hash survives an in-page (Turbo-style) navigation.
    leftNav?.addEventListener('click', (event) => {
        const link = (event.target as Element)?.closest<HTMLElement>('a[data-admin-hash][href^="/"]');
        if (!link) {
            return;
        }

        const preferences = getAdminPreferences() as Record<string, unknown>;
        preferences.selectedNavHash = String(link.dataset.adminHash);
        setAdminPreferences(preferences);
        persistSelectedNavHash(String(link.dataset.adminHash));

        // On small screens the drawer covers the page, it has to make way for the page being opened.
        if (isMobile()) {
            setSidebarOpen(false);
        }
    });

    applyFocusedAccordion();

    // Remember what the user opened and closed, at every level.
    const adminMenu = document.getElementById('adminMenu');

    adminMenu?.addEventListener('show.bs.collapse', persistNavState);
    adminMenu?.addEventListener('hide.bs.collapse', persistNavState);

    if (leftNav != null) {
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
    document.body.classList.add(compactClass);
    syncCompactState();

    if (explicit === true) {
        setCompactExplicit(true);
    }

    persistAdminPreferences();
};

const unSetCompactStatus = () => {
    document.body.classList.remove(compactClass);
    syncCompactState();

    setCompactExplicit(false);
    persistAdminPreferences();
};

export {
    applyNavStateFromPreferences,
    applySelectedNavFromSessionStorage,
    initializeMenu,
    setCompactStatus,
    setSidebarOpen,
    syncCompactState,
    unSetCompactStatus,
};
