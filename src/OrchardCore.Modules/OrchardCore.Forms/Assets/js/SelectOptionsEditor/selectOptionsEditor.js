function initializeSelectOptionsEditor(elem, data, defaultValue, modalBodyElement) {

    var previouslyChecked;

    var store = {
        debug: false,
        state: {
            options: data,
            default: defaultValue
        },
        addOption: function () {
            if (this.debug) { console.log('add option triggered') };
            this.state.options.push({ text: '', value: '' });
        },
        removeOption: function (index) {
            if (this.debug) { console.log('remove option triggered with', index) };
            this.state.options.splice(index, 1);
        },
        getOptionsFormattedList: function () {
            if (this.debug) { console.log('getOptionsFormattedList triggered') };
            return JSON.stringify(this.state.options.filter(function (x) { return !IsNullOrWhiteSpace(x.text) }));
        }
    }

    var selectOptionsTable = {
        template: '#select-options-table',
        props: ['data'],
        name: 'select-options-table',
        methods: {
            add: function () {
                store.addOption();
            },
            remove: function (index) {
                store.removeOption(index);
            },
            uncheck: function (index) {
                if (index == previouslyChecked) {
                    $('#customRadio_' + index)[0].checked = false;
                    store.state.default = null;
                    previouslyChecked = null;
                }
                else {
                    previouslyChecked = index;
                }
            },
            getOptionsFormattedList: function () {
                return store.getOptionsFormattedList();
            }
        }
    };

    var selectOptionsModal = {
        template: '#select-options-modal',
        props: ['data'],
        name: 'select-options-modal',
        methods: {
            getOptionsFormattedList: function () {
                return store.getOptionsFormattedList();
            },
            showModal: function () {
                $(modalBodyElement).modal();
            },
            closeModal: function () {
                var modal = $(modalBodyElement).modal();
                modal.modal('hide');
            }
        }
    };

    new Vue({
        components: {
            selectOptionsTable: selectOptionsTable,
            selectOptionsModal: selectOptionsModal
        },
        data: {
            sharedState: store.state
        },
        el: elem,
        methods: {
            showModal: function () {
                selectOptionsModal.methods.showModal();
            }
        }
    });

}

function IsNullOrWhiteSpace(str) {
    return str === null || str.match(/^ *$/) !== null;
}
