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
            parametersTable: parametersTable,
            parametersModal: parametersModal
        },
        data: {
            sharedState: store.state
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
