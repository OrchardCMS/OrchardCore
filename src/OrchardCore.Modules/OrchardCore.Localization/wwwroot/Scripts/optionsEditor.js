function initializeOptionsEditor(elem, data, defaultValue, selectedValue, list) {

    var optionsTable = {
        name: 'options-table',
        template: '#options-table',
        props: {
            data: {
                type: Array,
                required: true
            },
            defaultValue: {
                type: String,
                required: true
            },
            selectedValue: {
                type: String,
                required: true
            },
            list: {
                type: Array,
                required: true
            }
        },
        data() {
            return {
                defaultCulture: this.defaultValue,
                selectedCulture: typeof this.selectedValue != 'undefined' ? this.selectedValue : selectedValue,
                allCultures: this.list
            }
        },
        methods: {
            add() {
                if (!this.data.includes(this.selectedCulture)) {
                    this.data.push(this.selectedCulture);

                    const culture = this.selectedCulture;
                    const selectedCulture = this.allCultures.find(element => element.Name === culture);

                    if (selectedCulture) {
                        selectedCulture.Supported = true;
                    }

                    const allUnsupportedCultures = this.allCultures.filter(element => !element.Supported);

                    if (allUnsupportedCultures.length > 0) {
                        this.selectedCulture = allUnsupportedCultures[0].Name;
                    }
                }
            },
            remove(index) {
                const selectedCulture = this.data[index];
                const cultureRemoved = this.list.filter(element => element.Name === selectedCulture);

                if (cultureRemoved.length > 0) {
                    cultureRemoved[0].Supported = false;
                }

                this.data.splice(index, 1);

                const allUnsupportedCultures = this.allCultures.filter(element => !element.Supported);

                if (allUnsupportedCultures.length > 0) {
                    this.selectedCulture = allUnsupportedCultures[0].Name;
                }
            },
            getSupportedCultures() {
                return JSON.stringify(this.data);
            },
            getDefaultCulture() {
                let result = defaultValue;
                if (this.defaultCulture !== null) {
                    result = this.defaultCulture;
                }
                return result;
            },
            getAllCultures() {
                return this.AllCultures;
            }
        }
    };

    new Vue({
        components: {
            optionsTable
        },
        data() {
            return {
                allCultures: list,
                supportedCultures: data,
                defaultCulture: defaultValue,
                selectedCulture: selectedValue
            }
        },
        el
    });
}

