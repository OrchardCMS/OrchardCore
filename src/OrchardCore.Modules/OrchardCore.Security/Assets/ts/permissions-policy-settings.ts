const changeOrigin = (radio: HTMLInputElement) => {
    const allowedOrigins = document.getElementById(`${radio.name}AllowedOrigins`) as HTMLInputElement | null;
    if (!allowedOrigins) {
        return;
    }

    let origin = radio.dataset.originValue ?? "";

    if (radio.value === "Self") {
        allowedOrigins.classList.remove("d-none");
        allowedOrigins.classList.add("d-block");

        if (allowedOrigins.value !== "") {
            origin += ` ${allowedOrigins.value.trim()}`;
        }
    } else {
        allowedOrigins.classList.remove("d-block");
        allowedOrigins.classList.add("d-none");
    }

    const permissionValue = allowedOrigins.parentElement?.querySelector<HTMLInputElement>('input[type="hidden"]');
    if (permissionValue) {
        permissionValue.value = origin;
    }
};

const changeAllowedOrigins = (allowedOrigins: HTMLInputElement) => {
    const permissionValue = allowedOrigins.parentElement?.querySelector<HTMLInputElement>('input[type="hidden"]');

    if (permissionValue && permissionValue.value.startsWith("self")) {
        permissionValue.value = `self ${allowedOrigins.value.trim()}`;
    }
};

document.querySelectorAll<HTMLInputElement>(".permissions-policy-origin-radio").forEach((radio) => {
    radio.addEventListener("change", () => changeOrigin(radio));
});

document.querySelectorAll<HTMLInputElement>(".permissions-policy-allowed-origins").forEach((input) => {
    input.addEventListener("input", () => changeAllowedOrigins(input));
});
