selectOptionsEditor = function () {

    const initilize = (elemId, optionsData, defaultValue) => {
        var keyId = 1;
        //Add Key ID
        optionsData = optionsData.map((x) => {
            x.key = keyId;
            keyId++;
            return x;
        });

        var selectOptionsRow = {
            name: "select-options-row",
            template: "#select-options-row",
            props: ["option", "defaultValue"],
            data: function () {
                return {
                    partId: elemId,
                };
            },
            methods: {
                remove: function () {
                    this.$emit("remove-option", this.option);
                },
            },
            computed: {
                IsSelected: {
                    get: function () {
                        if (!IsNullOrWhiteSpace(this.option.value)) {
                            return this.option.value == this.defaultValue;
                        } else {
                            return this.option.text == this.defaultValue;
                        }
                    },
                    set: function (val) {
                        if (val) {
                            this.$emit("set-default", this.option);
                        } else {
                            this.$emit("set-default", null);
                        }
                    },
                },
                optionValue: {
                    get: function () {
                        return this.option.value;
                    },
                    set: function (val) {
                        var isSelected = this.IsSelected;
                        this.option.value = val;
                        if (isSelected) {
                            this.$emit("set-default", this.option);
                        } else {
                            this.$emit("reorder-option");
                        }
                    },
                },

                optionText: {
                    get: function () {
                        return this.option.text;
                    },
                    set: function (val) {
                        var isSelected = this.IsSelected;
                        this.option.text = val;
                        if (isSelected) {
                            this.$emit("set-default", this.option);
                        } else {
                            this.$emit("reorder-option");
                        }
                    },
                },

                optionCheck: {
                    get: function () {
                        if (IsNullOrWhiteSpace(this.option.value)) {
                            return this.option.text;
                        } else {
                            return this.option.value;
                        }
                    },
                },
            },
        };

        var selectOptionsTable = {
            name: "select-options-table",
            components: {
                selectOptionsRow: selectOptionsRow,
            },
            template: "#select-options-table",
            props: ["data"],
            data: function () {
                return {
                    partId: elemId,
                };
            },
            methods: {
                add: function () {
                    this.$emit("add-option");
                },
                onDragEnd: function () {
                    this.$emit("reorder-option");
                },
            },
        };

        var selectOptionsModal = {
            name: "select-options-modal",
            template: "#select-options-modal",
            props: ["data", "showModal", "validOptions"],
            data: function () {
                return {
                    optionsFormattedList: "[]",
                    partId: elemId,
                    defaultValue: "",
                    isValid: false,
                    jsonOptions: [],
                };
            },
            methods: {
                closeModal: function (save) {
                    if (save) {
                        this.$emit("modal-save", {
                            options: this.jsonOptions,
                            defaultValue: this.defaultValue,
                        });
                    } else {
                        this.$emit("modal-cancel");
                    }
                },
                showStart: function (params) {
                    this.$refs.modal.classList.toggle('d-block');
                    this.$refs.backdrop.classList.toggle('d-block');
                },
                showEnd: function (params) {
                    this.$refs.modal.classList.toggle('show');
                    this.$refs.backdrop.classList.toggle('show');
                }
            },
            watch: {
                showModal: function (newval) {
                    if (newval) {

                        this.optionsFormattedList = JSON.stringify(
                            this.validOptions,
                            null,
                            2
                        );
                        this.defaultValue = this.data.defaultValue;
                    } else {
                        this.optionsFormattedList = "[]";
                        this.defaultValue = "";
                    }
                },
                optionsFormattedList: function (newval) {
                    try {
                        var parsed = JSON.parse(newval);
                        if (!!newval && parsed instanceof Array) {
                            this.jsonOptions = parsed;
                            this.isValid = true;
                        } else {
                            this.isValid = false;
                        }
                    } catch (e) {
                        this.isValid = false;
                    }
                },
            },
        };

        new Vue({
            components: {
                selectOptionsTable: selectOptionsTable,
                selectOptionsModal: selectOptionsModal,
            },
            data: function () {
                return {
                    state: {
                        options: optionsData,
                        defaultValue: defaultValue,
                        partId: elemId,
                    },
                    debounceTimeout: null,
                    showModal: false,
                };
            },
            el: "#" + elemId,
            methods: {
                cancelChanges: function () {
                    this.showModal = false;
                },
                updateChanges: function (changes) {
                    this.state.options = changes.options
                        .filter(function (y) {
                            return !IsNullOrWhiteSpace(y.text);
                        })
                        .map(function (x) {
                            x.key = keyId++;
                            return x;
                        });
                    this.state.defaultValue = changes.defaultValue;
                    this.showModal = false;
                    this.debouncePreview();
                },
                setDefaultValue: function (opt) {
                    if (opt == null) {
                        this.state.defaultValue = "";
                    } else {
                        if (!IsNullOrWhiteSpace(opt.value)) {
                            this.state.defaultValue = opt.value;
                        } else {
                            this.state.defaultValue = opt.text;
                        }
                    }
                    this.debouncePreview();
                },
                addOption: function () {
                    this.state.options.push({
                        text: "",
                        value: "",
                        key: keyId++,
                    });
                    this.debouncePreview();
                },
                removeOption: function (opt) {
                    var index = this.state.options.findIndex(function (c) {
                        return c.key == opt.key;
                    });
                    if (index > -1) {
                        this.state.options.splice(index, 1);
                        this.debouncePreview();
                    }
                },
                reorderOption: function (evt) {
                    this.debouncePreview();
                },
                debouncePreview: function (params) {
                    if (this.debounceTimeout) clearTimeout(this.debounceTimeout);
                    this.debounceTimeout = setTimeout(() => {
                        const previewEvent = new Event("contentpreview:render");
                        document.dispatchEvent(previewEvent);
                    }, 500);
                },
            },
            computed: {
                stringify: function () {
                    return JSON.stringify(this.validOptions);
                },
                validOptions: function () {
                    return this.state.options
                        .map(function (x) {
                            return {
                                text: x.text,
                                value: x.value,
                            };
                        })
                        .filter(function (x) {
                            return !IsNullOrWhiteSpace(x.text);
                        });
                },
            },
        });

        function IsNullOrWhiteSpace(str) {
            return str == null || str.match(/^ *$/) !== null;
        }
    };

    const initilizeElement = (id, options, value) => {
        var wrapper = document.getElementById(id);

        if (wrapper != null) {
            initilize(id, options, value);
        }
    }

    const initilizeFieldType = (wrapper) => {
        var selectMenus = wrapper.getElementsByClassName('field-type-select-menu');
        for (let i = 0; i < selectMenus.length; i++) {
            var selectMenu = selectMenus[i];
            selectMenu.addEventListener('change', function (e) {
                var visibleForInputContainers = wrapper.getElementsByClassName('show-for-input');

                for (let i = 0; i < visibleForInputContainers.length; i++) {
                    var container = visibleForInputContainers[i];
                    if (e.target.value == 'reset' || e.target.value == 'submit' || e.target.value == 'hidden') {
                        container.classList.add('d-none');
                    } else {
                        container.classList.remove('d-none');
                    }
                }
            });
            selectMenu.dispatchEvent(new Event('change'));
        }
    };

    return {
        initilizeElement: initilizeElement,
        initilizeFieldType: initilizeFieldType
    };
}();

document.addEventListener('DOMContentLoaded', function () {
    var wrappers = document.getElementsByName('select-part-properties-wrapper');

    for (let i = 0; i < wrappers.length; i++) {
        var wrapper = wrappers[i];

        var fieldWrapper = wrapper.querySelector('.field-options-wrapper');

        if (fieldWrapper != null) {
            var initialDefaultValue = fieldWrapper.querySelector('.field-options-wrapper-initial-default-value');
            var initialOptions = fieldWrapper.querySelector('.field-options-wrapper-initial-options');

            selectOptionsEditor.initilizeElement(fieldWrapper.Id, initialOptions.innerHTML, initialDefaultValue.value)
        }
    }

    selectOptionsEditor.initilizeFieldType(document);
});
