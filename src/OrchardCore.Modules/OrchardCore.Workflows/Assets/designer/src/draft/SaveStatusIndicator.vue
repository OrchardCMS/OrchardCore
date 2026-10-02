<script setup lang="ts">
import { computed } from "vue";
import type { SaveStatus } from "../state/designerStore";
import { t } from "../i18n";

const props = defineProps<{ status: SaveStatus }>();

const emit = defineEmits<{
    (event: "retry"): void;
    (event: "resolve"): void;
}>();

const view = computed(() => {
    switch (props.status) {
        case "saving":
            return { icon: "fa-solid fa-rotate fa-spin", text: t("Saving"), tone: "text-body-secondary" };
        case "unsaved":
            return { icon: "fa-solid fa-circle", text: t("UnsavedChanges"), tone: "text-body-secondary" };
        case "offline":
            return { icon: "fa-solid fa-wifi", text: t("SaveRetrying"), tone: "text-warning-emphasis" };
        case "failed":
            return { icon: "fa-solid fa-triangle-exclamation", text: t("SaveFailed"), tone: "text-danger" };
        case "conflict":
            return { icon: "fa-solid fa-code-merge", text: t("SaveConflict"), tone: "text-danger" };
        default:
            return { icon: "fa-solid fa-check", text: t("Saved"), tone: "text-body-secondary" };
    }
});
</script>

<template>
    <span class="wfd-save-status small" :class="view.tone" :data-status="status" data-cy="save-status">
        <span role="status" aria-live="polite">
            <i :class="view.icon" aria-hidden="true"></i>
            {{ view.text }}
        </span>
        <button v-if="status === 'failed'" type="button" class="btn btn-link btn-sm p-0" data-cy="save-retry" @click="emit('retry')">{{ t("Retry") }}</button>
        <button v-else-if="status === 'conflict'" type="button" class="btn btn-link btn-sm p-0" data-cy="save-resolve" @click="emit('resolve')">
            {{ t("ResolveConflict") }}
        </button>
    </span>
</template>
