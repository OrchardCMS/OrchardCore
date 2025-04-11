window.formVisibilityGroupRules = (function () {
    function initialize(data) {

        if (!data.elementName || data.action == 'None' || !data.groups.length) {
            return;
        }

        const inputElement = getInputByName(data.elementName);

        if (!inputElement) {
            return;
        }
        // here we have the inputs Name
        data.groups.forEach(group => {

            group.rules.forEach(rule => {

                const ruleElement = getInputByName(rule.field);

                if (!ruleElement) {
                    console.warn(`Rule element not found: ${data.elementName}`);
                    return;
                }

                userInputValue(ruleElement, rule, validateRule, getInputByName, data);

                // does this value meet that rule
                if (!validateRule(ruleElement.value, rule)) {
                    console.log(`Rule Not met: ${rule.field} ${rule.operator} ${rule.value}`);
                    return;
                }
            });
        });

        // find widgetContainer show or hide
        const widgetContainer = inputElement.closest('.widget');
        if (widgetContainer) {
            if (data.action.toLowerCase() === 'show') {
                widgetContainer.classList.remove('d-none');
            } else if (data.action.toLowerCase() === 'hide') {
                widgetContainer.classList.add('d-none');
            }
        }
    }

    function getInputByName(name) {
        return document.querySelector(`input[name="${name}"],select[name="${name}"],textarea[name="${name}"]`);
    }

    function validateRule(inputValue, rule) {

        if (!rule.operator) {
            console.warn("Rule operator is missing for rule", rule);
            return false;
        }

        var operator = rule.operator.toLowerCase();

        var lowerInputValue = inputValue ? inputValue.toLowerCase() : "";

        var lowerRuleValue = rule.value ? rule.value.toLowerCase() : "";

        var numberInputValue = parseFloat(inputValue);

        var numberRuleValue = parseFloat(rule.value);

        switch (operator) {
            case 'is':
                return lowerInputValue === lowerRuleValue;

            case 'isnot':
                return lowerInputValue !== lowerRuleValue;

            case 'contains':
                return lowerInputValue.includes(lowerRuleValue);

            case 'doesnotcontain':
                return !lowerInputValue.includes(lowerRuleValue);

            case 'startswith':
                return lowerInputValue.startsWith(lowerRuleValue);

            case 'endswith':
                return lowerInputValue.endsWith(lowerRuleValue);

            case 'greaterthan':
                if (!isNaN(numberInputValue) && !isNaN(numberRuleValue)) {
                    return numberInputValue > numberRuleValue;
                }
                return inputValue > rule.value;

            case 'lessthan':
                if (!isNaN(numberInputValue) && !isNaN(numberRuleValue)) {
                    return numberInputValue < numberRuleValue;
                }
                return inputValue < rule.value;

            case 'empty':
                return lowerInputValue === "";

            case 'notempty':
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

function userInputValue(ruleElement, rule, validateRule, getInputByName, data) {
    function handleRuleEvent(event) {
        let currentValue;

        if (event.target.type === 'checkbox') {
            currentValue = event.target.checked ? 'true' : 'false';
        } else {
            currentValue = event.target.value;
        }

        if (validateRule(currentValue, rule)) {
            getInputByName(data.elementName).closest('.widget').classList.remove('d-none');
        } else {
            getInputByName(data.elementName).closest('.widget').classList.add('d-none');
        }
    }

    ruleElement.addEventListener('keyup', handleRuleEvent);
    ruleElement.addEventListener('change', handleRuleEvent);
}
