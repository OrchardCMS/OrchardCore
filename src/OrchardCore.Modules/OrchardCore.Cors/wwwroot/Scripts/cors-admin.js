var optionsList = Vue.component('options-list',
    {
        props: ['options', 'optionType', 'title', 'subTitle'],
        template: '#options-list',
        data: function () {
            return {
                'newOption': ''
            };
        },
        methods: {
            addOption: function (value) {
                if (value !== null && value !== '') {
                    var noDuplicates = (this.options.map(o => o.toLowerCase()).indexOf(value.toLowerCase()) < 0);
                    if (noDuplicates) {
                        this.options.push(value);
                    }
                }
            },
            deleteOption: function (value) {
                this.options.splice(this.options.indexOf(value), 1);
            }
        }
    });

var policyDetails = Vue.component('policy-details',
    {
        components: { optionsList: optionsList },
        props: ['policy'],
        template: '#policy-details'
    });

var corsApp = new Vue({
    el: '#corsAdmin',
    components: { policyDetails: policyDetails, optionsList: optionsList },
    data: {
        selectedPolicy: null,
        policies: null,
        defaultPolicyName: null
    },
    updated: function () {
        this.searchBox();
    },
    methods: {
        newPolicy: function () {
            this.selectedPolicy = {
                name: 'New policy',
                allowedOrigins: [],
                allowAnyOrigin: true,
                allowedMethods: [],
                allowAnyMethod: true,
                allowedHeaders: [],
                allowAnyHeader: true,
                allowCredentials: true,
                isDefaultPolicy: false,
                exposedHeaders: []
            };
        },
        editPolicy: function (policy) {
            this.selectedPolicy = Object.assign({}, policy);
            this.selectedPolicy.originalName = this.selectedPolicy.name;
        },
        deletePolicy: function (policy, event) {
            this.selectedPolicy = null;
            const policyToRemove = this.policies.find(item => item.name === policy.name);
            if (policyToRemove) {
                this.policies.splice(this.policies.indexOf(policyToRemove), 1);
            }
            event.stopPropagation();
            this.save();
        },
        updatePolicy: function (policy, event) {
            if (policy.isDefaultPolicy) {
                this.policies.forEach(p => p.isDefaultPolicy = false);
            }

            if (policy.originalName) {
                const policyIndex = this.policies.findIndex((oldPolicy) => oldPolicy.name === policy.originalName);
                this.policies[policyIndex] = policy;
            }
            else {
                this.policies.push(policy);
            }

            this.save();
            this.back();
        },
        save: function () {
            document.getElementById('corsSettings').value = JSON.stringify(this.policies);
            document.getElementById('corsForm').submit();
        },
        back: function () {
            this.selectedPolicy = null;
        },
        searchBox: function () {
            const searchBox = document.getElementById('search-box');

            // On Enter, edit the item if there is a single one
            searchBox.addEventListener('keydown', (e) => {
                if (e.key == 'Enter') {

                    // Edit the item if there is a single filtered element
                    const visible = document.querySelectorAll('#corsAdmin > ul > li:visible');
                    if (visible.length == 1) {
                        window.location = visible[0].querySelector('.edit').href;
                    }
                    return false;
                }
            });

            // On each keypress filter the list
            searchBox.addEventListener('keyup', (e) => {
                const search = searchBox.value.toLowerCase();
                const elementsToFilter = document.querySelectorAll("[data-filter-value]");

                // On ESC, clear the search box and display all
                if (e.keyCode == 27 || search == '') {
                    searchBox.value = '';
                    elementsToFilter.forEach(el => el.style.display = '');
                    document.getElementById('list-alert').classList.add("d-none");
                }
                else {
                    let intVisible = 0;
                    elementsToFilter.forEach(el => {
                        const text = el.dataset.filterValue.toLowerCase();
                        const found = text.indexOf(search) > -1;
                        el.style.display = found ? '' : 'none';

                        if (found) {
                            intVisible++;
                        }
                    });

                    // We display an alert if a search is not found
                    if (intVisible == 0) {
                        document.getElementById('list-alert').classList.remove("d-none");
                    }
                    else {
                        document.getElementById('list-alert').classList.add("d-none");
                    }
                }
            });
        }
    }
});

