// Manages the sticky action bar at the bottom of admin edit pages.
//
// Views render <div class="action-bar-bottom"> around their actions and mark the secondary ones with
// .action-bar-secondary. The secondary actions are moved to an overflow menu when the actions layout of the
// admin settings is "Menu" (exposed by the layout as body[data-actions-layout]) and always on small screens,
// so that only the main actions, e.g. "Publish" and "Cancel", remain visible. They are moved back to their
// original position when the menu is not used, e.g. when a small screen is rotated or resized.
//
// This file must remain in the foot bundle (TheAdmin.ts) since it queries the live DOM.

declare const bootstrap: typeof import('bootstrap');

const smallScreenQuery = window.matchMedia('(max-width: 767.98px)');

const shouldUseMenu = () => document.body.dataset.actionsLayout === 'menu' || smallScreenQuery.matches;

const createMenu = () => {
    const menu = document.createElement('div');
    menu.className = 'dropup action-bar-menu';

    const toggle = document.createElement('button');
    toggle.type = 'button';
    toggle.className = 'btn btn-outline-secondary action-bar-menu-toggle';
    toggle.setAttribute('data-bs-toggle', 'dropdown');
    toggle.setAttribute('data-bs-auto-close', 'outside');
    toggle.setAttribute('aria-haspopup', 'true');
    toggle.setAttribute('aria-expanded', 'false');

    const label = document.body.dataset.moreActionsLabel || 'More actions';
    toggle.setAttribute('aria-label', label);
    toggle.title = label;
    toggle.innerHTML = '<i class="fa-solid fa-ellipsis-vertical" aria-hidden="true"></i>';

    const items = document.createElement('div');
    items.className = 'dropdown-menu dropdown-menu-end action-bar-menu-items';

    menu.append(toggle, items);

    return { menu, toggle, items };
};

const initActionBar = (bar: HTMLElement) => {
    // Only the outermost secondary elements are moved, nested ones move with their parent.
    const secondaryActions = Array.from(bar.querySelectorAll<HTMLElement>('.action-bar-secondary'))
        .filter(element => !element.parentElement?.closest('.action-bar-secondary'));

    if (secondaryActions.length > 0) {
        const entries = secondaryActions.map(element => {
            const placeholder = document.createComment('action-bar-secondary');
            element.before(placeholder);
            return { element, placeholder };
        });

        const { menu, toggle, items } = createMenu();
        bar.prepend(menu);

        let usesMenu: boolean | null = null;

        const update = () => {
            const useMenu = shouldUseMenu();

            if (useMenu === usesMenu) {
                return;
            }

            usesMenu = useMenu;

            if (!useMenu) {
                bootstrap.Dropdown.getInstance(toggle)?.hide();
            }

            for (const { element, placeholder } of entries) {
                if (useMenu) {
                    items.append(element);
                } else {
                    placeholder.after(element);
                }
            }

            menu.hidden = !useMenu;
        };

        update();
        smallScreenQuery.addEventListener('change', update);
    }

    bar.classList.add('action-bar-ready');
};

(function () {
    const observedBars: HTMLElement[] = [];

    // Lets the content reserve the space covered by the fixed bar.
    const updateHeight = () => {
        const bar = observedBars.filter(b => b.isConnected && b.offsetParent !== null).pop();

        if (bar) {
            document.documentElement.style.setProperty('--oc-action-bar-bottom-height', bar.offsetHeight + 'px');
        } else {
            document.documentElement.style.removeProperty('--oc-action-bar-bottom-height');
        }
    };

    const resizeObserver = new ResizeObserver(updateHeight);

    const initBars = (root: ParentNode) => {
        const bars = Array.from(root.querySelectorAll<HTMLElement>('.action-bar-bottom:not(.action-bar-ready)'));

        if (root instanceof HTMLElement && root.matches('.action-bar-bottom:not(.action-bar-ready)')) {
            bars.unshift(root);
        }

        for (const bar of bars) {
            initActionBar(bar);
            observedBars.push(bar);
            resizeObserver.observe(bar);
        }
    };

    initBars(document);

    // Bars rendered later, e.g. by Vue components, are initialized when they are added to the page.
    new MutationObserver(mutations => {
        let removed = false;

        for (const mutation of mutations) {
            mutation.addedNodes.forEach(node => {
                if (node instanceof HTMLElement) {
                    initBars(node);
                }
            });

            removed ||= mutation.removedNodes.length > 0;
        }

        if (removed) {
            for (let i = observedBars.length - 1; i >= 0; i--) {
                if (!observedBars[i].isConnected) {
                    resizeObserver.unobserve(observedBars[i]);
                    observedBars.splice(i, 1);
                }
            }

            updateHeight();
        }
    }).observe(document.body, { childList: true, subtree: true });
})();
export {};
