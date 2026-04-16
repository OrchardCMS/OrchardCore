interface PhoneInputOptions {
    el: HTMLElement;
    input: HTMLInputElement;
    iti: any;
    hiddenInput?: HTMLInputElement | null;
}

/**
 * Initializes a phone input UI with an input group, validation icons, and hidden input syncing.
 * Builds the DOM structure (input group, tel input wrapper, status icon) inside the given element,
 * then binds to the provided intl-tel-input instance for validation and formatting.
 *
 * @param options.el - The container element with data-phone-* attributes.
 * @param options.iti - An initialized intl-tel-input instance.
 * @param options.hiddenInput - Optional hidden input to sync the E.164 number to.
 */
export default ({ el, input, iti, hiddenInput }: PhoneInputOptions) => {
    const confirmed = el.dataset.phoneConfirmed === 'True';
    const initialValue = el.dataset.phoneValue ?? '';

    // Both icons are overlaid inside the iti wrapper to avoid Bootstrap addon
    // background/border-radius conflicts.
    const itiWrapper = input.closest<HTMLElement>('.phone-input-iti-wrapper')
        ?? input.parentElement!;

    // Status icon
    const iconSpan = document.createElement('span');
    iconSpan.className = 'phone-input-status-icon';
    itiWrapper.appendChild(iconSpan);

    function updateIcon() {
        const number = iti.getNumber();
        const hasValue = number.length > 0;
        let iconClass: string;
        let title: string;
        let colorClass: string;

        if (confirmed && !hasValue) {
            iconClass = 'fa-solid fa-check-circle';
            colorClass = 'text-success';
            title = 'Verified';
        } else if (hasValue && iti.isValidNumber()) {
            iconClass = 'fa-solid fa-check';
            colorClass = 'text-success';
            title = 'Valid';
        } else if (hasValue) {
            iconClass = 'fa-solid fa-xmark';
            colorClass = 'text-danger';
            title = 'Invalid';
        } else {
            iconClass = 'fa-solid fa-circle-exclamation';
            colorClass = '';
            title = 'Unverified';
        }

        iconSpan.innerHTML = `<i class="${iconClass} ${colorClass}" title="${title}"></i>`;
    }

    function syncHiddenInput() {
        if (hiddenInput) {
            hiddenInput.value = iti.getNumber();
        }
    }

    input.addEventListener('input', () => {
        updateIcon();
        syncHiddenInput();
    });

    input.addEventListener('countrychange', () => {
        updateIcon();
        syncHiddenInput();
    });

    if (initialValue) {
        iti.setNumber(initialValue);
    }

    updateIcon();
    syncHiddenInput();
};
