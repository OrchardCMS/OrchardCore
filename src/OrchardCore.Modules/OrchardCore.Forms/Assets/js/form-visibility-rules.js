window.formVisibilityGroupRules = (function () {
    function initialize(data) {

        const inputElement = getInputByName(data.elementName);

        if (!inputElement || data.action === 'None') {
            return;
        }

        if (inputElement.type == 'checkbox' || inputElement.type == 'radio') {
            inputElement.setAttribute('data-default-value', inputElement.checked ? 'on' : 'off');
        } else {
            inputElement.setAttribute('data-default-value', inputElement.value);
        }

        const widgetContainer = inputElement.closest('.widget');

        processGroups(data, inputElement, widgetContainer, true);

        triggerProperChangeEvent(inputElement);
    }

    function triggerProperChangeEvent(element) {

        const tagName = element.tagName;

        const type = (element.type || '').toLowerCase();

        if (tagName === 'SELECT' || type === 'checkbox' || type === 'radio' || type === 'file') {

            element.dispatchEvent(new Event('change'));

            return;
        }

        if (tagName === 'INPUT' || tagName === 'TEXTAREA') {

            element.dispatchEvent(new Event('input'));

            element.dispatchEvent(new Event('keyup'));

            return;
        }

        element.dispatchEvent(new Event('input'));
    }

    function addProperListeners(element, callback) {

        const tagName = element.tagName;

        const type = (element.type || '').toLowerCase();

        if (tagName === 'SELECT') {

            if (type === 'checkbox') {

                element.addEventListener('change', callback);

                return;
            }

            element.addEventListener('change', callback);

            return;
        }

        if (tagName === 'INPUT') {

            if ((type === 'checkbox' || type === 'radio')) {

                element.addEventListener('change', callback);

                element.addEventListener('click', callback);

                return;
            }

            if (type === 'file') {

                element.addEventListener('change', callback);

                return;
            }

            element.addEventListener('input', callback);

            element.addEventListener('keyup', callback);

            return;
        }

        if (tagName === 'TEXTAREA') {

            element.addEventListener('input', callback);

            element.addEventListener('keyup', callback);

            return;
        }

        element.addEventListener('input', callback);
    }

    function processGroups(data, inputElement, widgetContainer, addHandlers) {

        let anyGroupRuleMet = false;

        data.groups.forEach(group => {

            let groupPassed = true;

            group.rules?.forEach(rule => {

                const fieldElement = getInputByName(rule.field);

                if (!fieldElement) {
                    console.warn(`Field element not found: ${rule.field}. Ignoring the bad field.`);
                    return;
                }

                const fieldValue = fieldElement.type === 'checkbox'
                    ? (fieldElement.checked ? "true" : "false")
                    : fieldElement.value;

                var validationResult = validateRule(fieldValue, rule);

                if (groupPassed && !validationResult) {

                    groupPassed = false;
                }

                if (addHandlers) {

                    addProperListeners(fieldElement, (e) => {

                        processGroups(data, inputElement, widgetContainer, false);
                    });
                }
            });

            anyGroupRuleMet = anyGroupRuleMet || groupPassed;
        });

        if (widgetContainer) {

            if (data.action === 'Show') {

                if (anyGroupRuleMet) {
                    widgetContainer.classList.remove('d-none');
                } else {
                    widgetContainer.classList.add('d-none');
                    restoreOriginalState(inputElement);
                }
            }

            else if (data.action === 'Hide') {

                if (anyGroupRuleMet) {
                    widgetContainer.classList.add('d-none');
                    restoreOriginalState(inputElement);
                } else {
                    widgetContainer.classList.remove('d-none');
                }
            }
        }
    }

    function restoreOriginalState(inputElement) {

        var originalValue = inputElement.getAttribute('data-default-value') || '';

        if (inputElement.type == 'checkbox' || inputElement.type == 'radio') {

            inputElement.checked = originalValue == 'on';
        } else {
            inputElement.value = originalValue;
        }

        triggerProperChangeEvent(inputElement);
    }

    function getInputByName(name) {
        return document.querySelector(`input[name="${name}"],select[name="${name}"],textarea[name="${name}"]`);
    }

    function validateRule(inputValue, rule) {

        if (!rule.operator) {
            console.warn("Rule operator is missing for rule", rule);
            return false;
        }

        var lowerInputValue = inputValue ? inputValue.trim().toLowerCase() : "";

        var lowerRuleValue = rule.value ? rule.value.trim().toLowerCase() : "";

        switch (rule.operator) {
            case 'Is':
                return lowerInputValue === lowerRuleValue;

            case 'IsNot':
                return lowerInputValue !== lowerRuleValue;

            case 'Contains':
                return lowerInputValue.includes(lowerRuleValue);

            case 'DoesNotContain':
                return !lowerInputValue.includes(lowerRuleValue);

            case 'StartsWith':
                return lowerInputValue.startsWith(lowerRuleValue);

            case 'EndsWith':
                return lowerInputValue.endsWith(lowerRuleValue);

            case 'GreaterThan':
                var numberInputValue = parseFloat(inputValue);
                var numberRuleValue = parseFloat(rule.value);

                if (!isNaN(numberInputValue) && !isNaN(numberRuleValue)) {
                    return numberInputValue > numberRuleValue;
                }
                return inputValue > rule.value;

            case 'LessThan':
                var numberInputValue = parseFloat(inputValue);
                var numberRuleValue = parseFloat(rule.value);

                if (!isNaN(numberInputValue) && !isNaN(numberRuleValue)) {
                    return numberInputValue < numberRuleValue;
                }
                return inputValue < rule.value;

            case 'Empty':
                return lowerInputValue === "";

            case 'NotEmpty':
                return lowerInputValue !== "";

            default:
                console.warn(`validateRule: Unknown operator "${rule.operator}" in rule`, rule);
                return false;
        }
    }

    return {
        initialize: initialize
    };

})();
