const randomUUID = (options = {}) => {
    // Default options.
    const defaultOptions = {
        includeHyphens: true
    };

    // Extend the default options with the provided options.
    const config = { ...defaultOptions, ...options };

    let value;
    if (typeof crypto === 'object' && typeof crypto.randomUUID === 'function') {
        value = crypto.randomUUID();
    } else {

        value = ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }

    if (!config.includeHyphens) {
        return value.replaceAll('-', '');
    }

    return value;
}

const togglePasswordVisibility = (passwordCtl, togglePasswordCtl) => {
    // toggle the type attribute
    type = passwordCtl.getAttribute('type') === 'password' ? 'text' : 'password';
    passwordCtl.setAttribute('type', type);

    // toggle the eye slash icon
    icon = togglePasswordCtl.getElementsByClassName('icon')[0];
    if (icon.getAttribute('data-icon')) { // if the icon is rendered as a svg
        type === 'password' ? icon.setAttribute('data-icon', 'eye') : icon.setAttribute('data-icon', 'eye-slash');
    }
    else { // if the icon is still a <i> element
        type === 'password' ? icon.classList.remove('fa-eye-slash') : icon.classList.remove('fa-eye');
        type === 'password' ? icon.classList.add('fa-eye') : icon.classList.add('fa-eye-slash');
    }
}

const copyToClipboard = (str) => {
    return navigator.clipboard.writeText(str);
};

const generateStrongPassword = (options = {}) => {

    // Default options.
    const defaultOptions = {
        generateBase64: false
    };

    // Extend the default options with the provided options.
    const config = { ...defaultOptions, ...options };

    // Create a Uint8Array with 32 bytes (256 bits).
    const array = new Uint8Array(32);

    crypto.getRandomValues(array);

    if (config.generateBase64) {
        // Convert the array to a binary string.
        let binaryString = '';
        array.forEach(byte => {
            binaryString += String.fromCharCode(byte);
        });

        // Convert the binary string to a Base64 string.
        return btoa(binaryString);
    }

    // Convert the array to a hexadecimal string.
    let hexString = '';
    array.forEach(byte => {
        hexString += byte.toString(16).padStart(2, '0');
    });

    return hexString;
}
