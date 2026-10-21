import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import generatePassword from "@orchardcore/bloom/helpers/passwordGenerator";

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
        const newPassword = generatePassword({
            length: parseInt(generate.getAttribute("data-password-length") ?? "0", 10) || 0,
            requireUppercase: generate.getAttribute("data-password-requireUppercase") === "true",
            requireLowercase: generate.getAttribute("data-password-requireLowercase") === "true",
            requireDigit: generate.getAttribute("data-password-requireDigit") === "true",
            requireNonAlphanumeric: generate.getAttribute("data-password-requireNonAlphanumeric") === "true",
            requiredUniqueChars: parseInt(generate.getAttribute("data-password-requiredUniqueChars") ?? "1", 10) || 1,
        });

        password.value = newPassword;
        passwordConfirmation.value = newPassword;
    });

    copy.addEventListener("click", () => {
        navigator.clipboard.writeText(password.value);
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
