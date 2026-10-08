<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, useId, watch } from "vue";
import type { Library, LibraryActivity } from "../api/types";
import { toolboxKey } from "../api/presets";
import { filterLibrary } from "../toolbox/filter";
import { t } from "../i18n";

// The activities that can come next after an outcome, searchable: clicking an unconnected outcome port opens it, and
// the activity picked is added after the outcome, connected to it.
const props = defineProps<{
    library: Library | null;
    // The display name of the outcome the activity comes after.
    outcome: string;
    // The position of the menu in the canvas area, in pixels.
    left: number;
    top: number;
}>();

const emit = defineEmits<{
    (event: "pick", key: string): void;
    (event: "cancel"): void;
}>();

const ids = `wfd-quick-add-${useId()}`;
const menu = ref<HTMLElement | null>(null);
const input = ref<HTMLInputElement | null>(null);
const list = ref<HTMLElement | null>(null);
const query = ref("");
const active = ref(0);

// The tasks first, which usually come after an outcome, then the events, which wait for something.
const items = computed<LibraryActivity[]>(() => {
    const activities = filterLibrary(props.library, query.value, "all").flatMap((category) => category.activities);

    return [...activities.filter((activity) => !activity.isEvent), ...activities.filter((activity) => activity.isEvent)];
});

watch(query, () => (active.value = 0));

const pick = (activity: LibraryActivity | undefined) => {
    if (activity) {
        emit("pick", toolboxKey(activity));
    }
};

const move = async (offset: number) => {
    if (items.value.length === 0) {
        return;
    }

    active.value = (active.value + offset + items.value.length) % items.value.length;
    await nextTick();
    list.value?.querySelector(`#${ids}-${active.value}`)?.scrollIntoView({ block: "nearest" });
};

const onKeyDown = (event: KeyboardEvent) => {
    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
        event.preventDefault();
        void move(event.key === "ArrowDown" ? 1 : -1);
    } else if (event.key === "Enter") {
        event.preventDefault();
        pick(items.value[active.value]);
    } else if (event.key === "Escape") {
        event.preventDefault();
        event.stopPropagation();
        emit("cancel");
    }
};

// A click elsewhere closes it.
const onDocumentPointerDown = (event: PointerEvent) => {
    if (!menu.value?.contains(event.target as Node)) {
        emit("cancel");
    }
};

onMounted(async () => {
    document.addEventListener("pointerdown", onDocumentPointerDown, true);
    await nextTick();
    input.value?.focus();
});

onBeforeUnmount(() => document.removeEventListener("pointerdown", onDocumentPointerDown, true));
</script>

<template>
    <div
        ref="menu"
        class="card shadow wfd-quick-add"
        role="dialog"
        :aria-label="t('AddNextActivity', outcome)"
        :style="{ left: `${left}px`, top: `${top}px` }"
        data-cy="quick-add"
        @keydown="onKeyDown"
    >
        <div class="card-body p-2">
            <div class="wfd-quick-add-title">{{ t("AddNextActivity", outcome) }}</div>
            <input
                ref="input"
                v-model="query"
                type="search"
                class="form-control form-control-sm"
                role="combobox"
                aria-autocomplete="list"
                aria-expanded="true"
                :aria-controls="`${ids}-list`"
                :aria-activedescendant="items.length > 0 ? `${ids}-${active}` : undefined"
                :placeholder="t('SearchActivities')"
                :aria-label="t('SearchActivities')"
                data-cy="quick-add-search"
            />
            <ul :id="`${ids}-list`" ref="list" class="list-unstyled wfd-quick-add-list" role="listbox" :aria-label="t('Activities')">
                <li
                    v-for="(activity, index) in items"
                    :id="`${ids}-${index}`"
                    :key="toolboxKey(activity)"
                    role="option"
                    class="wfd-quick-add-item"
                    :class="{ active: index === active }"
                    :aria-selected="index === active"
                    :data-cy="`quick-add-${toolboxKey(activity)}`"
                    @pointerenter="active = index"
                    @click="pick(activity)"
                >
                    <i :class="activity.icon || (activity.isEvent ? 'fa-solid fa-bolt' : 'fa-solid fa-gear')" aria-hidden="true"></i>
                    <span class="wfd-quick-add-name">{{ activity.displayText }}</span>
                    <span class="wfd-quick-add-category">{{ activity.category }}</span>
                </li>
            </ul>
            <p v-if="items.length === 0" class="wfd-section-hint m-0 mt-2" data-cy="quick-add-empty">{{ t("NoActivitiesFound") }}</p>
        </div>
    </div>
</template>
