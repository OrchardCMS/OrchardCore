import strenght from "@orchardcore/frontend/components/password-strength";

// Show or hide the connection string or table prefix section when the database provider is selected
const toggleConnectionStringAndPrefix = () => {
    const selectedOption = document.querySelector(
        "#DatabaseProvider option:checked"
    );
    if (selectedOption) {
        const connectionString =
            selectedOption
                .getAttribute("data-connection-string")
                ?.toLowerCase() === "true";
        const tablePrefix =
            selectedOption.getAttribute("data-table-prefix")?.toLowerCase() ===
            "true";

        const connectionStringElements =
            document.querySelectorAll<HTMLDivElement>(".connectionString");
        connectionStringElements.forEach(
            (el) => (el.style.display = connectionString ? "block" : "none")
        );

        const tablePrefixElements =
            document.querySelectorAll<HTMLDivElement>(".tablePrefix");
        tablePrefixElements.forEach(
            (el) => (el.style.display = tablePrefix ? "block" : "none")
        );

        document.querySelectorAll(".pwd").forEach((el) => {
            if (connectionString) {
                el.setAttribute("required", "required");
            } else {
                el.removeAttribute("required");
            }
        });

        const connectionStringHint = document.getElementById(
            "connectionStringHint"
        );
        if (connectionStringHint) {
            connectionStringHint.textContent =
                selectedOption.getAttribute("data-connection-string-sample") ||
                "";
        }
    }
};

const refreshDescription = (target: Element) => {
    const recipeName = target.getAttribute("data-recipe-name");
    const recipeDisplayName = target.getAttribute("data-recipe-display-name");
    const recipeDescription = target.getAttribute("data-recipe-description");

    const recipeButton = document.getElementById("recipeButton");
    const recipeNameInput = document.getElementById(
        "RecipeName"
    ) as HTMLInputElement;

    if (recipeButton && recipeNameInput) {
        recipeButton.textContent = recipeDisplayName || "";
        recipeNameInput.value = recipeName || "";
        recipeButton.setAttribute("title", recipeDescription || "");
        recipeButton.focus();
    }
};

const setLocalizationUrl = () => {
    const culturesList = document.getElementById(
        "culturesList"
    ) as HTMLSelectElement;

    culturesList.addEventListener("change", () => {
        const selectedOption = culturesList.options[culturesList.selectedIndex];
        if (selectedOption) {
            window.location.href = selectedOption.dataset.url || "";
        }
    });
};

const togglePasswordVisibility = (
    passwordCtl: HTMLInputElement | null,
    togglePasswordCtl: HTMLElement | null
) => {
    if (!passwordCtl || !togglePasswordCtl) {
        return;
    }

    // toggle the type attribute
    const type =
        passwordCtl.getAttribute("type") === "password" ? "text" : "password";
    passwordCtl.setAttribute("type", type);

    // toggle the eye slash icon
    const icon = togglePasswordCtl.getElementsByClassName("icon")[0];
    if (icon) {
        if (icon.getAttribute("data-icon")) {
            // if the icon is rendered as a svg
            icon.setAttribute(
                "data-icon",
                type === "password" ? "eye" : "eye-slash"
            );
        } else {
            // if the icon is still a <i> element
            icon.classList.toggle("fa-eye", type === "password");
            icon.classList.toggle("fa-eye-slash", type !== "password");
        }
    }
};

const init = () => {
    toggleConnectionStringAndPrefix();

    // Show hide the connection string when a provider is selected
    document
        .getElementById("DatabaseProvider")
        ?.addEventListener("change", function () {
            toggleConnectionStringAndPrefix();
        });

    // Refresh the recipe description
    document.querySelectorAll("#recipes div a").forEach(function (element) {
        element.addEventListener("click", function () {
            refreshDescription(this);
        });
    });

    const passwordElement = <HTMLInputElement>(
        document.getElementById("Password")
    );

    const options = JSON.parse(passwordElement?.dataset.strength ?? "") ?? {
        requiredLength: 6,
        requiredUniqueChars: 1,
        requireNonAlphanumeric: true,
        requireLowercase: true,
        requireUppercase: true,
        requireDigit: true,
    };

    if (passwordElement) {
        strenght(passwordElement, options);
    }

    if (passwordElement) {
        passwordElement.addEventListener("focus", function () {
            const popover = document.createElement("div");
            popover.className = "popover bs-popover-top";
            popover.role = "tooltip";
            popover.innerHTML = `<div class="popover-header">Password requirements: </div><div class="popover-body"><ul><li>Minimum length: ${
                options.requiredLength
            }</li><li>Unique Chars: ${
                options.requiredUniqueChars
            }</li><li>Uppercase: ${
                options.requireUppercase ? "required" : "not required"
            }</li><li>Lowercase: ${
                options.requireLowercase ? "required" : "not required"
            }</li><li>Digit: ${
                options.requireDigit ? "required" : "not required"
            }</li><li>Non alphanumeric: ${
                options.requireNonAlphanumeric ? "required" : "not required"
            }</li></ul></div>`;

            const rect = passwordElement.getBoundingClientRect();
            popover.style.position = "absolute";
            popover.style.top = `${rect.top + 53}px`;
            popover.style.left = `${rect.left}px`;
            document.body.appendChild(popover);

            function removePopover() {
                popover.remove();
                passwordElement?.removeEventListener("blur", removePopover);
            }

            passwordElement.addEventListener("blur", removePopover);
        });
    }

    const toggleConnectionString = document.querySelector(
        "#toggleConnectionString"
    );
    if (toggleConnectionString) {
        toggleConnectionString.addEventListener("click", function (e) {
            togglePasswordVisibility(
                document.querySelector("#ConnectionString"),
                document.querySelector("#toggleConnectionString")
            );
        });
    }

    const togglePassword = document.querySelector("#togglePassword");
    togglePassword?.addEventListener("click", function (e) {
        togglePasswordVisibility(
            document.querySelector("#Password"),
            document.querySelector("#togglePassword")
        );
    });

    const togglePasswordConfirmation = document.querySelector(
        "#togglePasswordConfirmation"
    );
    togglePasswordConfirmation?.addEventListener("click", function (e) {
        togglePasswordVisibility(
            document.querySelector("#PasswordConfirmation"),
            document.querySelector("#togglePasswordConfirmation")
        );
    });

    setLocalizationUrl();
};

init();