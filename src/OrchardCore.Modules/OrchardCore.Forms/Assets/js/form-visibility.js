window.formVisibilityGroups = function () {
    const defaultConfig = {
        template:
            `
        <div class="mb-3">
            <!-- Loop through each group -->
            <div class="card mb-1" v-for="(group, groupIndex) in groups" :key="groupIndex">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span>Group {{ groupIndex + 1 }}</span>
                    <input type="hidden" :name="prefix + 'Groups[' + groupIndex + '].IsRemoved'" value="false" />
                    <button type="button" class="btn btn-sm btn-danger" @click="removeGroup(groupIndex)">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </div>
                        
                <div class="card-body">

                    <!-- Loop through each rule -->
                    <ul class="list-group w-100">
                        <!-- Loop through each rule in the group -->
                        <li class="list-group-item" v-for="(rule, ruleIndex) in group.rules" :key="ruleIndex">
                            <div class="row">
                                <div class="col">
                                    <select class="form-select" v-model="rule.field" :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].Field'">
                                        <option value="">Select Field</option>
                                    <option v-for="option in filteredFieldOptions(rule.field)" :value="option.value">
                                        {{ option.text }}
                                        </option>
                                    </select>
                                </div>
                                <div class="col" :class="{'d-none': !rule.field}">
                                    <select class="form-select" v-model="rule.operator"
                                    :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].Operator'">
                                        <option value="">Select Operator</option>
                                        <option v-for="option in operatorsList(rule.field)" :value="option.value">
                                            {{ option.text }}
                                        </option>
                                    </select>
                                </div>
                                <div class="col" :class="{'d-none': !shouldShowValue(rule.operator)}">
                                    <input type="text" class="form-control" v-model="rule.value" placeholder="Value" :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].Value'" />
                                </div>
                                <div class="col-auto">
                                    <input type="hidden" :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].IsRemoved'" value="false" />
                                    <button type="button" class="btn btn-sm btn-danger" @click="removeRule(groupIndex, ruleIndex)">
                                        <i class="fa-solid fa-trash"></i>
                                    </button>
                                </div>
                            </div>
                        </li>
  
                    </ul>
                </div>

                <div class="card-footer">
                    <div class="d-flex justify-content-end">
                        <button type="button" class="btn btn-sm btn-primary" @click="addRule(groupIndex)">
                            <i class="fa-solid fa-plus"></i> New Rule
                        </button>
                    </div>
                </div>
            </div>
            <div class="d-flex justify-content-end p-3">
                <button type="button" class="btn btn-sm btn-primary" @click="addGroup()">
                    <i class="fa-solid fa-circle-plus"></i> New Group
                </button>
            </div>
        </div>
        `
    };

    const initialize = (instanceConfig) => {
        const config = Object.assign({}, defaultConfig, instanceConfig);
        if (!config.appElementSelector) {
            console.error('appElementSelector is required');
            return;
        }

        const app = new Vue({
            el: config.appElementSelector,
            data() {
                return {
                    groups: config.groupOptions || [],
                    fieldOptions: config.FieldOptions || [],
                    operatorOptions: config.operatorOptions || [],
                    allOperatorOptions: config.operatorOptions || [],
                    prefix: '',
                    widgetId: config.widgetId,
                    preloadedOptions: []
                };
            },

            methods: {
                addGroup() {
                    var newGroup = {
                        rules: [{
                            field: '',
                            operator: '',
                            value: ''
                        }]
                    };

                    this.groups.push(newGroup);
                    this.$set(this.groups, this.groups.length - 1, newGroup);
                },

                addRule(groupIndex) {
                    const newRule = {
                        field: '',
                        operator: '',
                        value: ''
                    };

                    this.$set(this.groups[groupIndex].rules, this.groups[groupIndex].rules.length, newRule);
                },

                removeGroup(groupIndex) {
                    this.groups.splice(groupIndex, 1);
                },

                removeRule(groupIndex, ruleIndex) {
                    this.groups[groupIndex].rules.splice(ruleIndex, 1);
                },

                populateFields() {
                    const inputs = this.getInputs(document);
                    this.fieldOptions = inputs.map(function (input) {
                        return {
                            value: input.htmlName,
                            text: input.htmlName,
                            type: input.htmlInputType
                        };
                    });
                },

                getInputs(el) {
                    const widgetElements = el.querySelectorAll('.widget-template');

                    const results = [];

                    widgetElements.forEach(function (widget) {
                        const formElementNameInput = widget.querySelector('input[name$="FormInputElementPart.Name"]');

                        if (formElementNameInput) {
                            const htmlName = formElementNameInput.value.trim();

                            let selectedOption = 'text';

                            let inputTypeSelect = widget.querySelector('select[name$="InputPart.Type"], select[name$="SelectPart.Editor"]');

                            if (inputTypeSelect) {
                                selectedOption = inputTypeSelect.options[inputTypeSelect.selectedIndex].value.toLowerCase();
                            }

                            if (!htmlName || !selectedOption) {
                                return;
                            }

                            results.push({
                                htmlName: htmlName,
                                htmlInputType: selectedOption
                            });
                        }
                    });
                    return results;
                },

                filteredFieldOptions() {
                    const widgetTemplate = this.$el.closest('.widget-template');

                    if (!widgetTemplate) return this.fieldOptions;

                    const containerName = widgetTemplate.querySelector('input[name$="FormInputElementPart.Name"]')?.value.trim() || "";

                    if (!containerName) {
                        return this.fieldOptions;
                    }

                    const setValues = new Set();

                    const filteredOptions = this.fieldOptions.filter(option => {

                        const optionValue = String(option.value || "").trim();

                        if (optionValue === containerName) {
                            return false;
                        }

                        if (setValues.has(optionValue)) {
                            return false;
                        }

                        setValues.add(optionValue);

                        return true;
                    });

                    return filteredOptions;
                },

                operatorsList(fieldId) {
                    const field = this.fieldOptions.find(field => field.value === fieldId);

                    if (!field) return [];

                    const mapping = this.operatorMapping();

                    if (!mapping[field.type]) return [];

                    return this.allOperatorOptions.filter(x =>
                        mapping[field.type].includes(x.value)
                    );
                },

                operatorMapping() {
                    return {
                        radio: ["Is", "IsNot", "Empty", "NotEmpty", "Contains", "DoesNotContain", "StartsWith", "EndsWith"],
                        checkbox: ["Is", "IsNot"],
                        text: ["Is", "IsNot", "Empty", "NotEmpty", "Contains", "DoesNotContain", "StartsWith", "EndsWith"],
                        number: ["Is", "IsNot", "GreaterThan", "LessThan"],
                        email: ["Is", "IsNot", "Empty", "NotEmpty"],
                        tel: ["Is", "IsNot"],
                        date: ["Is", "IsNot", "GreaterThan", "LessThan"],
                        time: ["Is", "IsNot", "GreaterThan", "LessThan"],
                        "datetime": ["Is", "IsNot", "GreaterThan", "LessThan"],
                        "datetime-local": ["Is", "IsNot", "GreaterThan", "LessThan"],
                        month: ["Is", "IsNot"],
                        week: ["Is", "IsNot"],
                        hidden: ["Is", "IsNot"],
                        password: ["Is", "IsNot", "Empty", "NotEmpty"],
                        color: ["Is", "IsNot"],
                        range: ["Is", "IsNot", "GreaterThan", "LessThan"],
                        file: ["Is", "IsNot"],
                        url: ["Is", "IsNot", "Contains"],
                        image: ["Is", "IsNot"],
                        reset: ["Is", "IsNot"],
                        search: ["Is", "IsNot", "Contains"],
                        dropdown: ["Is", "IsNot", "Empty", "NotEmpty", "Contains", "DoesNotContain", "StartsWith", "EndsWith"],
                        textarea: ["Is", "IsNot", "Empty", "NotEmpty", "Contains", "DoesNotContain", "StartsWith", "EndsWith"],
                        submit: []
                    };
                },

                toggleTabEvent() {
                    document.addEventListener('shown.bs.tab', (event) => {
                        if (!event.target.matches('[data-bs-toggle="tab"]')) {
                            return;
                        }

                        var container = event.target.closest('.content-part-wrapper-form-part');

                        var inputs = this.getInputs(container || document);

                        this.fieldOptions = inputs.map(input => ({
                            value: input.htmlName,
                            text: input.htmlName,
                            type: input.htmlInputType
                        }));
                    });
                },

                shouldShowValue(operator) {
                    if (!operator) {
                        return false;
                    }

                    const check = operator;

                    if (check === 'Empty' || check === 'NotEmpty') {
                        return false;
                    }

                    return true;
                }
            },

            mounted() {
                if (config.prefix) {
                    this.prefix = config.prefix + '.';
                }
                this.toggleTabEvent();
                this.groups = config.groupOptions || [];
                this.operatorOptions = config.operatorOptions || [];
                this.allOperatorOptions = config.operatorOptions || [];
                this.populateFields();
                const observer = new MutationObserver(mutations => {
                    mutations.forEach(mutation => {
                        if (mutation.type === 'childList' && mutation.addedNodes.length) {
                            this.preloadedOptions = this.filteredFieldOptions();
                        }
                    });
                });
                observer.observe(this.$el, { childList: true, subtree: true });
            }, template: config.template
        });
        return app;
    };

    return {
        initialize: initialize
    };
}();
