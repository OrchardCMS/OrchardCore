window.formVisibilityGroups = function () {

    const defaultConfig = {
        template:
            `
           <div>
        <ul class="list-group">
            <!-- Loop through each group -->
            <li class="list-group-item" v-for="(group, groupIndex) in groups" :key="groupIndex">
                <div class="d-flex justify-content-between mb-2">
                    <span>Group {{ groupIndex + 1 }}</span>
                    <input type="hidden" :name="prefix + 'Groups[' + groupIndex + '].IsRemoved'" value="false" />
                    <button type="button" class="btn btn-sm btn-danger" @click="removeGroup(groupIndex)">
                        <i class="fa-solid fa-trash"></i> Remove Group
                    </button>
                </div>

                <!-- Loop through each rule -->
                <ul class="list-group mb-3">
                    <!-- Loop through each rule in the group -->
                    <li class="list-group-item" v-for="(rule, ruleIndex) in group.rules" :key="ruleIndex">
                        <div class="row">
                            <div class="col">
                                <select class="form-select" v-model="rule.field" :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].Field'">
                                    <option value="">Select Field</option>
                                    <option v-for="option in fieldOptions" :value="option.value">
                                        {{ option.text }}
                                    </option>
                                </select>
                            </div>

                            <div class="col">
                                <select class="form-select" v-model="rule.operator" :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].Operator'">
                                    <option value="">Select Operator</option>
                                    <option v-for="option in operatorsList(rule.field)" :value="option.value">
                                        {{ option.text }}
                                    </option>
                                </select>
                            </div>

                            <div class="col">
                                <input type="text" class="form-control" v-model="rule.value" placeholder="Value" :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].Value'" />
                            </div>

                            <div class="col-auto">
                                <input type="hidden" :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].IsRemoved'" value="false" />
                                <button type="button" class="btn btn-sm btn-danger" @click="removeRule(groupIndex, ruleIndex)">
                                    <i class="fa-solid fa-trash"></i> Remove Rule
                                </button>
                            </div>
                        </div>
                    </li>
                    <li class="list-group-item">
                        <div class="d-flex justify-content-end mb-2">
                            <button type="button" class="btn btn-sm btn-secondary" @click="addRule(groupIndex)">
                                <i class="fa-solid fa-circle-plus"></i> New Rule
                            </button>
                        </div>
                    </li>
                </ul>
            </li>
            <li class="list-group-item">
                <div class="d-flex justify-content-end">
                    <button type="button" class="btn btn-sm btn-primary" @click="addGroup()">
                        <i class="fa-solid fa-circle-plus"></i> New Group
                    </button>
                </div>
            </li>
        </ul>
    </div>
        `
    };

    const initialize = (instanceConfig) => {
        const config = Object.assign({}, defaultConfig, instanceConfig);
        if (!config.appElementSelector) {
            console.error('appElementSelector is required');
            return;
        }

        let groupCounter = 0;

        const app = new Vue({
            el: config.appElementSelector,
            data() {
                return {
                    groups: config.groupOptions || [],
                    fieldOptions: config.fieldOptions || [],
                    operatorOptions: config.operatorOptions || [],
                    allOperatorOptions: config.operatorOptions || [],
                    prefix: '',
                    widgetId: config.widgetId,
                };
            },

            computed: {
                groupsJson() {
                    return JSON.stringify(this.groups);
                }
            },

            methods: {
                addGroup() {
                    var newGroup = {
                        id: 'group-' + (groupCounter++),
                        rules: [{
                            id: 'rule-' + Date.now() + '-' + Math.floor(Math.random() * 1000),
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
                        id: 'rule-' + Date.now() + '-' + Math.floor(Math.random() * 1000),
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

                        const inputTypeSelect = widget.querySelector('select[name$="InputPart.Type"]');

                        if (formElementNameInput && inputTypeSelect) {

                            const htmlName = formElementNameInput.value;

                            const selectedOption = inputTypeSelect.options[inputTypeSelect.selectedIndex].value;

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

                operatorsList(fieldId) {
                    const field = this.fieldOptions.find(f => f.value === fieldId);

                    if (!field) return [];

                    const mapping = this.operatorMapping();

                    if (!mapping[field.type]) return [];

                    return this.allOperatorOptions.filter(x =>
                        mapping[field.type].includes(x.value.toLowerCase())
                    );
                },

                operatorMapping() {
                    return {
                        checkbox: ["is", "isnot"],
                        text: ["is", "isnot", "empty", "notempty", "contains", "doesnotcontain", "startswith", "endswith"],
                        number: ["is", "isnot", "greaterthan", "lessthan"],
                        email: ["is", "isnot", "empty", "notempty"],
                        tel: ["is", "isnot"],
                        date: ["is", "isnot", "greaterthan", "lessthan"],
                        time: ["is", "isnot", "greaterthan", "lessthan"],
                        "datetime": ["is", "isnot", "greaterthan", "lessthan"],
                        "datetime-local": ["is", "isnot", "greaterthan", "lessthan"],
                        month: ["is", "isnot"],
                        week: ["is", "isnot"],
                        hidden: ["is", "isnot"],
                        password: ["is", "isnot", "empty", "notempty"],
                        color: ["is", "isnot"],
                        range: ["is", "isnot", "greaterthan", "lessthan"],
                        file: ["is", "isnot"],
                        url: ["is", "isnot", "contains"],
                        image: ["is", "isnot"],
                        reset: ["is", "isnot"],
                        search: ["is", "isnot", "contains"],
                        submit: [],
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

                console.log(this.groups);

            }, template: config.template
        });
        return app;
    };

    return {
        initialize: initialize
    };
}();
