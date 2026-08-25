import { getTranslations, setTranslations } from "@orchardcore/bloom/helpers/localizations";

// OrchardCore.Forms' SelectPart.Fields.Edit.cshtml: the Select field's static options editor - a
// draggable table (text/value/"is default" radio per row) plus a "bulk edit as JSON" modal.
// Single consumer, so this stays a module-own Assets/ts entry (matches the Cors precedent) rather
// than a shared bloom/components/* widget; it is NOT the same shape as options-table-editor.ts
// (Task 3) since the "default" selection is a single radio bound to a fallback-to-text value
// rather than a boolean-per-row flag, and it drives a live content-preview re-render on change.
declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string | Element): void };
};

// Loaded globally as the "vue-draggable" UMD resource (vuedraggable@4.1.0). The UMD wrapper's
// trailing `})["default"]` unwraps webpack's `__webpack_exports__["default"]` one level before
// assigning to `root["vuedraggable"]`, so `window.vuedraggable` IS the component itself, not an
// ES-module-shaped `{ default: ... }` namespace object (confirmed by runtime inspection: the
// unwrapped-one-too-many `vuedraggable.default` read is `undefined`, which silently registers
// `draggable` as an unresolved component - Vue then renders the literal, non-reactive
// `<draggable>` tag instead of the real one, and drag/rows/click all become permanent no-ops).
declare const vuedraggable: Record<string, unknown>;

export interface SelectOption {
    text: string;
    value: string;
    key: number;
}

interface RowInstance {
    option: SelectOption;
    defaultValue: string;
    $emit(event: string, ...args: unknown[]): void;
}

interface ModalInstance {
    data: { defaultValue: string };
    validOptions: SelectOption[];
    showModal: boolean;
    optionsFormattedList: string;
    defaultValue: string;
    isValid: boolean;
    jsonOptions: SelectOption[];
    $refs: { modal: HTMLElement; backdrop: HTMLElement };
    $emit(event: string, ...args: unknown[]): void;
}

const isNullOrWhiteSpace = (str: string | null | undefined): boolean => str == null || /^\s*$/.test(str);

let nextKey = 1;

const selectOptionsRow = {
    props: {
        option: { type: Object, required: true },
        defaultValue: { type: String, default: "" },
    },
    data() {
        return { t: getTranslations() };
    },
    methods: {
        remove(this: RowInstance) {
            this.$emit("remove-option", this.option);
        },
    },
    computed: {
        isSelected: {
            get(this: RowInstance) {
                if (!isNullOrWhiteSpace(this.option.value)) {
                    return this.option.value === this.defaultValue;
                }
                return this.option.text === this.defaultValue;
            },
            set(this: RowInstance, val: boolean) {
                this.$emit("set-default", val ? this.option : null);
            },
        },
        optionCheck(this: RowInstance) {
            return isNullOrWhiteSpace(this.option.value) ? this.option.text : this.option.value;
        },
    },
    template: `
        <tr>
            <td>
                <input type="text" class="form-control" v-model="option.text" v-on:change="$emit('reorder-option')" :placeholder="t.EnterTheText" />
            </td>
            <td>
                <input type="text" class="form-control courier" v-model="option.value" v-on:change="$emit('reorder-option')" :placeholder="t.EnterAValue" />
            </td>
            <td class="text-center align-middle">
                <div class="form-check ms-2">
                    <input type="checkbox" class="form-check-input"
                        :id="'option_' + option.key + '_id'"
                        :value="optionCheck"
                        v-model="isSelected">
                    <label :for="'option_' + option.key + '_id'" class="form-check-label" :title="t.SetAsDefault">
                        <span class="visually-hidden"></span>
                    </label>
                </div>
            </td>
            <td class="text-center">
                <a v-on:click.prevent.stop="remove" href="#" :title="t.RemoveElementFromList" class="btn">
                    <i class="fa-solid fa-xmark" aria-hidden="true"></i>
                </a>
            </td>
            <td class="text-center"><div class="btn cursor-move"><i class="fa-solid fa-up-down-left-right" aria-hidden="true"></i></div></td>
        </tr>
    `,
};

const selectOptionsTable = {
    components: { draggable: vuedraggable, "select-options-row": selectOptionsRow },
    props: {
        data: { type: Object, required: true },
    },
    data() {
        return { t: getTranslations() };
    },
    methods: {
        add() {
            (this as unknown as { $emit(e: string): void }).$emit("add-option");
        },
        onDragEnd() {
            (this as unknown as { $emit(e: string): void }).$emit("reorder-option");
        },
    },
    template: `
        <table class="table table-bordered table-sm select-options-table">
            <thead class="thead-light">
                <tr>
                    <th scope="col">{{ t.OptionText }}</th>
                    <th scope="col">{{ t.Value }}</th>
                    <th scope="col" colspan="3">{{ t.Default }}</th>
                </tr>
            </thead>
            <draggable v-model="data.options" tag="tbody" item-key="key" v-on:end="onDragEnd">
                <template v-slot:item="{ element: option }">
                    <select-options-row :option="option" :default-value="data.defaultValue"
                        v-on:remove-option="$emit('remove-option', $event)"
                        v-on:set-default="$emit('set-default', $event)"
                        v-on:reorder-option="$emit('reorder-option')" />
                </template>
            </draggable>
            <tfoot>
                <tr>
                    <td class="col-sm-12 text-center" colspan="5">
                        <a v-on:click="add()" class="btn btn-light w-100 btn-sm"><i class="fa-solid fa-plus small" aria-hidden="true"></i> {{ t.AddAnOption }}</a>
                    </td>
                </tr>
            </tfoot>
        </table>
    `,
};

const selectOptionsModal = {
    props: {
        data: { type: Object, required: true },
        showModal: { type: Boolean, default: false },
        validOptions: { type: Array, required: true },
    },
    data() {
        return {
            t: getTranslations(),
            optionsFormattedList: "[]",
            defaultValue: "",
            isValid: false,
            jsonOptions: [] as SelectOption[],
        };
    },
    methods: {
        closeModal(this: ModalInstance, save: boolean) {
            if (save) {
                this.$emit("modal-save", { options: this.jsonOptions, defaultValue: this.defaultValue });
            } else {
                this.$emit("modal-cancel");
            }
        },
    },
    watch: {
        showModal(this: ModalInstance, newVal: boolean) {
            if (newVal) {
                this.optionsFormattedList = JSON.stringify(this.validOptions, null, 2);
                this.defaultValue = this.data.defaultValue;
            } else {
                this.optionsFormattedList = "[]";
                this.defaultValue = "";
            }
        },
        optionsFormattedList(this: ModalInstance, newVal: string) {
            try {
                const parsed = JSON.parse(newVal);
                if (newVal && Array.isArray(parsed)) {
                    this.jsonOptions = parsed;
                    this.isValid = true;
                } else {
                    this.isValid = false;
                }
            } catch {
                this.isValid = false;
            }
        },
    },
    template: `
        <transition>
            <div class="modal fade text-start show d-block" role="dialog" aria-hidden="true" v-if="showModal" v-on:click.self="$emit('modal-cancel')">
                <div class="modal-dialog modal-dialog-centered" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">{{ t.EditData }}</h5>
                            <button type="button" class="btn-close" aria-label="Close" v-on:click="closeModal(false)"></button>
                        </div>
                        <div class="modal-body">
                            <div class="ocat-wrapper">
                                <label for="select-options-textarea" class="ocat-label">{{ t.Options }}</label>
                                <div class="ocat-end">
                                    <textarea id="select-options-textarea" name="select-options-textarea" rows="8" class="form-control"
                                    :class="{ 'is-invalid': !isValid }"
                                    v-model="optionsFormattedList"></textarea>
                                    <span class="hint">{{ t.JsonRepresentationHint }}</span>
                                </div>
                            </div>
                            <div class="ocat-wrapper">
                                <label for="options-default-value" class="ocat-label">{{ t.DefaultValue }}</label>
                                <div class="ocat-end">
                                    <input id="options-default-value" name="options-default-value" class="form-control" type="text" v-model="defaultValue" />
                                    <span class="hint">{{ t.DefaultValueHint }}</span>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-primary btn-submit"
                            :disabled="!isValid"
                            v-on:click="closeModal(true)">{{ t.Ok }}</button>
                            <button type="button" class="btn btn-secondary" v-on:click="closeModal(false)">{{ t.Cancel }}</button>
                        </div>
                    </div>
                </div>
            </div>
        </transition>
        <div class="modal-backdrop fade show" v-if="showModal"></div>
    `,
};

export interface SelectPartEditorConfig {
    element: HTMLElement;
    options: SelectOption[];
    defaultValue: string;
    translations: Record<string, string>;
    hiddenDefaultValueInputId: string;
    hiddenDefaultValueInputName: string;
    hiddenOptionsInputId: string;
    hiddenOptionsInputName: string;
}

interface RootInstance {
    state: { options: SelectOption[]; defaultValue: string };
    showModal: boolean;
    debounceTimeout: ReturnType<typeof setTimeout> | null;
    validOptions: SelectOption[];
}

const initSelectPartEditor = (config: SelectPartEditorConfig): void => {
    setTranslations(config.translations);
    const t = getTranslations();

    config.options.forEach((o) => {
        o.key = nextKey++;
    });

    const mountTarget = config.element.querySelector<HTMLElement>(".select-part-editor-mount") ?? config.element;

    Vue.createApp({
        components: { "select-options-table": selectOptionsTable, "select-options-modal": selectOptionsModal },
        data() {
            return {
                t,
                state: { options: config.options, defaultValue: config.defaultValue },
                debounceTimeout: null as ReturnType<typeof setTimeout> | null,
                showModal: false,
            };
        },
        methods: {
            cancelChanges(this: RootInstance & { showModal: boolean }) {
                this.showModal = false;
            },
            updateChanges(this: RootInstance & { showModal: boolean; debouncePreview(): void }, changes: { options: SelectOption[]; defaultValue: string }) {
                this.state.options = changes.options
                    .filter((y) => !isNullOrWhiteSpace(y.text))
                    .map((x) => {
                        x.key = nextKey++;
                        return x;
                    });
                this.state.defaultValue = changes.defaultValue;
                this.showModal = false;
                this.debouncePreview();
            },
            setDefaultValue(this: RootInstance & { debouncePreview(): void }, opt: SelectOption | null) {
                if (opt == null) {
                    this.state.defaultValue = "";
                } else {
                    this.state.defaultValue = !isNullOrWhiteSpace(opt.value) ? opt.value : opt.text;
                }
                this.debouncePreview();
            },
            addOption(this: RootInstance & { debouncePreview(): void }) {
                this.state.options.push({ text: "", value: "", key: nextKey++ });
                this.debouncePreview();
            },
            removeOption(this: RootInstance & { debouncePreview(): void }, opt: SelectOption) {
                const index = this.state.options.findIndex((c) => c.key === opt.key);
                if (index > -1) {
                    this.state.options.splice(index, 1);
                    this.debouncePreview();
                }
            },
            reorderOption(this: { debouncePreview(): void }) {
                this.debouncePreview();
            },
            debouncePreview(this: RootInstance) {
                if (this.debounceTimeout) {
                    clearTimeout(this.debounceTimeout);
                }
                this.debounceTimeout = setTimeout(() => {
                    document.dispatchEvent(new Event("contentpreview:render"));
                }, 500);
            },
        },
        computed: {
            stringify(this: RootInstance) {
                return JSON.stringify(this.validOptions);
            },
            validOptions(this: RootInstance) {
                return this.state.options
                    .map((x) => ({ text: x.text, value: x.value }))
                    .filter((x) => !isNullOrWhiteSpace(x.text));
            },
        },
        template: `
            <a href="#" v-on:click.prevent.stop="showModal = true" class="float-end" :title="t.EditData"><i class="fa-solid fa-pen-to-square" aria-hidden="true"></i></a>
            <select-options-table :data="state"
                                  v-on:reorder-option="reorderOption"
                                  v-on:add-option="addOption"
                                  v-on:remove-option="removeOption"
                                  v-on:set-default="setDefaultValue">
            </select-options-table>
            <select-options-modal :data="state"
                                  :valid-options="validOptions"
                                  :show-modal="showModal"
                                  v-on:modal-cancel="cancelChanges"
                                  v-on:modal-save="updateChanges">
            </select-options-modal>
            <input id="${config.hiddenDefaultValueInputId}" name="${config.hiddenDefaultValueInputName}" type="hidden" v-model="state.defaultValue">
            <input id="${config.hiddenOptionsInputId}" name="${config.hiddenOptionsInputName}" type="hidden" :value="stringify" />
        `,
    }).mount(mountTarget);
};

export default initSelectPartEditor;
