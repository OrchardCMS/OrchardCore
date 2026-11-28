declare const bootstrap: typeof import('bootstrap');

const actionMenuSelector = '.admin-list-actions-menu, .action-bar-menu';
const toggleSelector = '[data-bs-toggle="dropdown"]';

type DropdownHideEvent = Event & { clickEvent?: MouseEvent };

// Bootstrap closes every open dropdown on a click, including the ancestors of a nested toggle.
document.addEventListener('hide.bs.dropdown', (event: DropdownHideEvent) => {
    const toggle = event.target;
    const target = event.clickEvent?.target;

    if (!(toggle instanceof HTMLElement) || !toggle.closest(actionMenuSelector) || !(target instanceof Element)) {
        return;
    }

    const nestedToggle = target.closest(toggleSelector);
    const menu = toggle.parentElement?.querySelector(':scope > .dropdown-menu');

    if (nestedToggle && menu?.contains(nestedToggle)) {
        event.preventDefault();
    }
});

document.addEventListener('hidden.bs.dropdown', event => {
    const toggle = event.target;

    if (!(toggle instanceof HTMLElement) || !toggle.closest(actionMenuSelector)) {
        return;
    }

    const menu = toggle.parentElement?.querySelector(':scope > .dropdown-menu');
    menu?.querySelectorAll<HTMLElement>(`${toggleSelector}[aria-expanded="true"]`).forEach(child => {
        bootstrap.Dropdown.getInstance(child)?.hide();
    });
});

// Expanding an inline submenu changes the size and potentially the placement of its outer menu.
for (const eventName of ['shown.bs.dropdown', 'hidden.bs.dropdown']) {
    document.addEventListener(eventName, event => {
        const toggle = event.target;

        if (!(toggle instanceof HTMLElement) || !toggle.closest(actionMenuSelector)) {
            return;
        }

        let menu = toggle.closest('.dropdown-menu');
        while (menu) {
            const parentToggle = menu.parentElement?.querySelector<HTMLElement>(`:scope > ${toggleSelector}`);
            if (parentToggle) {
                bootstrap.Dropdown.getInstance(parentToggle)?.update();
            }
            menu = menu.parentElement?.closest('.dropdown-menu') ?? null;
        }
    });
}

export {};
