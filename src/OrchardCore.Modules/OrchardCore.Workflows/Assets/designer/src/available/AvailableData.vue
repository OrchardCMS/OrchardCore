<script setup lang="ts">
import { computed } from "vue";
import type { DesignerNode } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { typeDisplayName } from "../variables/variableValues";
import { availableData, type AvailableSource, type AvailableValue } from "./availableData";
import AvailableValueItem from "./AvailableValueItem.vue";
import { t } from "../i18n";

// The data the edited activity can use, with the JavaScript and Liquid expressions that read each value. Clicking
// an expression asks the panel to insert it in the activity's editor.
const props = defineProps<{ node: DesignerNode; store: DesignerStore }>();

const emit = defineEmits<{ (event: "insert", text: string): void }>();

const state = props.store.state;

const groups = computed(() => {
    const data = availableData(
        state.nodes,
        state.transitions,
        state.variables,
        props.node.id,
        {
            lastResult: t("LastResult"),
            lastResultDescription: t("LastResultHint"),
            lastResultFrom: (activity, description) => t("LastResultFrom", activity, description),
            correlationId: t("CorrelationId"),
            correlationIdDescription: t("CorrelationIdHint"),
            inputDescription: t("InputHint"),
        },
        state.globalValues,
    );

    return [
        { key: "variables", title: t("VariablesTab"), values: data.variables, empty: t("NoVariables") },
        ...(data.self.length > 0 ? [{ key: "self", title: t("AvailableThisActivity"), values: data.self, empty: "" }] : []),
        ...data.activities.map((activity) => ({ key: activity.activityId, title: t("AvailableFrom", activity.title), values: activity.values, empty: "" })),
        { key: "workflow", title: t("AvailableWorkflow"), values: data.workflow, empty: "" },
        ...(data.inputs.length > 0 ? [{ key: "inputs", title: t("AvailableInputs"), values: data.inputs, empty: "" }] : []),
        ...(data.global.length > 0 ? [{ key: "global", title: t("AvailableGlobal"), values: data.global, empty: "" }] : []),
        ...(data.functions.length > 0 ? [{ key: "functions", title: t("AvailableFunctions"), values: data.functions, empty: "" }] : []),
    ];
});

const sourceLabel = (source: AvailableSource) =>
    source === "Input" ? t("SourceInput") : source === "Output" ? t("SourceOutput") : source === "Properties" ? t("SourceProperty") : null;

// "Content item · input"
const detailOf = (value: AvailableValue) => {
    const source = sourceLabel(value.source);
    const type = typeDisplayName(state.variableTypes, value.typeName);

    return source ? `${type} · ${source}` : type;
};
</script>

<template>
    <section class="wfd-available" :aria-label="t('AvailableData')" data-cy="available-data">
        <p class="wfd-section-hint">{{ t("AvailableDataHint") }}</p>

        <template v-for="group in groups" :key="group.key">
            <h5 class="wfd-available-group" :data-cy="`available-group-${group.key}`">{{ group.title }}</h5>
            <p v-if="group.values.length === 0" class="wfd-section-hint">{{ group.empty }}</p>
            <ul class="list-unstyled wfd-available-list">
                <AvailableValueItem
                    v-for="value in group.values"
                    :key="value.key"
                    :value="value"
                    :detail="detailOf"
                    :data-key="`available-${group.key}-${value.key}`"
                    @insert="emit('insert', $event)"
                />
            </ul>
        </template>

        <p class="wfd-section-hint" data-cy="available-filters">
            {{ t("AvailableFilters") }}
            <a href="https://docs.orchardcore.net/en/latest/reference/modules/Liquid/" target="_blank" rel="noopener noreferrer">{{ t("LiquidDocumentation") }}</a>
        </p>
    </section>
</template>
