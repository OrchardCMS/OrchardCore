function initializeCustomMetaTagsEditor(elem, data, modalBodyElement) {

    var store = {
        debug: false,
        state: {
            metaTags: data
        },
        addMetaTag: function () {
            this.state.metaTags.push({ content: '', name: '', property: '', charset: '', httpEquiv: '' });
        },
        removeMetaTag: function (index) {
            this.state.metaTags.splice(index, 1);
        },
        getMetaTagsFormattedList: function () {
            return JSON.stringify(this.state.metaTags);
        }
    }

    var metaTagsTable = {
        template: '#meta-tags-table',
        props: ['data'],
        name: 'meta-tags-table',
        methods: {
            add: function () {
                store.addMetaTag();
            },
            remove: function (index) {
                store.removeMetaTag(index);
            },
            getMetaTagsFormattedList: function () {
                return store.getMetaTagsFormattedList();
            }
        }
    };

    var metaTagsModal = {
        template: '#meta-tags-modal',
        props: ['data'],
        name: 'meta-tags-modal',
        methods: {
            getMetaTagsFormattedList: function () {
                return store.getMetaTagsFormattedList();
            },
            showModal: function () {
                metaTagsModal.props.data.modal = new bootstrap.Modal(modalBodyElement[0]);
                metaTagsModal.props.data.modal.show();
            },
            closeModal: function () {
                metaTagsModal.props.data.modal.hide();
            }
        }
    };

    new Vue({
        components: {
            metaTagsTable: metaTagsTable,
            metaTagsModal: metaTagsModal
        },
        data: {
            sharedState: store.state,
            modal: null
        },
        el: elem,
        methods: {
            showModal: function () {
                metaTagsModal.methods.showModal();
            }
        }
    });
}
