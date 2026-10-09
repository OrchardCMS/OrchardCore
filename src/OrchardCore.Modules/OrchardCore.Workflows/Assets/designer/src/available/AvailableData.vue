<script setup lang="ts">
import { computed, nextTick, ref, useId, watch } from "vue";
import type { DesignerNode } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { typeDisplayName } from "../variables/variableValues";
import { availableData, type AvailableSource, type AvailableValue } from "./availableData";
import AvailableValueItem from "./AvailableValueItem.vue";
import { t } from "../i18n";

// The data the edited activity can use, with the JavaScript and Liquid expressions that read each value. Clicking
// an expression asks the panel to insert it in the activity's editor. Each group (the variables, what an activity
// provides, the global values...) is a tab, so a short panel shows one group at a time.
const props = defineProps<{ node: DesignerNode; store: DesignerStore }>();

const emit = defineEmits<{ (event: "insert", text: string): void }>();

const state = props.store.state;
const ids = `wfd-available-${useId()}`;
const groupList = ref<HTMLElement | null>(null);

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

// The first group that has values is shown first: the variables, or the data of the nearest activity.
const firstGroup = () => (groups.value.find((group) => group.values.length > 0) ?? groups.value[0])?.key ?? null;

const selected = ref<string | null>(firstGroup());
const selectedGroup = computed(() => groups.value.find((group) => group.key === selected.value) ?? null);

watch(
    () => props.node.id,
    () => (selected.value = firstGroup()),
);

// A group can go away, for example when the activity that provided it is disconnected.
watch(groups, () => {
    if (!selectedGroup.value) {
        selected.value = firstGroup();
    }
});

// The Liquid filters are mentioned with the global values, or the functions when there are none.
const filtersGroup = computed(() => (groups.value.some((group) => group.key === "global") ? "global" : "functions"));

// The groups follow the ARIA tabs pattern, vertically: one tab stop, the up and down arrows, Home and End.
const onGroupKeyDown = async (event: KeyboardEvent) => {
    const keys = groups.value.map((group) => group.key);
    const index = keys.indexOf(selected.value ?? "");
    let next: number;

    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
        next = index + (event.key === "ArrowDown" ? 1 : -1);
    } else if (event.key === "Home") {
        next = 0;
    } else if (event.key === "End") {
        next = keys.length - 1;
    } else {
        return;
    }

    event.preventDefault();
    selected.value = keys[(next + keys.length) % keys.length];
    await nextTick();
    groupList.value?.querySelector<HTMLElement>(`[data-cy="available-group-${selected.value}"]`)?.focus();
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
    <section class="wfd-available" :aria-label="t('AvailableData')" data-cy="available-data">
        <p class="wfd-section-hint">{{ t("AvailableDataHint") }}</p>

        <div class="wfd-available-layout">
            <div ref="groupList" class="wfd-available-tabs" role="tablist" aria-orientation="vertical" :aria-label="t('AvailableData')" @keydown="onGroupKeyDown">
                <button
                    v-for="group in groups"
                    :id="`${ids}-${group.key}`"
                    :key="group.key"
                    type="button"
                    role="tab"
                    class="wfd-available-tab"
                    :class="{ active: group.key === selected }"
                    :aria-selected="group.key === selected"
                    :aria-controls="`${ids}-${group.key}-panel`"
                    :tabindex="group.key === selected ? 0 : -1"
                    :title="group.title"
                    :data-cy="`available-group-${group.key}`"
                    @click="selected = group.key"
                >
                    <span class="wfd-available-tab-title">{{ group.title }}</span>
                    <span class="badge rounded-pill wfd-available-tab-count">{{ group.values.length }}</span>
                </button>
            </div>

            <!-- Hidden rather than removed, so the fields a user opened stay open. -->
            <div
                v-for="group in groups"
                v-show="group.key === selected"
                :id="`${ids}-${group.key}-panel`"
                :key="group.key"
                class="wfd-available-values"
                role="tabpanel"
                :aria-labelledby="`${ids}-${group.key}`"
            >
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
                <p v-if="group.key === filtersGroup" class="wfd-section-hint" data-cy="available-filters">
                    {{ t("AvailableFilters") }}
                    <a href="https://docs.orchardcore.net/en/latest/reference/modules/Liquid/" target="_blank" rel="noopener noreferrer">{{ t("LiquidDocumentation") }}</a>
                </p>
            </div>
        </div>
    </section>
</template>
