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
        policies:null
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
                AllowCredentials: true
            };
        },
        editPolicy: function (policy) {
            this.selectedPolicy = Object.assign({}, policy);
        },
        deletePolicy: function (policy, event) {
            this.selectedPolicy = null;
            var policyToRemove = this.policies.filter(function (item) { return item.Name === policy.Name; });
            if (policyToRemove.length > 0)
                this.policies.splice($.inArray(policyToRemove[0], this.policies), 1);
            document.getElementById('ISite_Policies').value = JSON.stringify(this.policies);
            event.stopPropagation();
        },
        updatePolicy: function (policy, event) {
            this.deletePolicy(policy, event);
            this.policies.push(policy);
            document.getElementById('ISite_Policies').value = JSON.stringify(this.policies);
        },
        cancelUpdate: function () {
            this.selectedPolicy = null;
        }
    }
});