function randomUUID() {
    if (typeof crypto === 'object' && typeof crypto.randomUUID === 'function') {
        return crypto.randomUUID();
    }

    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    );
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
