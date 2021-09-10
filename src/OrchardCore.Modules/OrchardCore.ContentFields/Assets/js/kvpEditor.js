function initializeKvpEditor(elem, keys, values) {

    var data = [];

    for (i = 0; i < keys.length; i++) {
        data.push({ key: keys[i], value: values[i] });
    }

    var store = {
        debug: false,
        state: {
            options: data,
        },
        addOption: function () {
            if (this.debug) { console.log('add option triggered') };
            this.state.options.push({ key: '', value: '' });
        },
        removeOption: function (index) {
            if (this.debug) { console.log('remove option triggered with', index) };
            this.state.options.splice(index, 1);
        },
        getKeys: function () {
            if (this.debug) { console.log('getKeys triggered') };
            return JSON.stringify(this.state.options.map(x => x.key));
        },
        getValues: function () {
            if (this.debug) { console.log('getValues triggered') };
            return JSON.stringify(this.state.options.map(x => x.value));
        }
    }

    var kvpTable = {
        template: '#kvp-table',
        props: ['data'],
        name: 'kvp-table',
        methods: {
            add: function () {
                store.addOption();
            },
            remove: function (index) {
                store.removeOption(index);
            },
            getKeys: function () {
                return store.getKeys();
            },
            getValues: function () {
                return store.getValues();
            }
        }
    };

    new Vue({
        components: {
            kvpTable: kvpTable
        },
        data: {
            sharedState: store.state
        },
        el: elem,
        methods: {
        }
    });

}
