import intlTelInput from 'intl-tel-input/build/js/intlTelInputWithUtils';
import 'intl-tel-input/build/css/intlTelInput.css';
import './phone-input.css';

document.querySelectorAll<HTMLElement>('[data-phone-input]').forEach((el) => {
    const form = el.closest('form');
    const hiddenInput = form?.querySelector<HTMLInputElement>('[data-phone-e164]');
    const disabled = el.dataset.phoneDisabled === 'True';
    const initialValue = el.dataset.phoneValue ?? '';
    const defaultRegion = el.dataset.phoneRegion ?? '';
    const confirmed = el.dataset.phoneConfirmed === 'True';

    // Build the input group
    const group = document.createElement('div');
    group.className = 'input-group col-md-4 phone-input-group';

    // Create the tel input
    const input = document.createElement('input');
    input.type = 'tel';
    input.className = 'form-control';
    input.disabled = disabled;

    // Wrapper so intl-tel-input's DOM wrapping stays contained
    const itiWrapper = document.createElement('div');
    itiWrapper.className = 'phone-input-iti-wrapper';
    itiWrapper.appendChild(input);
    group.appendChild(itiWrapper);

    // Status icon
    const iconSpan = document.createElement('span');
    iconSpan.className = 'input-group-text';
    group.appendChild(iconSpan);

    el.appendChild(group);

    // Initialize intl-tel-input
    const iti = intlTelInput(input, {
        initialCountry: defaultRegion?.toLowerCase() || 'auto',
        separateDialCode: true,
        formatAsYouType: true,
        nationalMode: true,
        autoPlaceholder: 'aggressive',
        ...(defaultRegion
            ? { preferredCountries: [defaultRegion.toLowerCase()] }
            : {}),
    });

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
});
