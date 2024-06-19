function initializeItemsEditor(elem, data, modalBodyElement) {
    var store = {
        debug: false,
        state: {
            items: data,
        },
        addItem: function () {
            if (this.debug) {
                console.log('add item triggered');
            }
            this.state.items.push('');
        },
        removeItem: function (index) {
            if (this.debug) {
                console.log('remove item triggered with', index);
            }
            this.state.items.splice(index, 1);
        },
        getItemsFormattedList: function () {
            if (this.debug) {
                console.log('getItemsFormattedList triggered');
            }
            return JSON.stringify(
                this.state.items.filter(function (x) {
                    return !IsNullOrWhiteSpace(x);
                })
            );
        },
    };

    var itemsTable = {
        template: '#items-table',
        props: ['data'],
        name: 'items-table',
        methods: {
            add: function () {
                store.addItem();
            },
            remove: function (index) {
                store.removeItem(index);
            },
            getItemsFormattedList: function () {
                return store.getItemsFormattedList();
            },
        },
    };

    var itemsModal = {
        template: '#items-modal',
        props: ['data'],
        name: 'items-modal',
        methods: {
            getItemsFormattedList: function () {
                return store.getItemsFormattedList();
            },
            showModal: function () {
                valuesModal.props.data.modal = new bootstrap.Modal(
                    modalBodyElement[0]
                );
                valuesModal.props.data.modal.show();
            },
            closeModal: function () {
                valuesModal.props.data.modal.hide();
            },
        },
    };

    new Vue({
        components: {
            itemsTable: itemsTable,
            itemsModal: itemsModal,
        },
        data: {
            sharedState: store.state,
            modal: null,
        },
        el: elem,
        methods: {
            showModal: function () {
                valuesModal.methods.showModal();
            },
        },
    });
}

