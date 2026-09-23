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

// Optional name -> value auto-fill (PR #19581's "Set the value automatically in Predefined List
// editor"): while a row's targetKey has never been touched directly, typing in sourceKey mirrors
// the text into targetKey, so a new option doesn't require typing the same text twice. Tracked
// per-row (see ROW_TOUCHED, a WeakSet keyed by the row object itself - stable across vuedraggable
// reordering, unlike the original Vue 2 implementation's global previousIndex/previousName pair,
// which is exactly what made it cross-contaminate unrelated rows: once ANY row's value diverged
// from its name, editing a DIFFERENT row's name could still overwrite that first row's value on
// the next change detection pass, because the "is this a manual edit" check compared the new
// value against one shared previousName instead of that specific row's own history) once the
// user edits targetKey directly (even to blank it out), auto-fill permanently stops for that row -
// matching review requirement #1. A brand new row always starts untouched, so its first sourceKey
// keystroke mirrors immediately - matching requirement #2. Because tracking is per-row rather
// than by matching text content, two rows that happen to share a label do NOT interfere with each
// other's touched state or with which row is selected as defaultColumn's default - avoiding
// requirement #3's regression.
export interface OptionsTableAutoFillColumn {
    sourceKey: string;
    targetKey: string;
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
    autoFillColumn?: OptionsTableAutoFillColumn;
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
    autoFillColumn?: OptionsTableAutoFillColumn;
}

interface JsonModalInstance {
    rows: Record<string, string>[];
    modal: InstanceType<typeof bootstrap.Modal> | null;
}

// Tracks, per row object, whether its autoFillColumn.targetKey has ever been edited directly -
// see OptionsTableAutoFillColumn above for why this must be per-row-identity rather than a single
// shared "previous" pair. A WeakSet (not a plain Set) so removed rows don't leak memory, and
// module-scoped (not component `data()`) so it isn't itself reactive - Vue tracking writes to it
// would otherwise trigger unrelated re-renders on every keystroke.
const touchedTargets = new WeakSet<Record<string, string>>();

// A row whose autoFillColumn.targetKey already holds a non-blank value did NOT get there via this
// session's auto-fill (a brand new row always starts with an empty target - see add() below), so
// it must be treated as already touched: either it's an existing, previously-saved option loaded
// from the server (the review-reported bug - editing that option's label would otherwise silently
// overwrite its real, possibly-deliberately-different value), or it came from a JSON-modal paste
// that already specified both a label and a value. Called for every row set the component didn't
// itself just create blank (initial load, and every JSON-modal replacement).
const markPrefilledRowsAsTouched = (rows: Record<string, string>[], autoFillColumn?: OptionsTableAutoFillColumn | null): void => {
    if (!autoFillColumn) {
        return;
    }

    for (const row of rows) {
        if ((row[autoFillColumn.targetKey] ?? "").trim() !== "") {
            touchedTargets.add(row);
        }
    }
};

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
                            <input type="text" class="form-control courrier" v-model="row[column.key]" :placeholder="column.placeholderKey ? t[column.placeholderKey] : ''" v-on:input="onColumnInput(row, column.key)" />
                        </td>
                        <td v-if="defaultColumn && defaultColumn.mode === 'radio'" class="text-center align-middle">
                            <div class="form-check ms-2">
                                <input type="radio" class="form-check-input" :id="'customRadio_' + index" :name="defaultColumn.radioGroupName" :value="row[defaultColumn.key]" :checked="row === selectedRow" v-on:click="onRadioClick(row)" />
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
        autoFillColumn: { type: Object, default: null },
        rootDefaultValue: { type: String, default: "" },
        addKey: { type: String, required: true },
        removeRowKey: { type: String, required: true },
    },
    emits: ["update:rows", "update:rootDefaultValue"],
    data() {
        return {
            t: getTranslations(),
            // Tracks the currently-selected radio's ROW OBJECT (identity), not its value string -
            // see review requirement #3 ("adding a new option with the same label as a different
            // default option should not change the default to the new option"). Auto-fill (see
            // OptionsTableAutoFillColumn) makes two rows sharing a label very likely to also share
            // a value once one of the two hasn't had its value directly edited yet; a native radio
            // group's checked state is otherwise determined purely by value equality against
            // v-model, so two same-valued rows would both render checked and clicking either would
            // ambiguously "select" both. Tracking the actual row reference sidesteps that: checked
            // state below compares `row === selectedRow`, which is unambiguous even when two rows'
            // values are textually identical. Initialized once, on creation, by matching rows
            // against the initial rootDefaultValue prop - see created() below.
            selectedRow: null as Record<string, string> | null,
        };
    },
    created(this: RowsTableInstance & { selectedRow: Record<string, string> | null; rootDefaultValue: string }) {
        markPrefilledRowsAsTouched(this.rows, this.autoFillColumn);

        if (this.defaultColumn && this.defaultColumn.mode === "radio" && this.rootDefaultValue) {
            this.selectedRow = this.rows.find((row) => row[this.defaultColumn!.key] === this.rootDefaultValue) ?? null;
        }
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
        onColumnInput(
            this: RowsTableInstance & {
                selectedRow: Record<string, string> | null;
                $emit: (event: string, ...args: unknown[]) => void;
            },
            row: Record<string, string>,
            columnKey: string,
        ) {
            const autoFill = this.autoFillColumn;
            if (!autoFill) {
                return;
            }

            if (columnKey === autoFill.targetKey) {
                // A direct edit to the target column (even clearing it to "") permanently opts
                // this row out of auto-fill - matching review requirement #1 ("when a value is
                // defined for an option and someone changes the option label it should not
                // change its value").
                touchedTargets.add(row);
                return;
            }

            if (columnKey === autoFill.sourceKey && !touchedTargets.has(row)) {
                // Mirror the label into the not-yet-touched value column - requirement #2 ("when
                // someone adds a new option the value should be prefilled with the same text as
                // the option label"). Setting row[targetKey] here does NOT mark this row as
                // touched (only a direct edit to targetKey does, above), so typing continues to
                // mirror on every subsequent keystroke until the user edits the value directly.
                row[autoFill.targetKey] = row[columnKey];

                // If this row is the currently-selected radio default and its value column is the
                // auto-filled target, the hidden rootDefaultValue field (bound by value, not row
                // identity - see selectedRow above) must be refreshed too, or the form would post
                // a stale default value from before the label/auto-fill edit.
                if (this.defaultColumn?.mode === "radio" && this.selectedRow === row && this.defaultColumn.key === autoFill.targetKey) {
                    this.$emit("update:rootDefaultValue", row[autoFill.targetKey]);
                }
            }
        },
        onRadioClick(
            this: RowsTableInstance & {
                selectedRow: Record<string, string> | null;
                $emit: (event: string, ...args: unknown[]) => void;
            },
            row: Record<string, string>,
        ) {
            const key = this.defaultColumn!.key;
            if (this.selectedRow === row) {
                // Second click on the already-selected row's radio: native radios have no
                // "uncheck" gesture, so this mirrors the original Vue 2 template's
                // previouslyChecked toggle-off behavior.
                this.selectedRow = null;
                this.$emit("update:rootDefaultValue", "");
            } else {
                this.selectedRow = row;
                this.$emit("update:rootDefaultValue", row[key]);
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
        autoFillColumn: { type: Object, default: null },
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
        updateFromJson(
            this: JsonModalInstance & {
                autoFillColumn?: OptionsTableAutoFillColumn | null;
                $emit: (event: string, ...args: unknown[]) => void;
            },
            value: string,
        ) {
            try {
                const rows = JSON.parse(value);
                // Rows pasted/edited as raw JSON already specify both columns explicitly (or the
                // admin typed only a label and expects it to still auto-fill on the next row edit
                // if left blank) - either way these are fresh row objects the auto-fill tracking
                // above has never seen, so treat any non-blank target value the same as an
                // existing, previously-saved option: already touched, never silently overwritten.
                markPrefilledRowsAsTouched(rows, this.autoFillColumn);
                this.$emit("update:rows", rows);
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
                autoFillColumn: config.autoFillColumn ?? null,
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
                :auto-fill-column="autoFillColumn"
                :add-key="addKey"
                :remove-row-key="removeRowKey"
            ></options-table>
            <options-modal
                ref="modal"
                v-model:rows="rows"
                :auto-fill-column="autoFillColumn"
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
