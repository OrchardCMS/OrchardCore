function initializeOptionsEditor(elem, data, defaultValue, selectedValue, list) {

    var optionsTable = {
        name: 'options-table',
        template: '#options-table',
        props: ['data', 'defaultValue', 'selectedValue', 'list'],
        data: function () {
            return {
                defaultCulture: this.defaultValue,
                selectedCulture: typeof this.selectedValue != 'undefined' ? this.selectedValue : selectedValue,
                allCultures: this.list
            }
        },
        methods: {
            add: function () {
                if (!this.data.includes(this.selectedCulture)) {
                    this.data.push(this.selectedCulture);
                }
            },
            remove: function (index) {
                this.data.splice(index, 1);
            },
            getSupportedCultures: function () {
                return JSON.stringify(this.data);
            },
            getDefaultCulture: function () {
                var result = defaultValue;
                if (this.defaultCulture != null) {
                    result = this.defaultCulture;
                }
                return result;
            },
            getAllCultures: function () {
                return this.AllCultures;
            }
        }
    };

    new Vue({
        components: {
            optionsTable: optionsTable
        },
        data: {
            allCultures: list,
            supportedCultures: data,
            defaultCulture: defaultValue,
            selectedCulture: selectedValue
        },
        el: elem
    });

}
