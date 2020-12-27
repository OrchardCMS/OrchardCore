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
                this.options.push(value);
            },
            deleteOption: function (value) {
                this.options.splice($.inArray(value[0], this.options), 1);
            }
        }
    });

var policyDetails = Vue.component('policy-details',
    {
        components: { optionsList : optionsList },
        props: ['policy'],
        template: '#policy-details'
    });

var corsApp = new Vue({
    el: '#corsAdmin',
    components: { policyDetails : policyDetails, optionsList : optionsList },
    data: {
        selectedPolicy: null,
        policies: null,
        defaultPolicyName: null
    },
    methods: {
        newPolicy: function () {
            this.selectedPolicy = {
                Name: 'New policy',
                AllowedOrigins: [],
                AllowAnyOrigin: true,
                AllowedMethods: [],
                AllowAnyMethod: true,
                AllowedHeaders: [],
                AllowAnyHeader: true,
                AllowCredentials: true,
                IsDefaultPolicy: false
            };
        },
        editPolicy: function (policy) {
            this.selectedPolicy = Object.assign({}, policy);
            this.selectedPolicy.OriginalName = this.selectedPolicy.Name;
        },
        deletePolicy: function (policy, event) {
            this.selectedPolicy = null;
            var policyToRemove = this.policies.filter(function (item) { return item.Name === policy.Name; });
            if (policyToRemove.length > 0)
                this.policies.splice($.inArray(policyToRemove[0], this.policies), 1);
            event.stopPropagation();
            this.save();
        },
        updatePolicy: function (policy, event) {
            if (policy.IsDefaultPolicy) {
                this.policies.forEach(p => p.IsDefaultPolicy = false);
            }
            if (policy.OriginalName) {
                var policyIndex = this.policies.findIndex((oldPolicy) => oldPolicy.Name === policy.OriginalName);
                this.policies[policyIndex] = policy;
            }
            else {
                this.policies.push(policy);
            }
            this.save();
            this.back();
        },
        save: function () {
            document.getElementById('CorsSettings').value = JSON.stringify(this.policies);
            document.getElementById('corsForm').submit();
        },
        back: function () {
            this.selectedPolicy = null;
        }
    }
});
