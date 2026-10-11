<script setup lang="ts">
import { computed, reactive, ref } from "vue";
import type { Library, LibraryStep, StepCategory } from "../api/types";
import { STEP_DRAG_TYPE, filterLibrary, stepIcon } from "./filter";
import { t } from "../i18n";

const props = defineProps<{ library: Library | null; loading: boolean; error: string | null }>();

const emit = defineEmits<{ (event: "add", type: string): void }>();

const query = ref("");
// Every group is open until the user collapses it.
const collapsed = reactive(new Set<StepCategory>());

const categories = computed(() => filterLibrary(props.library, query.value));
const searching = computed(() => query.value.trim().length > 0);

const isExpanded = (category: StepCategory) => searching.value || !collapsed.has(category);

const toggle = (category: StepCategory) => {
    if (collapsed.has(category)) {
        collapsed.delete(category);
    } else {
        collapsed.add(category);
    }
};

const portSummary = (step: LibraryStep) => {
    const describe = (ports: LibraryStep["inputs"]) => ports.map((port) => (port.kind === "Files" ? t("FilesPort", "Files") : t("RecordsPort", "Rows"))).join(", ");

    return [step.inputs.length > 0 ? `${t("Inputs", "In")}: ${describe(step.inputs)}` : null, step.outputs.length > 0 ? `${t("Outputs", "Out")}: ${describe(step.outputs)}` : null]
        .filter(Boolean)
        .join(" · ");
};

const onDragStart = (step: LibraryStep, event: DragEvent) => {
    event.dataTransfer?.setData(STEP_DRAG_TYPE, step.name);
    event.dataTransfer?.setData("text/plain", step.displayText);

    if (event.dataTransfer) {
        event.dataTransfer.effectAllowed = "copy";
    }
};
</script>

<template>
    <div class="dpd-toolbox-inner">
        <div class="dpd-toolbox-filters">
            <input v-model="query" type="search" class="form-control form-control-sm" :placeholder="t('SearchSteps', 'Search the steps')" :aria-label="t('SearchSteps', 'Search the steps')" data-cy="toolbox-search" />
        </div>

        <div v-if="loading" class="dpd-toolbox-message">
            <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
            {{ t("LoadingSteps", "Loading the steps…") }}
        </div>
        <div v-else-if="error" class="dpd-toolbox-message text-danger" role="alert">{{ error }}</div>
        <div v-else-if="categories.length === 0" class="dpd-toolbox-message" data-cy="toolbox-empty">{{ t("NoStepsFound", "No step found.") }}</div>

        <section v-for="category in categories" :key="category.category" class="dpd-toolbox-category" :data-cy="`toolbox-group-${category.category}`">
            <h3 class="dpd-toolbox-category-title">
                <button type="button" class="btn btn-sm w-100 text-start" :aria-expanded="isExpanded(category.category)" :data-cy="`toolbox-category-${category.category}`" @click="toggle(category.category)">
                    <i class="fa-solid fa-fw dpd-mirror-rtl" :class="isExpanded(category.category) ? 'fa-caret-down' : 'fa-caret-right'" aria-hidden="true"></i>
                    {{ category.displayName }}
                    <span class="badge text-bg-secondary ms-1">{{ category.steps.length }}</span>
                </button>
            </h3>
            <ul v-show="isExpanded(category.category)" class="dpd-toolbox-list">
                <li v-for="step in category.steps" :key="step.name">
                    <button
                        type="button"
                        class="dpd-toolbox-card"
                        :class="`is-${step.category.toLowerCase()}`"
                        draggable="true"
                        :title="t('AddStep', 'Add {0}', step.displayText)"
                        :aria-label="t('AddStep', 'Add {0}', step.displayText)"
                        :aria-describedby="`dpd-toolbox-${step.name}-description`"
                        :data-cy="`toolbox-step-${step.name}`"
                        @dragstart="onDragStart(step, $event)"
                        @click="emit('add', step.name)"
                    >
                        <i class="dpd-toolbox-icon fa-fw" :class="stepIcon(step)" aria-hidden="true"></i>
                        <span class="dpd-toolbox-text">
                            <span class="dpd-toolbox-name">{{ step.displayText }}</span>
                            <span :id="`dpd-toolbox-${step.name}-description`" class="dpd-toolbox-description">{{ step.description }}</span>
                            <span class="dpd-toolbox-ports">{{ portSummary(step) }}</span>
                        </span>
                    </button>
                </li>
            </ul>
        </section>
    </div>
</template>
