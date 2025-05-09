window.formVisibilityGroupRules = (function () {
    function initialize(data) {

        const inputElement = getInputByName(data.elementName);

        if (!inputElement || data.action === 'None') {
            return;
        }

        const inputType = inputElement.type.toLowerCase();

        if (inputType === 'checkbox' || inputType === 'radio') {
            inputElement.setAttribute('data-default-value', inputElement.checked ? 'on' : 'off');
        } else {
            inputElement.setAttribute('data-default-value', inputElement.value);
        }

        const widgetContainer = inputElement.closest('.widget');

        if (widgetContainer && !widgetContainer.dataset.originalDisplay) {
            widgetContainer.dataset.originalDisplay = getComputedStyle(widgetContainer).display;
        }

        processGroups(data, inputElement, widgetContainer, true);

        triggerProperChangeEvent(inputElement);
    }

    function triggerProperChangeEvent(element) {

        const tagName = element.tagName;

        const type = (element.type || '').toLowerCase();

        if (tagName === 'SELECT' || type === 'file') {

            element.dispatchEvent(new Event('change'));

            return;
        }

        if (type === 'checkbox' || type === 'radio') {
            document.querySelectorAll(`input[name="${element.name}"]`).forEach(element => {
                element.dispatchEvent(new Event('change'));
            });
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

            if (type === 'checkbox') {

                var checkboxes = document.querySelectorAll(`input[name="${element.name}"]`);

                for (var i = 0; i < checkboxes.length; i++) {

                    var checkbox = checkboxes[i];

                    checkbox.addEventListener('change', callback);
                }
                return;
            }

            if (type === 'radio') {

                var radioButtons = document.querySelectorAll(`input[name="${element.name}"]`);

                for (var i = 0; i < radioButtons.length; i++) {

                    var radio = radioButtons[i];

                    radio.addEventListener('change', callback);
                }
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

                const fieldType = fieldElement.type.toLowerCase();

                if (fieldType === 'radio') {

                    var selectedRadio = document.querySelector('input[name="' + rule.field + '"]:checked');

                    fieldValue = selectedRadio ? selectedRadio.value : '';

                } else if (fieldType === 'checkbox') {

                    const findCheckboxes = `input[name="${rule.field}"]`;

                    var checkboxWidget = document.querySelectorAll(findCheckboxes);

                    if (checkboxWidget.length > 1) {

                        var checkedBoxes = document.querySelectorAll(`${findCheckboxes}:checked`);

                        fieldValue = Array.from(checkedBoxes).map(checkbox => checkbox.value).join(',');
                    } else {
                        fieldValue = fieldElement.checked ? "true" : "false";
                    }
                } else {
                    fieldValue = fieldElement.value;
                }

                var validationResult = validateRule(fieldValue, rule, rule.caseSensitive);

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
                    showElement(widgetContainer);
                } else {
                    hideElement(widgetContainer);
                    restoreOriginalState(inputElement);
                }
            }
            else if (data.action === 'Hide') {

                if (anyGroupRuleMet) {
                    hideElement(widgetContainer);
                    restoreOriginalState(inputElement);
                } else {
                    showElement(widgetContainer);
                }
            }
        }
    }

    function restoreOriginalState(inputElement) {

        var originalValue = inputElement.getAttribute('data-default-value') || '';

        var type = inputElement.type.toLowerCase();

        if (type === 'checkbox' || type === 'radio') {
            document.querySelectorAll(`input[name="${inputElement.name}"]`)
                .forEach(element => {
                    element.checked = (originalValue === 'on');
                });
        } else {
            inputElement.value = originalValue;
        }

        triggerProperChangeEvent(inputElement);
    }

    function hideElement(element) {

        if (!element.dataset.originalDisplay) {

            element.dataset.originalDisplay = getComputedStyle(element).display;
        }

        element.style.display = "none";
    }

    function showElement(element) {

        element.style.display = element.dataset.originalDisplay || "block";
    }

    function getInputByName(name) {
        return document.querySelector(`input[name="${name}"],select[name="${name}"],textarea[name="${name}"]`);
    }

    function validateRule(inputValue, rule, caseSensitive) {

        if (!rule.operator) {
            console.warn("Rule operator is missing for rule", rule);
            return false;
        }

        var rawInputValue = inputValue ? inputValue.trim() : '';
        var rawRuleValue = rule.value ? rule.value.trim() : '';

        if (!caseSensitive) {
            rawInputValue = rawInputValue.toLowerCase();
            rawRuleValue = rawRuleValue.toLowerCase();
        }

        var inputNumber = getNumeric(rawInputValue);
        var ruleNumber = getNumeric(rawRuleValue);

        switch (rule.operator) {

            case 'Is':
                if (!isNaN(inputNumber) && !isNaN(ruleNumber)) {
                    return inputNumber === ruleNumber;
                }
                return rawInputValue === rawRuleValue;

            case 'IsNot':
                if (!isNaN(inputNumber) && !isNaN(ruleNumber)) {
                    return inputNumber !== ruleNumber;
                }
                return rawInputValue !== rawRuleValue;

            case 'Contains':
                return rawInputValue.includes(rawRuleValue);

            case 'DoesNotContain':
                return !rawInputValue.includes(rawRuleValue);

            case 'StartsWith':
                return rawInputValue.startsWith(rawRuleValue);

            case 'EndsWith':
                return rawInputValue.endsWith(rawRuleValue);

            case 'GreaterThan':
                if (!isNaN(inputNumber) && !isNaN(ruleNumber)) {
                    return inputNumber > ruleNumber;
                }
                return false;

            case 'LessThan':
                if (!isNaN(inputNumber) && !isNaN(ruleNumber)) {
                    return inputNumber < ruleNumber;
                }
                return false;

            case 'Empty':
                return rawInputValue === '';

            case 'NotEmpty':
                return rawInputValue !== '';

            default:
                console.warn(`validateRule: Unknown operator "${rule.operator}" in rule`, rule);
                return false;
        }
    }

    function getNumeric(value) {

        var raw = (value || '').trim();

        var lang = document.documentElement.lang || 'en-US';

        var parts = new Intl.NumberFormat(lang, {
            style: 'decimal',
            minimumFractionDigits: 0,
            maximumFractionDigits: 20
        }).formatToParts(12345.6);

        var groupingSeparator = parts.find(part => part.type === 'group')?.value || ',';

        var decimalSeparator = parts.find(part => part.type === 'decimal')?.value || '.';

        var cleanedValue = raw.split(groupingSeparator).join('');

        var normalized = cleanedValue.split(decimalSeparator).join('.');

        var result = parseFloat(normalized);

        return result;
    }

    return {
        initialize: initialize
    };
})();
