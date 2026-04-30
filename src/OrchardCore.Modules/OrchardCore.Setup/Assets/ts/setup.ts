import strenght from "@orchardcore/bloom/components/password-strength";

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
        let popover: HTMLDivElement | null = null;

        const getUniqueCharsCount = (value: string) => new Set(value.split("")).size;

        const evaluatePasswordRules = (value: string) => {
            const length = value.length >= options.requiredLength;
            const uniqueChars = (options.requiredUniqueChars ?? 1) <= 1
                ? true
                : getUniqueCharsCount(value) >= options.requiredUniqueChars;
            const uppercase = !options.requireUppercase || /[A-Z]/.test(value);
            const lowercase = !options.requireLowercase || /[a-z]/.test(value);
            const digit = !options.requireDigit || /[0-9]/.test(value);
            const nonAlphanumeric = !options.requireNonAlphanumeric || /[^\da-zA-Z]/.test(value);

            return { length, uniqueChars, uppercase, lowercase, digit, nonAlphanumeric };
        };

        const buildRequirementItem = (
            label: string,
            ruleName: "length" | "uniqueChars" | "uppercase" | "lowercase" | "digit" | "nonAlphanumeric"
        ) => {
            const li = document.createElement("li");
            li.dataset.passwordRule = ruleName;
            li.className = "password-rule-item text-muted";
            li.textContent = `○ ${label}`;
            return li;
        };

        const createPopover = () => {
            const el = document.createElement("div");
            el.id = "passwordRequirementsPopover";
            el.className =
                "popover bs-popover-top show password-requirements-popover";
            el.role = "tooltip";

            const requirements: HTMLLIElement[] = [];
            requirements.push(buildRequirementItem(`Minimum length: ${options.requiredLength}`, "length"));

            if ((options.requiredUniqueChars ?? 1) > 1) {
                requirements.push(buildRequirementItem(`Unique chars: ${options.requiredUniqueChars}`, "uniqueChars"));
            }

            if (options.requireUppercase) {
                requirements.push(buildRequirementItem("Uppercase: required", "uppercase"));
            }

            if (options.requireLowercase) {
                requirements.push(buildRequirementItem("Lowercase: required", "lowercase"));
            }

            if (options.requireDigit) {
                requirements.push(buildRequirementItem("Digit: required", "digit"));
            }

            if (options.requireNonAlphanumeric) {
                requirements.push(buildRequirementItem("Non alphanumeric: required", "nonAlphanumeric"));
            }

            const arrow = document.createElement("div");
            arrow.className = "popover-arrow";

            const header = document.createElement("div");
            header.className = "popover-header";
            header.textContent = "Password requirements:";

            const body = document.createElement("div");
            body.className = "popover-body";

            const list = document.createElement("ul");
            list.className = "mb-0 ps-0 list-unstyled";
            list.id = "passwordRequirementsPopup";
            requirements.forEach((item) => list.appendChild(item));

            body.appendChild(list);
            el.appendChild(arrow);
            el.appendChild(header);
            el.appendChild(body);

            document.body.appendChild(el);

            const rect = passwordElement.getBoundingClientRect();
            const offset = 8;
            el.style.top = `${
                rect.top + window.scrollY - el.offsetHeight - offset
            }px`;
            el.style.left = `${rect.left + window.scrollX}px`;
            passwordElement.setAttribute("aria-describedby", el.id);

            return el;
        };

        const updatePopoverRules = (value: string) => {
            if (!popover) {
                return;
            }

            const result = evaluatePasswordRules(value);
            const items = popover.querySelectorAll<HTMLLIElement>("#passwordRequirementsPopup li[data-password-rule]");

            items.forEach((item) => {
                const rule = item.dataset.passwordRule as keyof typeof result;
                const met = !!result[rule];

                item.classList.toggle("text-success", met);
                item.classList.toggle("text-muted", !met);
                item.textContent = `${met ? "✓" : "○"} ${item.textContent?.replace(/^[✓○]\s*/, "")}`;
            });
        };

        const onInput = () => updatePopoverRules(passwordElement.value);

        const removePopover = () => {
            popover?.remove();
            popover = null;
            passwordElement.removeAttribute("aria-describedby");
            passwordElement.removeEventListener("input", onInput);
        };

        passwordElement.addEventListener("focus", () => {
            if (!popover) {
                popover = createPopover();
                updatePopoverRules(passwordElement.value);
                passwordElement.addEventListener("input", onInput);
            }
        });

        passwordElement.addEventListener("blur", () => {
            removePopover();
        });
    }

    const toggleConnectionString = document.querySelector(
        "#toggleConnectionString"
    );
    if (toggleConnectionString) {
        toggleConnectionString.addEventListener("click", function () {
            togglePasswordVisibility(
                document.querySelector("#ConnectionString"),
                document.querySelector("#toggleConnectionString")
            );
        });
    }

    const togglePassword = document.querySelector("#togglePassword");
    togglePassword?.addEventListener("click", function () {
        togglePasswordVisibility(
            document.querySelector("#Password"),
            document.querySelector("#togglePassword")
        );
    });

    const togglePasswordConfirmation = document.querySelector(
        "#togglePasswordConfirmation"
    );
    togglePasswordConfirmation?.addEventListener("click", function () {
        togglePasswordVisibility(
            document.querySelector("#PasswordConfirmation"),
            document.querySelector("#togglePasswordConfirmation")
        );
    });

    setLocalizationUrl();
};

init();
