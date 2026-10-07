<script setup lang="ts">
import { computed, ref, useId } from "vue";
import type { DesignerNode } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { typeDisplayName } from "../variables/variableValues";
import { readPreference, writePreference } from "../ui/preferences";
import { availableData, type AvailableSource, type AvailableValue } from "./availableData";
import { t } from "../i18n";

// The data the edited activity can use, with the JavaScript and Liquid expressions that read each value. Clicking
// an expression asks the panel to insert it in the activity's editor.
const props = defineProps<{ node: DesignerNode; store: DesignerStore }>();

const emit = defineEmits<{ (event: "insert", text: string): void }>();

const EXPANDED_KEY = "wfd-available-data-expanded";

const state = props.store.state;
const ids = `wfd-available-${useId()}`;
const expanded = ref(readPreference(EXPANDED_KEY) !== "false");

const data = computed(() =>
    availableData(state.nodes, state.transitions, state.variables, props.node.id, {
        lastResult: t("LastResult"),
        lastResultDescription: t("LastResultHint"),
        correlationId: t("CorrelationId"),
        correlationIdDescription: t("CorrelationIdHint"),
    }),
);

const toggle = () => {
    expanded.value = !expanded.value;
    writePreference(EXPANDED_KEY, String(expanded.value));
};

const sourceLabel = (source: AvailableSource) =>
    source === "Input" ? t("SourceInput") : source === "Output" ? t("SourceOutput") : source === "Properties" ? t("SourceProperty") : null;

const detailOf = (value: AvailableValue) => {
    const source = sourceLabel(value.source);
    const type = typeDisplayName(state.variableTypes, value.typeName);

    return source ? `${type}, ${source}` : type;
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

            <template v-for="group in [
                { key: 'variables', title: t('VariablesTab'), values: data.variables, empty: t('NoVariables') },
                ...data.activities.map((activity) => ({ key: activity.activityId, title: t('AvailableFrom', activity.title), values: activity.values, empty: '' })),
                { key: 'workflow', title: t('AvailableWorkflow'), values: data.workflow, empty: '' },
            ]" :key="group.key">
                <h5 class="wfd-available-group" :data-cy="`available-group-${group.key}`">{{ group.title }}</h5>
                <p v-if="group.values.length === 0" class="wfd-section-hint">{{ group.empty }}</p>
                <ul class="list-unstyled wfd-available-list">
                    <li v-for="value in group.values" :key="value.key" class="wfd-available-value" :data-cy="`available-${group.key}-${value.key}`">
                        <div class="wfd-available-name">
                            <code>{{ value.name }}</code>
                            <span class="text-secondary">({{ detailOf(value) }})</span>
                        </div>
                        <div v-if="value.description" class="wfd-available-description">{{ value.description }}</div>
                        <div class="wfd-available-expressions">
                            <button
                                type="button"
                                class="wfd-snippet"
                                :title="t('InsertExpression', value.javaScript)"
                                data-cy="available-javascript"
                                @mousedown.prevent
                                @click="emit('insert', value.javaScript)"
                            >
                                <span class="wfd-snippet-syntax">JS</span><code>{{ value.javaScript }}</code>
                            </button>
                            <button
                                type="button"
                                class="wfd-snippet"
                                :title="t('InsertExpression', value.liquid)"
                                data-cy="available-liquid"
                                @mousedown.prevent
                                @click="emit('insert', value.liquid)"
                            >
                                <span class="wfd-snippet-syntax">Liquid</span><code>{{ value.liquid }}</code>
                            </button>
                        </div>
                    </li>
                </ul>
            </template>
        </div>
    </section>
</template>
