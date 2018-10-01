function initializeOptionsEditor(el, data) {

    var optionsEditor = $(el);
    var previouslyChecked;

    var optionsTable = {
        template: '#options-table',
        props: ['value'],
        name: 'options-table',
        computed: {
            list: {
                get() {
                    return this.value;
                },
                set(value) {
                    this.$emit('input', value);
                }
            }
        },
        methods: {
            add: function () {
                this.list.push({ name: '', value: ''});
            },
            remove: function (index) {
                this.list.splice(index, 1);
            },
            uncheck: function (index) {
                if (index == previouslyChecked) {
                    $('#customRadio_' + index)[0].checked = false;
                    previouslyChecked = null;
                }
                else {
                    previouslyChecked = index;
                }

            },
            onChange: function () {
                checked = false;
            }
        }
    };

    new Vue({
        components: {
            optionsTable: optionsTable
        },
        el: optionsEditor.get(0),
        data: {
            option: data,
            dragging: false
        },
        methods: {
            getData: function () {
                return JSON.stringify(data);
            }
        }

    });

}