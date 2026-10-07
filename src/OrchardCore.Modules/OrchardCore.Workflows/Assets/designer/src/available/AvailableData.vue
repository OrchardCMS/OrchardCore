<script setup lang="ts">
import { computed, ref, useId } from "vue";
import type { DesignerNode } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { typeDisplayName } from "../variables/variableValues";
import { readPreference, writePreference } from "../ui/preferences";
import { availableData, type AvailableSource, type AvailableValue } from "./availableData";
import AvailableValueItem from "./AvailableValueItem.vue";
import { t } from "../i18n";

// The data the edited activity can use, with the JavaScript and Liquid expressions that read each value. Clicking
// an expression asks the panel to insert it in the activity's editor.
const props = defineProps<{ node: DesignerNode; store: DesignerStore }>();

const emit = defineEmits<{ (event: "insert", text: string): void }>();

const EXPANDED_KEY = "wfd-available-data-expanded";

const state = props.store.state;
const ids = `wfd-available-${useId()}`;
const expanded = ref(readPreference(EXPANDED_KEY) !== "false");

const groups = computed(() => {
    const data = availableData(state.nodes, state.transitions, state.variables, props.node.id, {
        lastResult: t("LastResult"),
        lastResultDescription: t("LastResultHint"),
        correlationId: t("CorrelationId"),
        correlationIdDescription: t("CorrelationIdHint"),
    });

    return [
        { key: "variables", title: t("VariablesTab"), values: data.variables, empty: t("NoVariables") },
        ...data.activities.map((activity) => ({ key: activity.activityId, title: t("AvailableFrom", activity.title), values: activity.values, empty: "" })),
        { key: "workflow", title: t("AvailableWorkflow"), values: data.workflow, empty: "" },
    ];
});

const toggle = () => {
    expanded.value = !expanded.value;
    writePreference(EXPANDED_KEY, String(expanded.value));
};

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
    <section class="wfd-available" :aria-labelledby="`${ids}-title`" data-cy="available-data">
        <h4 :id="`${ids}-title`" class="wfd-section-title">
            <button type="button" class="wfd-available-toggle" :aria-expanded="expanded" :aria-controls="`${ids}-body`" data-cy="available-data-toggle" @click="toggle">
                <i class="fa-solid fa-fw" :class="expanded ? 'fa-caret-down' : 'fa-caret-right wfd-mirror-rtl'" aria-hidden="true"></i>
                {{ t("AvailableData") }}
            </button>
        </h4>

        <div v-show="expanded" :id="`${ids}-body`">
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
        </div>
    </section>
</template>
