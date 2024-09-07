function initializeParametersEditor(elem, data, modalBodyElement) {

    var store = {
        state: {
            parameters: data
        },
        addParameter: function () {
            this.state.parameters.push({ name: '', value: '' });
        },
        removeParameter: function (index) {
            this.state.parameters.splice(index, 1);
        },
        getParametersFormattedList: function () {
            return JSON.stringify(this.state.parameters.filter(function (x) { return !IsNullOrWhiteSpace(x.name) }));
        }
    }

    var parametersTable = {
        template: '#parameters-table',
        props: ['data'],
        name: 'parameters-table',
        methods: {
            add: function () {
                store.addParameter();
            },
            remove: function (index) {
                store.removeParameter(index);
            },
            getParametersFormattedList: function () {
                return store.getParametersFormattedList();
            }
        }
    };

    var parametersModal = {
        template: '#parameters-modal',
        props: ['data'],
        name: 'parameters-modal',
        methods: {
            getParametersFormattedList: function () {
                return store.getParametersFormattedList();
            },
            showModal: function () {
                parametersModal.props.data.modal = new bootstrap.Modal(modalBodyElement[0]);
                parametersModal.props.data.modal.show();
            },
            closeModal: function () {
                parametersModal.props.data.modal.hide();
            }
        }
    };

    new Vue({
        components: {
            parametersTable: parametersTable,
            parametersModal: parametersModal
        },
        data: {
            sharedState: store.state,
            modal: null
        },
        el: elem,
        methods: {
            showModal: function () {
                parametersModal.methods.showModal();
            }
        }
    });

}

function IsNullOrWhiteSpace(str) {
    return str === null || str.match(/^ *$/) !== null;
}
