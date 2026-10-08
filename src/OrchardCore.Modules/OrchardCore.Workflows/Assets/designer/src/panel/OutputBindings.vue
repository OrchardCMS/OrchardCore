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

// The declared variables an output can be stored in: those of a type its values convert to first, then the others,
// whose type is shown. The bound name stays selected when no variable has exactly that name.
const options = (output: ActivityOutput) => {
    const bound = bindings.value[output.name];
    const compatible = state.variables.filter((variable) => isAssignable(output.typeName, variable.typeName)).map((variable) => variable.name);
    const others = state.variables
        .filter((variable) => !isAssignable(output.typeName, variable.typeName))
        .map((variable) => ({ name: variable.name, label: `${variable.name} (${typeName(variable.typeName)})` }));
    const missing = bound && !state.variables.some((variable) => variable.name === bound) ? [bound] : [];

    return { compatible: [...compatible, ...missing], others };
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
    <!-- Each output of the activity can be stored in a variable of the workflow, which later activities read. -->
    <section v-if="outputs.length > 0" class="wfd-outputs" :aria-label="t('Outputs')" data-cy="activity-outputs">
        <p class="wfd-section-hint">{{ readOnly ? t("OutputsHintReadOnly") : state.variables.length > 0 ? t("OutputsHint") : t("NoVariablesToBind") }}</p>

        <div v-for="output in outputs" :key="output.name" class="wfd-output" :data-cy="`output-${output.name}`">
            <div class="wfd-output-source">
                <div class="wfd-output-head">
                    <span :id="`${ids}-${output.name}-name`" class="wfd-output-name">{{ output.displayName }}</span>
                    <span class="badge wfd-output-type" data-cy="output-type">{{ typeName(output.typeName) }}</span>
                </div>
                <p v-if="output.description" class="wfd-output-description">{{ output.description }}</p>
            </div>

            <i class="fa-solid fa-arrow-right wfd-mirror-rtl wfd-output-arrow" aria-hidden="true"></i>

            <div class="wfd-output-target">
                <span v-if="readOnly" class="wfd-output-binding" data-cy="output-binding">
                    <code v-if="bindings[output.name]">{{ bindings[output.name] }}</code>
                    <span v-else class="text-secondary">{{ t("NotBound") }}</span>
                </span>
                <div v-else class="input-group input-group-sm">
                    <label class="input-group-text" :for="`${ids}-${output.name}`">{{ t("StoreIn") }}</label>
                    <select
                        :id="`${ids}-${output.name}`"
                        class="form-select"
                        :value="bindings[output.name] ?? ''"
                        :aria-describedby="`${ids}-${output.name}-name`"
                        data-cy="output-binding"
                        @change="bind(output, ($event.target as HTMLSelectElement).value)"
                    >
                        <option value="">{{ t("DontStore") }}</option>
                        <option v-for="name in options(output).compatible" :key="name" :value="name">{{ name }}</option>
                        <optgroup v-if="options(output).others.length > 0" :label="t('OtherVariableTypes')">
                            <option v-for="option in options(output).others" :key="option.name" :value="option.name">{{ option.label }}</option>
                        </optgroup>
                    </select>
                </div>
                <p v-if="problemOf(output)" class="wfd-output-problem" data-cy="output-problem">
                    <i class="fa-solid fa-triangle-exclamation" aria-hidden="true"></i>
                    {{ problemOf(output) }}
                </p>
            </div>
        </div>
    </section>
</template>
