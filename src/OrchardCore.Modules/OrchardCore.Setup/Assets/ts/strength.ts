export default function strength(element, options) {

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

    function getPercentage(a, b) {
        return (b / a) * 100;
    }

    function getLevel(value) {

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
    }

    function checkStrength(value) {

        const minLength = value.length >= settings.requiredLength ? 1 : 0;
        capitalletters = !settings.requireUppercase || value.match(upperCase) ? 1 : 0;
        lowerletters = !settings.requireLowercase || value.match(lowerCase) ? 1 : 0;
        numbers = !settings.requireDigit || value.match(number) ? 1 : 0;
        specialchars = !settings.requireNonAlphanumeric || value.match(specialchar) ? 1 : 0;

        const total = minLength + capitalletters + lowerletters + numbers + specialchars;
        const percentage = getPercentage(5, total);

        valid = percentage >= 100;

        createProgressBar(percentage, getLevel(percentage));
    }

    function createProgressBar(percentage, level) {
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
    }

    element.addEventListener("keyup", (event) => checkStrength(element.value));
    element.addEventListener("keydown", (event) => checkStrength(element.value));
    element.addEventListener("change", (event) => checkStrength(element.value));
    element.addEventListener("drop", (event) => {
        event.preventDefault();
        checkStrength(event.dataTransfer.getData("text"));
    });

    element.form.addEventListener("submit", (event) => {
        checkStrength(element.value);
        if (!valid) {
            event.preventDefault();
        }
    });
}
