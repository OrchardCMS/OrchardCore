import { getTranslations, setTranslations } from "../helpers/localizations";

// Shared by 4 near-identical Vue 2 "draggable key/value(s) table + JSON-edit-modal" widgets that
// existed as separate copy-pasted files before this migration:
//   - OrchardCore.ContentFields' TextFieldPredefinedListEditorSettings.Edit.cshtml (2 columns,
//     name/value, "default" column in radio mode)
//   - OrchardCore.ContentFields' MultiTextFieldSettings.Edit.cshtml (2 columns, name/value,
//     "default" column in checkbox mode)
//   - OrchardCore.OpenId's OpenIdClientSettings.Edit.cshtml (2 columns, name/value, no "default"
//     column)
//   - OrchardCore.Seo's SeoMetaPart.Edit.cshtml (5 columns: content/name/property/httpEquiv/
//     charset, no "default" column)
//
// Follows this repo's established plain-.ts bloom-component shape (Vue.createApp + template
// string, Options API) - see content-type-picker.ts and translation-editor.ts - rather than a
// .vue SFC: no SFC compilation exists anywhere in this repo's shared bloom/ workspace, only in
// the fully independent media-picker/media-gallery Vite apps.
//
// All display text is resolved through the IJSLocalizer / getTranslations() pattern (see
// src/docs/reference/modules/Localize/javascript-localization.md and the Media Gallery app,
// which established it) rather than passed pre-translated from Razor: each consumer view calls
// Orchard.GetJSLocalizations("options-table-editor") and serializes the result into a
// translations="..." attribute; this component seeds the shared store once via setTranslations()
// and every label below is looked up by key through getTranslations(), exactly like
// media-gallery's App.vue does with its own "media-gallery" group.
declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string | Element): void };
};

declare const bootstrap: typeof import("bootstrap");

// Loaded globally as the "vue-draggable" UMD resource (see each consumer module's
// ResourceManifest changes) - the Vue 3 build of vuedraggable (vuedraggable@4.1.0, per
// ResourceManagementOptionsConfiguration.cs), per the Task 1 decision recorded in
// .hermes/plans/2026-08-22_023850-vue2-to-vue3-vite-ts-migration.md ("## Vue-draggable
// replacement decision"). The UMD wrapper's trailing `})["default"]` unwraps webpack's
// `__webpack_exports__["default"]` one level before assigning to `root["vuedraggable"]`, so
// `window.vuedraggable` IS the component itself, not an ES-module-shaped `{ default: ... }`
// namespace object - confirmed by runtime inspection: reading `vuedraggable.default` is
// `undefined`, which silently registers `draggable` as an unresolved component, so Vue renders
// the literal non-reactive `<draggable>` tag instead of the real one and drag/rows/click become
// permanent no-ops (caught while writing functional test coverage for issue #19772 - see
// select-part-editor.ts for the fix this mirrors).
declare const vuedraggable: Record<string, unknown>;

export interface OptionsTableColumn {
    key: string;
    // Keys into the shared translations store (getTranslations()), NOT pre-translated text -
    // each consumer module's IJSLocalizer group ("options-table-editor") supplies the actual
    // localized strings; see ContentFieldsJSLocalizer/OpenIdJSLocalizer/SeoJSLocalizer.
    labelKey: string;
    placeholderKey?: string;
}

export type DefaultColumnMode = "radio" | "checkbox";

export interface OptionsTableDefaultColumn {
    key: string;
    labelKey: string;
    mode: DefaultColumnMode;
    // Required when mode is "radio": Vue 3's v-model on a radio group needs every radio input in
    // the group to share a `name` attribute so the browser (and Vue's v-model compilation) treats
    // them as mutually exclusive - mirrors the original hand-written Vue 2 template's shared
    // `name="@Html.NameFor(m => m.DefaultValue)"` attribute.
    radioGroupName?: string;
}

// Translation keys this component looks up for its own chrome (add button, modal, JSON hint) -
// every consumer's IJSLocalizer must supply all of these under the "options-table-editor" group,
// in addition to whatever column/default-column labelKeys it references.
export interface OptionsTableEditorTranslationKeys {
    addKey: string;
    editDataKey: string;
    okKey: string;
    cancelKey: string;
    removeRowKey: string;
    jsonTextareaLabelKey: string;
    jsonTextareaHintKey: string;
}

export interface OptionsTableEditorConfig extends OptionsTableEditorTranslationKeys {
    element: HTMLElement;
    rows: Record<string, string>[];
    columns: OptionsTableColumn[];
    defaultColumn?: OptionsTableDefaultColumn;
    initialDefaultValue?: string;
    // When set, rows whose value at this column key is blank/whitespace-only are dropped from the
    // serialized hidden-input payload on every change - mirrors the original Vue 2
    // getOptionsFormattedList()/getParametersFormattedList()'s
    // `.filter(x => !IsNullOrWhiteSpace(x.name))`, used by ContentFields' optionsEditor and
    // OpenId's parametersEditor but NOT by Seo's customMetaTagsEditor (which serializes every row
    // unfiltered - its columns are all individually optional, with no single required key).
    filterEmptyKey?: string;
    // Hidden <input> that receives JSON.stringify(rows) on every change, for classic form postback.
    hiddenInputId: string;
    hiddenInputName: string;
    modalBodyElements: HTMLCollectionOf<Element>;
}

interface RowsTableInstance {
    rows: Record<string, string>[];
    columns: OptionsTableColumn[];
    defaultColumn?: OptionsTableDefaultColumn;
}

interface JsonModalInstance {
    rows: Record<string, string>[];
    modal: InstanceType<typeof bootstrap.Modal> | null;
}

// Table + inline row editing, with an optional "default" column that renders as either a radio
// (single-select across the whole table, sharing one form-field name) or a per-row checkbox
// (multi-select, one boolean per row) depending on the consumer - see OptionsTableDefaultColumn.
const optionsTableComponent = {
    template: `
        <table class="table table-bordered table-sm options-table">
            <thead class="thead-light">
                <tr>
                    <th scope="col" v-for="column in columns" :key="column.key">{{ t[column.labelKey] }}</th>
                    <th scope="col" v-if="defaultColumn" :colspan="defaultColumn.mode === 'radio' ? 3 : 1">{{ t[defaultColumn.labelKey] }}</th>
                </tr>
            </thead>
            <draggable v-model="rows" tag="tbody" item-key="__row" handle=".cursor-move">
                <template #item="{ element: row, index }">
                    <tr>
                        <td v-for="column in columns" :key="column.key">
                            <input type="text" class="form-control courrier" v-model="row[column.key]" :placeholder="column.placeholderKey ? t[column.placeholderKey] : ''" />
                        </td>
                        <td v-if="defaultColumn && defaultColumn.mode === 'radio'" class="text-center align-middle">
                            <div class="form-check ms-2">
                                <input type="radio" class="form-check-input" :id="'customRadio_' + index" :name="defaultColumn.radioGroupName" :value="row[defaultColumn.key]" v-model="rootDefaultValue" v-on:click="onRadioClick(row[defaultColumn.key])" />
                                <label class="form-check-label" :title="t[defaultColumn.labelKey]" v-bind:for="'customRadio_' + index"></label>
                            </div>
                        </td>
                        <td v-else-if="defaultColumn" class="text-center align-middle">
                            <div class="form-check ms-2">
                                <input type="checkbox" class="form-check-input" :id="'customRadio_' + index" v-model="row[defaultColumn.key]" true-value="true" false-value="" />
                                <label class="form-check-label" :title="t[defaultColumn.labelKey]" v-bind:for="'customRadio_' + index"></label>
                            </div>
                        </td>
                        <td class="text-center">
                            <a v-on:click="remove(index)" href="javascript:void(0)" :title="t[removeRowKey]" class="btn">
                                <i class="fa-solid fa-xmark" aria-hidden="true"></i>
                            </a>
                        </td>
                        <td class="text-center"><div class="btn cursor-move"><i class="fa-solid fa-up-down-left-right" aria-hidden="true"></i></div></td>
                    </tr>
                </template>
            </draggable>
        </table>
        <a v-on:click="add()" class="btn btn-light w-100 btn-sm"><i class="fa-solid fa-plus small" aria-hidden="true"></i> {{ t[addKey] }}</a>
    `,
    components: { draggable: vuedraggable },
    props: {
        rows: { type: Array, required: true },
        columns: { type: Array, required: true },
        defaultColumn: { type: Object, default: null },
        rootDefaultValue: { type: String, default: "" },
        addKey: { type: String, required: true },
        removeRowKey: { type: String, required: true },
    },
    emits: ["update:rows", "update:rootDefaultValue"],
    data() {
        return {
            t: getTranslations(),
            // Tracks which value was checked the last time a radio was clicked, mirroring the
            // original Vue 2 template's module-scoped `previouslyChecked` - lets a second click on
            // the already-selected radio clear the selection entirely (radios have no native
            // "uncheck" gesture otherwise). Tracked by value rather than index, since drag
            // reordering (unlike the original, non-draggable radio column) can change a row's
            // index without changing its identity.
            previouslyCheckedValue: null as string | null,
        };
    },
    methods: {
        add(this: RowsTableInstance) {
            const row: Record<string, string> = {};
            this.columns.forEach((column) => (row[column.key] = ""));
            if (this.defaultColumn && this.defaultColumn.mode === "checkbox") {
                row[this.defaultColumn.key] = "";
            }
            this.rows.push(row);
        },
        remove(this: RowsTableInstance, index: number) {
            this.rows.splice(index, 1);
        },
        onRadioClick(
            this: { previouslyCheckedValue: string | null; $emit: (event: string, ...args: unknown[]) => void },
            value: string,
        ) {
            if (this.previouslyCheckedValue === value) {
                this.$emit("update:rootDefaultValue", "");
                this.previouslyCheckedValue = null;
            } else {
                this.previouslyCheckedValue = value;
            }
        },
    },
};

// The raw-JSON textarea fallback editor, shown in a Bootstrap modal - lets an admin paste/edit
// the whole rows array as JSON in one go instead of row-by-row.
const jsonModalComponent = {
    template: `
        <div class="modal fade text-start" role="dialog" aria-hidden="true" ref="modalRoot">
            <div class="modal-dialog modal-dialog-centered" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">{{ t[editDataKey] }}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="ocat-wrapper">
                            <label class="ocat-label">{{ t[jsonTextareaLabelKey] }}</label>
                            <div class="ocat-end">
                                <textarea rows="8" class="form-control" :value="JSON.stringify(rows)" v-on:input="updateFromJson($event.target.value)"></textarea>
                                <span class="hint">{{ t[jsonTextareaHintKey] }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary btn-submit" v-on:click="closeModal()">{{ t[okKey] }}</button>
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">{{ t[cancelKey] }}</button>
                    </div>
                </div>
            </div>
        </div>
    `,
    props: {
        rows: { type: Array, required: true },
        editDataKey: { type: String, required: true },
        jsonTextareaLabelKey: { type: String, required: true },
        jsonTextareaHintKey: { type: String, required: true },
        okKey: { type: String, required: true },
        cancelKey: { type: String, required: true },
    },
    emits: ["update:rows"],
    data() {
        return {
            t: getTranslations(),
            modal: null as InstanceType<typeof bootstrap.Modal> | null,
        };
    },
    methods: {
        updateFromJson(this: JsonModalInstance & { $emit: (event: string, ...args: unknown[]) => void }, value: string) {
            try {
                this.$emit("update:rows", JSON.parse(value));
            } catch {
                // Malformed JSON mid-edit: ignore until the admin fixes it, same as the original
                // Vue 2 template's bare `data.options = JSON.parse($event.target.value)`, which
                // would have thrown synchronously on every keystroke of invalid JSON.
            }
        },
        showModal(this: JsonModalInstance & { $refs: Record<string, Element> }) {
            const modalRoot = this.$refs.modalRoot;
            if (modalRoot) {
                this.modal = new bootstrap.Modal(modalRoot);
                this.modal.show();
            }
        },
        closeModal(this: JsonModalInstance) {
            this.modal?.hide();
        },
    },
};

// Mounts one OptionsTableEditor instance. Each consumer module's own thin .ts entry (e.g.
// options-editor-fields.ts, openid-client-settings.ts, seo-meta-part.ts) calls this with its
// specific column/default-column/translation-key config; see those files for the
// observeAndInit-wrapped call site that makes this AJAX-widget-injection-safe.
const initOptionsTableEditor = (config: OptionsTableEditorConfig): void => {
    const modalBodyElement = config.modalBodyElements[0];
    // Consumer views mount into a dedicated inner element (class "options-table-editor-mount")
    // rather than config.element itself, since config.element is typically a shared wrapper div
    // that also holds sibling, non-Vue-owned markup (e.g. TextFieldPredefinedListEditorSettings'
    // "Editor" dropdown lives in the same .field-editor wrapper as the options table) - mounting
    // Vue directly on config.element would let it silently replace that sibling markup on its
    // first render. Falls back to config.element for any consumer that has no such sibling
    // content and mounts the whole wrapper directly.
    const mountTarget = config.element.querySelector<HTMLElement>(".options-table-editor-mount") ?? config.element;

    Vue.createApp({
        components: {
            "options-table": optionsTableComponent,
            "options-modal": jsonModalComponent,
        },
        data() {
            return {
                t: getTranslations(),
                rows: config.rows,
                columns: config.columns,
                defaultColumn: config.defaultColumn ?? null,
                // Only meaningful when config.defaultColumn.mode === "radio": the single selected
                // row's defaultColumn.key value, bound via v-model in optionsTableComponent.
                rootDefaultValue: config.initialDefaultValue ?? "",
                addKey: config.addKey,
                editDataKey: config.editDataKey,
                removeRowKey: config.removeRowKey,
                jsonTextareaLabelKey: config.jsonTextareaLabelKey,
                jsonTextareaHintKey: config.jsonTextareaHintKey,
                okKey: config.okKey,
                cancelKey: config.cancelKey,
            };
        },
        computed: {
            rowsJson(this: { rows: Record<string, string>[] }) {
                const filterKey = config.filterEmptyKey;
                const filtered = filterKey ? this.rows.filter((row) => (row[filterKey] ?? "").trim() !== "") : this.rows;
                return JSON.stringify(filtered);
            },
        },
        methods: {
            showModal(this: { $refs: Record<string, { showModal(): void }> }) {
                if (modalBodyElement) {
                    this.$refs.modal?.showModal();
                }
            },
        },
        template: `
            <a href="javascript:void(0)" v-on:click="showModal" class="float-end" :title="t[editDataKey]"><i class="fa-solid fa-pen-to-square" aria-hidden="true"></i></a>
            <options-table
                v-model:rows="rows"
                v-model:root-default-value="rootDefaultValue"
                :columns="columns"
                :default-column="defaultColumn"
                :add-key="addKey"
                :remove-row-key="removeRowKey"
            ></options-table>
            <options-modal
                ref="modal"
                v-model:rows="rows"
                :edit-data-key="editDataKey"
                :json-textarea-label-key="jsonTextareaLabelKey"
                :json-textarea-hint-key="jsonTextareaHintKey"
                :ok-key="okKey"
                :cancel-key="cancelKey"
            ></options-modal>
            <input class="form-control" :id="'${config.hiddenInputId}'" :name="'${config.hiddenInputName}'" type="hidden" :value="rowsJson" />
        `,
    }).mount(mountTarget);
};

export { setTranslations };
export default initOptionsTableEditor;
