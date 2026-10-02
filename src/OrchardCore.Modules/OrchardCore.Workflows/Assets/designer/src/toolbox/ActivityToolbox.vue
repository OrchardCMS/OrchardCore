<script setup lang="ts">
import { computed, reactive, ref } from "vue";
import type { Library, LibraryActivity } from "../api/types";
import { ACTIVITY_DRAG_TYPE, filterLibrary, type ActivityKind } from "./filter";
import { t } from "../i18n";

const props = defineProps<{ library: Library | null; loading: boolean; error: string | null }>();

const emit = defineEmits<{ (event: "add", activityName: string): void }>();

const query = ref("");
const kind = ref<ActivityKind>("all");
const expanded = reactive(new Set<string>());

const categories = computed(() => filterLibrary(props.library, query.value, kind.value));
const searching = computed(() => query.value.trim().length > 0 || kind.value !== "all");

const isExpanded = (name: string) => searching.value || expanded.has(name);

const toggle = (name: string) => {
    if (expanded.has(name)) {
        expanded.delete(name);
    } else {
        expanded.add(name);
    }
};

const onDragStart = (activity: LibraryActivity, event: DragEvent) => {
    event.dataTransfer?.setData(ACTIVITY_DRAG_TYPE, activity.name);
    event.dataTransfer?.setData("text/plain", activity.displayText);

    if (event.dataTransfer) {
        event.dataTransfer.effectAllowed = "copy";
    }
};

const kinds: { value: ActivityKind; label: string }[] = [
    { value: "all", label: t("All") },
    { value: "events", label: t("Events") },
    { value: "tasks", label: t("Tasks") },
];
</script>

<template>
    <div class="wfd-toolbox-inner">
        <div class="wfd-toolbox-filters">
            <input
                v-model="query"
                type="search"
                class="form-control form-control-sm"
                :placeholder="t('SearchActivities')"
                :aria-label="t('SearchActivities')"
                data-cy="toolbox-search"
            />
            <div class="btn-group btn-group-sm w-100 mt-2" role="group" :aria-label="t('ActivityKind')">
                <template v-for="item in kinds" :key="item.value">
                    <input
                        :id="`wfd-kind-${item.value}`"
                        v-model="kind"
                        type="radio"
                        class="btn-check"
                        name="wfd-toolbox-kind"
                        :value="item.value"
                        :data-cy="`toolbox-filter-${item.value}`"
                    />
                    <label class="btn btn-outline-secondary" :for="`wfd-kind-${item.value}`">{{ item.label }}</label>
                </template>
            </div>
        </div>

        <div v-if="loading" class="wfd-toolbox-message">
            <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
            {{ t("LoadingActivities") }}
        </div>
        <div v-else-if="error" class="wfd-toolbox-message text-danger" role="alert">{{ error }}</div>
        <div v-else-if="categories.length === 0" class="wfd-toolbox-message" data-cy="toolbox-empty">{{ t("NoActivitiesFound") }}</div>

        <section v-for="category in categories" :key="category.name" class="wfd-toolbox-category">
            <h3 class="wfd-toolbox-category-title">
                <button
                    type="button"
                    class="btn btn-sm w-100 text-start"
                    :aria-expanded="isExpanded(category.name)"
                    :data-cy="`toolbox-category-${category.name}`"
                    @click="toggle(category.name)"
                >
                    <i class="fa-solid fa-fw wfd-mirror-rtl" :class="isExpanded(category.name) ? 'fa-caret-down' : 'fa-caret-right'" aria-hidden="true"></i>
                    {{ category.name }}
                    <span class="badge text-bg-secondary ms-1">{{ category.activities.length }}</span>
                </button>
            </h3>
            <ul v-show="isExpanded(category.name)" class="wfd-toolbox-list">
                <li v-for="activity in category.activities" :key="activity.name">
                    <button
                        type="button"
                        class="wfd-toolbox-card"
                        :class="{ 'is-event': activity.isEvent }"
                        draggable="true"
                        :title="t('AddActivity', activity.displayText)"
                        :aria-label="t('AddActivity', activity.displayText)"
                        :data-cy="`toolbox-activity-${activity.name}`"
                        @dragstart="onDragStart(activity, $event)"
                        @click="emit('add', activity.name)"
                    >
                        <i class="wfd-toolbox-icon fa-fw" :class="activity.icon || (activity.isEvent ? 'fa-solid fa-bolt' : 'fa-solid fa-gear')" aria-hidden="true"></i>
                        <span class="wfd-toolbox-thumb" v-html="activity.thumbnailHtml"></span>
                    </button>
                </li>
            </ul>
        </section>
    </div>
</template>
