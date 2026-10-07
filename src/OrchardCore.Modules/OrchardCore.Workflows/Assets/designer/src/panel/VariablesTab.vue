<script setup lang="ts">
import { computed, nextTick, ref, toRaw, useId, watch } from "vue";
import type { DesignerApi } from "../api/designerApi";
import { DesignerApiError } from "../api/designerApi";
import type { VariableDefinition, VariableType } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import type { RevisionTask } from "../services/revisionQueue";
import { editorOf, formatDefault, formatValue, parseDefault, typeDisplayName } from "../variables/variableValues";
import { t } from "../i18n";

// A declaration being edited. Its default is kept as the text of its input until it is saved.
interface Row {
    key: number;
    name: string;
    typeName: string;
    defaultText: string;
    description: string;
    error: string | null;
}

const props = withDefaults(
    defineProps<{
        store: DesignerStore;
        api: DesignerApi;
        readOnly?: boolean;
        /**
         * Runs a request that changes the draft, in the designer's revision queue.
         */
        mutate?: <T>(task: RevisionTask<T>) => Promise<T>;
    }>(),
    {
        readOnly: false,
        mutate: undefined,
    },
);

const emit = defineEmits<{
    (event: "error", error: unknown): void;
}>();

const state = props.store.state;
const ids = `wfd-variables-${useId()}`;
const rows = ref<Row[]>([]);
const list = ref<HTMLElement | null>(null);
let nextKey = 0;

// Increments on every edit, so a save that completes doesn't overwrite what was typed in the meantime.
let editVersion = 0;

// The variables a save of this tab stored, which don't need to be loaded into the rows again.
let savedVariables: VariableDefinition[] | null = null;

const mutate = <T,>(task: RevisionTask<T>) => (props.mutate ? props.mutate(task) : task(state.revision));

const toRow = (variable: VariableDefinition): Row => ({
    key: nextKey++,
    name: variable.name,
    typeName: variable.typeName,
    defaultText: formatDefault(variable.defaultValue, editorOf(state.variableTypes, variable.typeName)),
    description: variable.description ?? "",
    error: null,
});

// The variables were loaded (or loaded again after a conflict or a restore).
watch(
    () => state.variables,
    (variables) => {
        if (toRaw(variables) !== savedVariables) {
            rows.value = variables.map(toRow);
        }
    },
    { immediate: true },
);

const instanceValues = computed(() => state.instance?.variableValues ?? null);

// The types a row can pick, including its own type when it isn't available (its feature is disabled).
const typeOptions = (row: Row): VariableType[] =>
    state.variableTypes.some((type) => type.name.toLowerCase() === row.typeName.toLowerCase())
        ? state.variableTypes
        : [...state.variableTypes, { name: row.typeName, displayName: row.typeName, editor: "text" }];

const editor = (row: Row) => editorOf(state.variableTypes, row.typeName);

// v-model gives a number input's value as a number.
const defaultTextOf = (row: Row) => String(row.defaultText ?? "");

// A row added and left empty isn't saved.
const isBlank = (row: Row) => !row.name.trim() && !defaultTextOf(row).trim() && !row.description.trim();

const touch = () => {
    editVersion++;
};

/**
 * Saves every declaration. A default that can't be read, or a declaration the server rejects, shows its
 * error on its row.
 */
const save = async () => {
    if (props.readOnly) {
        return;
    }

    const sent: Row[] = [];
    const variables: VariableDefinition[] = [];
    let invalid = false;

    for (const row of rows.value) {
        if (isBlank(row)) {
            row.error = null;
            continue;
        }

        const parsed = parseDefault(defaultTextOf(row), editor(row));

        if (!parsed.valid) {
            row.error = t("InvalidDefaultValue", typeDisplayName(state.variableTypes, row.typeName));
            invalid = true;
            continue;
        }

        row.error = null;
        sent.push(row);
        variables.push({
            name: row.name.trim(),
            typeName: row.typeName,
            defaultValue: parsed.value,
            description: row.description.trim() || null,
        });
    }

    if (invalid) {
        return;
    }

    const version = editVersion;

    try {
        const result = await mutate((revision) => props.api.saveVariables(revision, variables));

        savedVariables = result.variables;
        props.store.applyServerChange(result.revision, result.issues);
        state.variables = result.variables;

        // The server trims the names.
        if (version === editVersion) {
            sent.forEach((row, index) => {
                row.name = result.variables[index]?.name ?? row.name;
            });
        }
    } catch (error) {
        const errors = error instanceof DesignerApiError && error.status === 400 ? error.problem.variableErrors : undefined;

        if (errors?.length) {
            for (const variableError of errors) {
                const row = sent[variableError.index];

                if (row) {
                    row.error = variableError.message;
                }
            }

            return;
        }

        emit("error", error);
    }
};

const add = async () => {
    const row: Row = { key: nextKey++, name: "", typeName: state.variableTypes[0]?.name ?? "string", defaultText: "", description: "", error: null };

    rows.value.push(row);
    touch();
    await nextTick();
    list.value?.querySelector<HTMLInputElement>(`[data-key="${row.key}"] [data-cy=variable-name]`)?.focus();
};

const remove = async (row: Row) => {
    rows.value.splice(rows.value.indexOf(row), 1);
    touch();

    if (!isBlank(row)) {
        await save();
    }
};

// A default that the new type's editor can't show is cleared.
const onTypeChange = async (row: Row) => {
    if (!parseDefault(defaultTextOf(row), editor(row)).valid || editor(row) === "none") {
        row.defaultText = "";
    }

    touch();
    await save();
};

const onDefaultChange = async () => {
    touch();
    await save();
};

const valueOf = (variable: VariableDefinition) => {
    const values = instanceValues.value;

    return values && Object.prototype.hasOwnProperty.call(values, variable.name) ? formatValue(values[variable.name]) : null;
};

const defaultOf = (variable: VariableDefinition) => formatDefault(variable.defaultValue, editorOf(state.variableTypes, variable.typeName));
</script>

<template>
    <div class="wfd-variables" data-cy="variables-tab">
        <p class="wfd-panel-intro">{{ readOnly ? t("VariablesTabHintReadOnly") : t("VariablesTabHint") }}</p>

        <template v-if="readOnly">
            <p v-if="state.variables.length === 0" class="wfd-panel-message" data-cy="variables-empty">{{ t("NoVariables") }}</p>
            <table v-else class="table table-sm wfd-variable-table" data-cy="variables-table">
                <thead>
                    <tr>
                        <th scope="col">{{ t("VariableName") }}</th>
                        <th scope="col">{{ t("VariableType") }}</th>
                        <th scope="col">{{ state.instance ? t("VariableValue") : t("VariableDefault") }}</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="variable in state.variables" :key="variable.name" :data-cy="`variable-row-${variable.name}`">
                        <td>
                            <code>{{ variable.name }}</code>
                            <div v-if="variable.description" class="small text-secondary">{{ variable.description }}</div>
                        </td>
                        <td>{{ typeDisplayName(state.variableTypes, variable.typeName) }}</td>
                        <td v-if="state.instance" class="wfd-variable-value" data-cy="variable-value">
                            <code v-if="valueOf(variable) !== null">{{ valueOf(variable) }}</code>
                            <span v-else class="text-secondary">{{ t("NoValue") }}</span>
                        </td>
                        <td v-else class="wfd-variable-value">
                            <code v-if="defaultOf(variable)">{{ defaultOf(variable) }}</code>
                        </td>
                    </tr>
                </tbody>
            </table>
        </template>

        <template v-else>
            <p v-if="rows.length === 0" class="wfd-panel-message" data-cy="variables-empty">{{ t("NoVariables") }}</p>
            <ul ref="list" class="wfd-variable-list list-unstyled">
                <li v-for="row in rows" :key="row.key" class="wfd-variable" :class="{ 'is-invalid': row.error }" :data-key="row.key" data-cy="variable">
                    <div class="wfd-variable-head">
                        <input
                            :id="`${ids}-name-${row.key}`"
                            v-model="row.name"
                            type="text"
                            class="form-control form-control-sm font-monospace"
                            :class="{ 'is-invalid': row.error }"
                            :placeholder="t('VariableName')"
                            :aria-label="t('VariableName')"
                            :aria-describedby="row.error ? `${ids}-error-${row.key}` : undefined"
                            spellcheck="false"
                            autocomplete="off"
                            data-cy="variable-name"
                            @input="touch"
                            @change="save"
                        />
                        <select v-model="row.typeName" class="form-select form-select-sm" :aria-label="t('VariableType')" data-cy="variable-type" @change="onTypeChange(row)">
                            <option v-for="type in typeOptions(row)" :key="type.name" :value="type.name">{{ type.displayName }}</option>
                        </select>
                        <button
                            type="button"
                            class="btn btn-sm btn-link text-danger"
                            :title="t('DeleteVariable', row.name || t('NewVariable'))"
                            :aria-label="t('DeleteVariable', row.name || t('NewVariable'))"
                            data-cy="variable-delete"
                            @click="remove(row)"
                        >
                            <i class="fa-solid fa-trash" aria-hidden="true"></i>
                        </button>
                    </div>

                    <p v-if="editor(row) === 'none'" class="small text-secondary mb-0" data-cy="variable-no-default">{{ t("NoDefaultForType") }}</p>
                    <select
                        v-else-if="editor(row) === 'boolean'"
                        v-model="row.defaultText"
                        class="form-select form-select-sm"
                        :aria-label="t('VariableDefault')"
                        data-cy="variable-default"
                        @change="onDefaultChange"
                    >
                        <option value="">{{ t("NoDefault") }}</option>
                        <option value="true">{{ t("True") }}</option>
                        <option value="false">{{ t("False") }}</option>
                    </select>
                    <textarea
                        v-else-if="editor(row) === 'json'"
                        v-model="row.defaultText"
                        class="form-control form-control-sm font-monospace"
                        rows="3"
                        spellcheck="false"
                        :placeholder="t('VariableDefault')"
                        :aria-label="t('VariableDefault')"
                        data-cy="variable-default"
                        @input="touch"
                        @change="save"
                    ></textarea>
                    <input
                        v-else
                        v-model="row.defaultText"
                        :type="editor(row) === 'number' ? 'number' : editor(row) === 'datetime' ? 'datetime-local' : 'text'"
                        :step="editor(row) === 'number' ? 'any' : undefined"
                        class="form-control form-control-sm"
                        :placeholder="t('VariableDefault')"
                        :aria-label="t('VariableDefault')"
                        data-cy="variable-default"
                        @input="touch"
                        @change="save"
                    />

                    <input
                        v-model="row.description"
                        type="text"
                        class="form-control form-control-sm"
                        :placeholder="t('VariableDescription')"
                        :aria-label="t('VariableDescription')"
                        data-cy="variable-description"
                        @input="touch"
                        @change="save"
                    />

                    <div v-if="row.error" :id="`${ids}-error-${row.key}`" class="invalid-feedback d-block" role="alert" data-cy="variable-error">{{ row.error }}</div>
                </li>
            </ul>
            <div class="wfd-variables-actions">
                <button type="button" class="btn btn-sm btn-outline-primary" data-cy="variable-add" @click="add">
                    <i class="fa-solid fa-plus" aria-hidden="true"></i>
                    {{ t("AddVariable") }}
                </button>
            </div>
        </template>
    </div>
</template>
