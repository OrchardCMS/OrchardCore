/**
 * This function initializes a password strength checker on a given input element.
 * It evaluates the password based on specified requirements such as minimum length,
 * presence of uppercase, lowercase, digits, and special characters.
 * A visual progress bar is displayed to indicate the strength level of the password.
 *
 * @param {HTMLElement} element - The input element to which the strength checker is applied.
 * @param {Object} options - Configuration options for password requirements and display settings.
 * @param {number} options.requiredLength - Minimum required length of the password.
 * @param {boolean} options.requireUppercase - Whether an uppercase letter is required.
 * @param {boolean} options.requireLowercase - Whether a lowercase letter is required.
 * @param {boolean} options.requireDigit - Whether a digit is required.
 * @param {boolean} options.requireNonAlphanumeric - Whether a special character is required.
 * @param {string} options.target - CSS selector for the element where the strength progress bar is displayed.
 * @param {string} options.style - CSS style string for the progress bar.
 */
export default (element: HTMLInputElement, options: any) => {

    const settings = Object.assign({
        requiredLength: 8,
        requireUppercase: false,
        requireLowercase: false,
        requireDigit: false,
        requireNonAlphanumeric: false,
        target: '#passwordStrength',
        style: "margin-top: 7px; height: 7px; border-radius: 5px"
    }, options);

    let capitalletters = 0;
    let lowerletters = 0;
    let numbers = 0;
    let specialchars = 0;

    const upperCase = /[A-Z]/;
    const lowerCase = /[a-z]/;
    const number = /[0-9]/;
    const specialchar = /[^\da-zA-Z]/;

    let valid = false;

    const getPercentage = (a: number, b: number) => (b / a) * 100;

    const getLevel = (value: number) => {
        if (value >= 100) {
            return "bg-success";
        }

        if (value >= 50) {
            return "bg-warning";
        }

        if (value == 0) {
            return ''; // grayed
        }

        return "bg-danger";
    };

    const checkStrength = (value: string) => {

        const minLength = value.length >= settings.requiredLength ? 1 : 0;
        capitalletters = !settings.requireUppercase || value.match(upperCase) ? 1 : 0;
        lowerletters = !settings.requireLowercase || value.match(lowerCase) ? 1 : 0;
        numbers = !settings.requireDigit || value.match(number) ? 1 : 0;
        specialchars = !settings.requireNonAlphanumeric || value.match(specialchar) ? 1 : 0;

        const total = minLength + capitalletters + lowerletters + numbers + specialchars;
        const percentage = getPercentage(5, total);

        valid = percentage >= 100;

        createProgressBar(percentage, getLevel(percentage));
    };

    const createProgressBar = (percentage: string | number, level: string) => {
        const el = document.createElement("div");
        el.className = "progress";
        el.setAttribute("value", percentage.toString());
        el.setAttribute("style", settings.style);
        el.setAttribute("max", "100");
        el.setAttribute("aria-describedby", "");
        const bar = document.createElement("div");
        bar.className = "progress-bar " + level;
        bar.style.width = percentage + "%";
        el.appendChild(bar);
        const target = document.querySelector(settings.target);
        target.innerHTML = "";
        target.appendChild(el);
    };

    element.addEventListener("keyup", () => checkStrength(element.value));
    element.addEventListener("keydown", () => checkStrength(element.value));
    element.addEventListener("change", () => checkStrength(element.value));
    element.addEventListener("drop", (event) => {
        event.preventDefault();
        checkStrength(event.dataTransfer?.getData("text") ?? "");
    });

    element.form?.addEventListener("submit", (event) => {
        checkStrength(element.value);
        if (!valid) {
            event.preventDefault();
        }
    });
}
