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
                                    <option v-for="option in operatorOptions" :value="option.value">
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
                    groups: [],
                    fieldOptions: config.fieldOptions || [],
                    operatorOptions: [],
                    prefix: '',
                };
            },

            computed: {
                groupsJson() {
                    return JSON.stringify(this.groups);
                }
            },

            methods: {
                addGroup() {
                    this.groups.push({
                        id: 'group-' + (groupCounter++),
                        rules: []
                    });
                },

                addRule(groupIndex) {
                    const newRule = {
                        id: 'rule-' + Date.now() + '-' + Math.floor(Math.random() * 1000),
                        field: '',
                        operator: '',
                        value: ''
                    };
                    this.groups[groupIndex].rules.push(newRule);
                },

                removeGroup(groupIndex) {
                    this.groups.splice(groupIndex, 1);
                },

                removeRule(groupIndex, ruleIndex) {
                    this.groups[groupIndex].rules.splice(ruleIndex, 1);
                },

                populateFields() {
                    const inputs = this.getInputs();
                    this.fieldOptions = inputs.map(input => {
                        return {
                            value: input.htmlId,
                            text: input.htmlName
                        };
                    });
                },

                getInputs() {
                    const widgetElements = document.querySelectorAll('.widget-template');

                    const results = [];

                    widgetElements.forEach(function (widget) {

                        const formElementInput = widget.querySelector('input[name$="FormElementPart.Id"]');

                        const formElementNameInput = widget.querySelector('input[name$="FormInputElementPart.Name"]');

                        const inputTypeSelect = widget.querySelector('select[name$="InputPart.Type"]');

                        if (formElementInput && inputTypeSelect) {

                            const htmlId = formElementInput.value;

                            const htmlName = formElementNameInput.value;

                            const selectedOption = inputTypeSelect.options[inputTypeSelect.selectedIndex].value;

                            if (!formElementInput.value || !selectedOption) {
                                return;
                            }
                            results.push({
                                htmlId: htmlId,
                                htmlName: htmlName,
                                htmlInputType: selectedOption
                            });
                        }
                    });
                    return results;
                },

                findOperators() {
                    const operatorData = document.getElementById('operatorData');
                    if (operatorData) {
                        try {
                            return JSON.parse(operatorData.getAttribute('data-operators'));
                        } catch (e) {
                            console.error("Error parsing operator data:", e);
                        }
                    }
                    return [];
                }
            },

            mounted() {

                if (config.prefix) {
                    this.prefix = config.prefix + '.';
                }

                console.log('config.appElementSelector:', config.appElementSelector); // Log the appElementSelector value


                //var appElement = document.querySelector(config.appElementSelector);


                //if (!appElement) {
                //    console.log('Null?:', appElement);
                //    return;
                //}

                //var data = appElement.getAttribute('data-groups') || '{}';
                //console.log("data-groups attribute value:", data); // Log the data-groups attribute value

                //var groups = JSON.parse(data);
                //console.log("Parsed groups data:", groups); // Log the parsed groups data

                //// Load groups from the data-groups attribute
                //if (groups) {
                //    this.groups = groups;
                //}

                //var savedGroups = localStorage.getItem('formVisibilityGroups');
                //if (savedGroups) {
                //    groups = JSON.parse(savedGroups);
                //}

                this.$nextTick(() => {
                    this.populateFields();
                    console.log("Vue component mounted. Prefix is:", this.prefix);
                    this.operatorOptions = this.findOperators();
                });
            },
            template: config.template
        });
        return app;
    };

    return {
        initialize: initialize
    };
}();
