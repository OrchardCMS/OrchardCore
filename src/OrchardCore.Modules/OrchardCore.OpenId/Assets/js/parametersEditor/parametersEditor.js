function initializeParametersEditor(elem, data, modalBodyElement) {

    const store = {
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
            return JSON.stringify(this.state.parameters.filter(parameter => !IsNullOrWhiteSpace(parameter.name)));
        }
    };

    const ParametersTable = {
        template: '#parameters-table',
        props: ['data'],
        name: 'parameters-table',
        methods: {
            add: store.addParameter,
            remove: store.removeParameter,
            getParametersFormattedList: store.getParametersFormattedList
        }
    };

    const ParametersModal = {
        template: '#parameters-modal',
        props: ['data'],
        name: 'parameters-modal',
        methods: {
            getParametersFormattedList: store.getParametersFormattedList,
            showModal: function () {
                this.data.modal.show();
            },
            closeModal: function () {
                this.data.modal.hide();
            }
        }
    };

    new Vue({
        components: {
            ParametersTable,
            ParametersModal
        },
        data() {
            return {
                sharedState: store.state,
                modal: new bootstrap.Modal(modalBodyElement[0])
            };
        },
        el,
        methods: {
            showModal: ParametersModal.methods.showModal
        }
    });

}

function IsNullOrWhiteSpace(str) {
    return str === null || str.match(/^ *$/) !== null;
}
