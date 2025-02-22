function initializeOptionsEditor(elem, data, defaultValue, modalBodyElement) {

    var previouslyChecked;

    var store = {
        debug: false,
        state: {
            options: data,
            selected: defaultValue
        },
        addOption: function () {
            if (this.debug) { console.log('add option triggered') };
            this.state.options.push({ name: '', value: '' });
        },
        removeOption: function (index) {
            if (this.debug) { console.log('remove option triggered with', index) };
            this.state.options.splice(index, 1);
        },
        getOptionsFormattedList: function () {
            if (this.debug) { console.log('getOptionsFormattedList triggered') };
            return JSON.stringify(this.state.options.filter(function (x) { return !IsNullOrWhiteSpace(x.name) }));
        }
    }

    var optionsTable = {
        template: '#options-table',
        props: ['data'],
        name: 'options-table',
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
                    store.state.selected = null;
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

    var optionsModal = {
        template: '#options-modal',
        props: ['data'],
        name: 'options-modal',
        methods: {
            getOptionsFormattedList: function () {
                return store.getOptionsFormattedList();
            },
            showModal: function () {
                optionsModal.props.data.modal = new bootstrap.Modal(modalBodyElement[0]);
                optionsModal.props.data.modal.show();
            },
            closeModal: function () {
                optionsModal.props.data.modal.hide();
            }
        }
    };

    new Vue({
        components: {
            optionsTable: optionsTable,
            optionsModal: optionsModal
        },
        data: {
            sharedState: store.state,
            modal: null
        },
        el: elem,
        methods: {
            showModal: function () {
                optionsModal.methods.showModal();
            }
        }
    });

}

function IsNullOrWhiteSpace(str) {
    return str === null || str.match(/^ *$/) !== null;
}