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
                    // Intentionally mutates the shared array reference passed down from
                    // corsApp.selectedPolicy; the parent never re-passes a new array, so this
                    // is how the edit is reflected back up.
                    if (noDuplicates) {
                        // eslint-disable-next-line vue/no-mutating-props
                        this.options.push(value);
                    }
                }
            },
            deleteOption: function (value) {
                // eslint-disable-next-line vue/no-mutating-props -- see addOption above.
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

window.corsApp = new Vue({
    el: '#corsAdmin',
    components: { policyDetails: policyDetails, optionsList: optionsList },
    data: function () {
        return {
            selectedPolicy: null,
            policies: [],
            defaultPolicyName: null
        };
    },
    updated: function () {
        this.searchBox();
    },
    methods: {
        newPolicy: function () {
            this.selectedPolicy = {
                name: 'New policy',
                allowedOrigins: [],
                allowAnyOrigin: false,
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
            var policyToRemove = this.policies.filter(function (item) { return item.name === policy.name; });
            if (policyToRemove.length > 0)
                this.policies.splice(this.policies.indexOf(policyToRemove[0]), 1);
            event.stopPropagation();
            this.save();
        },
        updatePolicy: function (policy) {
            if (policy.isDefaultPolicy) {
                this.policies.forEach(p => p.isDefaultPolicy = false);
            }

            if (policy.originalName) {
                var policyIndex = this.policies.findIndex((oldPolicy) => oldPolicy.name === policy.originalName);
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
            var searchBox = document.getElementById('search-box');

            if (!searchBox) {
                return;
            }

            // On Enter, edit the item if there is a single one
            searchBox.addEventListener('keydown', function (e) {
                if (e.key == 'Enter') {

                    // Edit the item if there is a single filtered element
                    var visible = Array.from(document.querySelectorAll('#corsAdmin > ul > li')).filter(function (li) {
                        return li.style.display !== 'none';
                    });
                    if (visible.length == 1) {
                        window.location = visible[0].querySelector('.edit').getAttribute('href');
                    }
                    return false;
                }
            });

            // On each keypress filter the list
            searchBox.addEventListener('keyup', function (e) {
                var search = searchBox.value.toLowerCase();
                var elementsToFilter = document.querySelectorAll("[data-filter-value]");

                // On ESC, clear the search box and display all
                if (e.key === 'Escape' || search == '') {
                    searchBox.value = '';
                    elementsToFilter.forEach(function (el) { el.style.display = ''; });
                    document.getElementById('list-alert').classList.add("d-none");
                }
                else {
                    var intVisible = 0;
                    elementsToFilter.forEach(function (el) {
                        var text = el.dataset.filterValue.toLowerCase();
                        var found = text.indexOf(search) > -1;
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
