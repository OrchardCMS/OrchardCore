<script setup lang="ts">
import { computed, useId } from "vue";
import type { DesignerApi } from "../api/designerApi";
import type { ActivityOutput, DesignerNode } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import type { RevisionTask } from "../services/revisionQueue";
import { findVariable, isAssignable, typeDisplayName } from "../variables/variableValues";
import { t } from "../i18n";

const props = withDefaults(
    defineProps<{
        node: DesignerNode;
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
const ids = `wfd-outputs-${useId()}`;

const outputs = computed(() => props.node.outputs ?? []);
const bindings = computed(() => props.node.outputBindings ?? {});

const mutate = <T,>(task: RevisionTask<T>) => (props.mutate ? props.mutate(task) : task(state.revision));

const typeName = (name: string) => typeDisplayName(state.variableTypes, name);

// The declared variables, and the bound name when no variable has exactly that name, so it stays selected.
const options = (output: ActivityOutput) => {
    const bound = bindings.value[output.name];
    const variables = state.variables.map((variable) => ({ name: variable.name, label: `${variable.name} (${typeName(variable.typeName)})` }));

    return bound && !variables.some((variable) => variable.name === bound) ? [...variables, { name: bound, label: bound }] : variables;
};

/**
 * Why the binding of `output` may not work: its variable isn't declared, or the output's values may not
 * convert to the variable's type.
 */
const problemOf = (output: ActivityOutput) => {
    const bound = bindings.value[output.name];

    if (!bound) {
        return null;
    }

    const variable = findVariable(state.variables, bound);

    if (!variable) {
        return t("BoundVariableMissing", bound);
    }

    return isAssignable(output.typeName, variable.typeName) ? null : t("OutputTypeMismatch", typeName(output.typeName), variable.name, typeName(variable.typeName));
};

const bind = async (output: ActivityOutput, variableName: string) => {
    const next = { ...bindings.value };

    if (variableName) {
        next[output.name] = variableName;
    } else {
        delete next[output.name];
    }

    // The request may wait in the queue, so it keeps the activity shown now.
    const activityId = props.node.id;

    try {
        const result = await mutate((revision) => props.api.saveOutputBindings(activityId, revision, next));

        props.store.applyServerChange(result.revision, result.issues);
        props.store.replaceNode(result.node);
    } catch (error) {
        emit("error", error);
    }
};
</script>

<template>
    <section v-if="outputs.length > 0" class="wfd-outputs" :aria-labelledby="`${ids}-title`" data-cy="activity-outputs">
        <h4 :id="`${ids}-title`" class="wfd-section-title">{{ t("Outputs") }}</h4>
        <p class="wfd-section-hint">{{ readOnly ? t("OutputsHintReadOnly") : state.variables.length > 0 ? t("OutputsHint") : t("NoVariablesToBind") }}</p>

        <div v-for="output in outputs" :key="output.name" class="wfd-output" :data-cy="`output-${output.name}`">
            <component :is="readOnly ? 'span' : 'label'" :for="readOnly ? undefined : `${ids}-${output.name}`" class="wfd-output-label">
                {{ output.displayName }}
                <span class="text-secondary">({{ typeName(output.typeName) }})</span>
            </component>
            <span v-if="readOnly" class="wfd-output-binding" data-cy="output-binding">
                <code v-if="bindings[output.name]">{{ bindings[output.name] }}</code>
                <span v-else class="text-secondary">{{ t("NotBound") }}</span>
            </span>
            <select
                v-else
                :id="`${ids}-${output.name}`"
                class="form-select form-select-sm"
                :value="bindings[output.name] ?? ''"
                data-cy="output-binding"
                @change="bind(output, ($event.target as HTMLSelectElement).value)"
            >
                <option value="">{{ t("NotBound") }}</option>
                <option v-for="option in options(output)" :key="option.name" :value="option.name">{{ option.label }}</option>
            </select>
            <p v-if="problemOf(output)" class="wfd-output-problem" data-cy="output-problem">
                <i class="fa-solid fa-triangle-exclamation" aria-hidden="true"></i>
                {{ problemOf(output) }}
            </p>
        </div>
    </section>
</template>
