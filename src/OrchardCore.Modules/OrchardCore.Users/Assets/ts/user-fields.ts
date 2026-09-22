import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

const toggleIcon = (element: Element) => {
    if (element.classList.contains("fa-eye")) {
        element.classList.remove("fa-eye");
        element.classList.add("fa-eye-slash");

        return;
    }

    element.classList.remove("fa-eye-slash");
    element.classList.add("fa-eye");
};

const toggleFieldType = (element: HTMLInputElement) => {
    element.type = element.type === "password" ? "text" : "password";
};

const initPasswordGeneratorWrapper = (element: HTMLElement) => {
    const password = element.querySelector<HTMLInputElement>(".password-input-field");
    const passwordConfirmation = element.querySelector<HTMLInputElement>(".password-confirmation-input-field");
    const passwordToggle = element.querySelector<HTMLElement>(".password-toggle-button");
    const passwordConfirmationToggle = element.querySelector<HTMLElement>(".password-confirmation-toggle-button");
    const generate = element.querySelector<HTMLElement>(".password-generator-button");
    const copy = element.querySelector<HTMLElement>(".copy-password-button");

    if (!password || !passwordConfirmation || !passwordToggle || !passwordConfirmationToggle || !generate || !copy) {
        return;
    }

    generate.addEventListener("click", () => {
        const requiredPasswordLength = parseInt(generate.getAttribute("data-password-length") ?? "0", 10);
        const requireUppercase = generate.getAttribute("data-password-requireUppercase") === "true";
        const requireLowercase = generate.getAttribute("data-password-requireLowercase") === "true";
        const requireDigit = generate.getAttribute("data-password-requireDigit") === "true";
        const requireNonAlphanumeric = generate.getAttribute("data-password-requireNonAlphanumeric") === "true";
        // NOTE: RequiredUniqueChars is actually a count (e.g. "4"), never the literal string "true" -
        // this comparison is always false, a pre-existing bug in the original inline script kept
        // as-is here (unrelated to this extraction; the actual no-op is in password-generator.js's
        // `requiredUniqueChars | 1` coercion, a file outside this pass's scope).
        const requiredUniqueChars = generate.getAttribute("data-password-requiredUniqueChars") === "true";

        const newPassword = passwordManager.generatePassword(
            requiredPasswordLength,
            requireUppercase,
            requireLowercase,
            requireDigit,
            requireNonAlphanumeric,
            requiredUniqueChars,
        );

        password.value = newPassword;
        passwordConfirmation.value = newPassword;
    });

    copy.addEventListener("click", () => {
        passwordManager.copyPassword(password.value);
    });

    const togglePasswordFieldState = () => {
        toggleFieldType(password);
        toggleFieldType(passwordConfirmation);

        const toggleIconElement = passwordToggle.querySelector(".toggle-icon");
        const confirmationToggleIconElement = passwordConfirmationToggle.querySelector(".toggle-icon");

        if (toggleIconElement) {
            toggleIcon(toggleIconElement);
        }

        if (confirmationToggleIconElement) {
            toggleIcon(confirmationToggleIconElement);
        }
    };

    passwordToggle.addEventListener("click", togglePasswordFieldState);
    passwordConfirmationToggle.addEventListener("click", togglePasswordFieldState);
};

observeAndInit(".password-generator-wrapper", initPasswordGeneratorWrapper);
