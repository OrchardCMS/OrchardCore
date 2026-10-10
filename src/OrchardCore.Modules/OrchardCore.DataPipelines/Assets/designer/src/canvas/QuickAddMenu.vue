<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, useId, watch } from "vue";
import type { Library, LibraryStep, PortKind } from "../api/types";
import { compatibleSteps, stepIcon } from "../toolbox/filter";
import { t } from "../i18n";

// The steps that can come after an output, searchable: clicking an unconnected output port opens it, and the step
// picked is added after the output, connected to it by the server. Only the steps whose first input receives the
// output's kind (rows or files) are offered.
const props = defineProps<{
    library: Library | null;
    // The kind of the output, and its display name.
    kind: PortKind;
    port: string;
    // The position of the menu in the canvas area, in pixels.
    left: number;
    top: number;
}>();

const emit = defineEmits<{
    (event: "pick", type: string): void;
    (event: "cancel"): void;
}>();

const ids = `dpd-quick-add-${useId()}`;
const menu = ref<HTMLElement | null>(null);
const input = ref<HTMLInputElement | null>(null);
const list = ref<HTMLElement | null>(null);
const query = ref("");
const active = ref(0);

const items = computed(() => compatibleSteps(props.library, props.kind, query.value));

watch(query, () => (active.value = 0));

const pick = (step: LibraryStep | undefined) => {
    if (step) {
        emit("pick", step.name);
    }
};

const move = async (offset: number) => {
    if (items.value.length === 0) {
        return;
    }

    active.value = (active.value + offset + items.value.length) % items.value.length;
    await nextTick();
    list.value?.querySelector(`#${ids}-${active.value}`)?.scrollIntoView?.({ block: "nearest" });
};

const onKeyDown = (event: KeyboardEvent) => {
    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
        event.preventDefault();
        void move(event.key === "ArrowDown" ? 1 : -1);
    } else if (event.key === "Enter") {
        event.preventDefault();
        pick(items.value[active.value]?.step);
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
        class="card shadow dpd-quick-add"
        role="dialog"
        :aria-label="t('AddStepAfterOutput', 'Add a step after {0}', port)"
        :style="{ left: `${left}px`, top: `${top}px` }"
        data-cy="quick-add"
        @keydown="onKeyDown"
    >
        <div class="card-body p-2">
            <div class="dpd-quick-add-title">
                {{ t("AddStepAfterOutput", "Add a step after {0}", port) }}
                <span class="badge text-bg-light border ms-1">{{ kind === "Files" ? t("FilesPort", "Files") : t("RecordsPort", "Rows") }}</span>
            </div>
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
                :placeholder="t('SearchSteps', 'Search the steps')"
                :aria-label="t('SearchSteps', 'Search the steps')"
                data-cy="quick-add-search"
            />
            <ul :id="`${ids}-list`" ref="list" class="list-unstyled dpd-quick-add-list" role="listbox" :aria-label="t('Steps', 'Steps')">
                <li
                    v-for="(item, index) in items"
                    :id="`${ids}-${index}`"
                    :key="item.step.name"
                    role="option"
                    class="dpd-quick-add-item"
                    :class="{ active: index === active }"
                    :aria-selected="index === active"
                    :title="item.step.description"
                    :data-cy="`quick-add-${item.step.name}`"
                    @pointerenter="active = index"
                    @click="pick(item.step)"
                >
                    <i class="fa-fw" :class="stepIcon(item.step)" aria-hidden="true"></i>
                    <span class="dpd-quick-add-name">{{ item.step.displayText }}</span>
                    <span class="dpd-quick-add-category">{{ item.category.displayName }}</span>
                </li>
            </ul>
            <p v-if="items.length === 0" class="dpd-section-hint m-0 mt-2" data-cy="quick-add-empty">{{ t("NoCompatibleSteps", "No step can receive this output.") }}</p>
        </div>
    </div>
</template>
