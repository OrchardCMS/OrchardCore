import intlTelInput from 'intl-tel-input/build/js/intlTelInputWithUtils';
import phoneInput from '@orchardcore/bloom/components/phone-input';
import 'intl-tel-input/build/css/intlTelInput.css';
import './phone-input.css';

document.querySelectorAll<HTMLElement>('[data-phone-input]').forEach((el) => {
    const hiddenInput = el.nextElementSibling?.matches('[data-phone-e164]')
        ? el.nextElementSibling as HTMLInputElement
        : el.closest('form')?.querySelector<HTMLInputElement>('[data-phone-e164]');
    const disabled = el.dataset.phoneDisabled === 'True';
    const required = el.dataset.phoneRequired === 'True';
    const defaultRegion = el.dataset.phoneRegion ?? '';

    // Build the wrapper
    const group = document.createElement('div');
    group.className = 'col-md-4 phone-input-group';

    // Create the tel input
    const input = document.createElement('input');
    input.type = 'tel';
    input.className = 'form-control';
    input.disabled = disabled;
    input.required = required;

    // Wrapper so intl-tel-input's DOM wrapping stays contained
    const itiWrapper = document.createElement('div');
    itiWrapper.className = 'phone-input-iti-wrapper';
    itiWrapper.appendChild(input);
    group.appendChild(itiWrapper);

    el.appendChild(group);

    // Initialize intl-tel-input
    const iti = intlTelInput(input, {
        initialCountry: defaultRegion?.toLowerCase() || '',
        separateDialCode: true,
        formatAsYouType: true,
        nationalMode: true,
        autoPlaceholder: 'aggressive',
        ...(defaultRegion
            ? { preferredCountries: [defaultRegion.toLowerCase()] }
            : {}),
    });

    // Delegate validation and syncing to Bloom component
    phoneInput({ el, input, iti, hiddenInput });
});
