<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from "vue";
import type { DesignerConfig } from "./config";
import type { DesignerApi } from "./api/designerApi";
import { designerStore, type DesignerStore } from "./state/designerStore";
import DesignerCanvas from "./canvas/DesignerCanvas.vue";
import ToastHost from "./ui/ToastHost.vue";
import { t } from "./i18n";

const props = withDefaults(defineProps<{ config: DesignerConfig; api: DesignerApi; store?: DesignerStore }>(), {
    store: () => designerStore,
});

const emit = defineEmits<{ (event: "edit", activityId: string): void }>();

const state = props.store.state;
const loading = ref(true);
const loadError = ref<string | null>(null);
const canvas = ref<InstanceType<typeof DesignerCanvas> | null>(null);

const isEditable = (target: EventTarget | null) =>
    target instanceof HTMLElement && (target.isContentEditable || ["INPUT", "TEXTAREA", "SELECT"].includes(target.tagName));

// Undo and redo work anywhere in the designer except in form fields, where they edit the field.
const onKeyDown = (event: KeyboardEvent) => {
    if (props.config.readOnly || !(event.ctrlKey || event.metaKey) || isEditable(event.target)) {
        return;
    }

    const key = event.key.toLowerCase();

    if (key === "z" && !event.shiftKey) {
        event.preventDefault();
        props.store.undo();
    } else if (key === "y" || (key === "z" && event.shiftKey)) {
        event.preventDefault();
        props.store.redo();
    }
};

onMounted(async () => {
    document.addEventListener("keydown", onKeyDown);

    try {
        props.store.loadDefinition(await props.api.getDefinition());
    } catch {
        loadError.value = t("LoadFailed");
    } finally {
        loading.value = false;
    }
});

onBeforeUnmount(() => document.removeEventListener("keydown", onKeyDown));

defineExpose({ canvas });
</script>

<template>
    <div class="wfd" :class="{ 'wfd-readonly': config.readOnly }" data-cy="workflow-designer">
        <header class="wfd-toolbar" data-cy="designer-toolbar">
            <h2 class="wfd-title text-truncate">{{ state.settings?.name }}</h2>

            <div v-if="!config.readOnly" class="btn-group btn-group-sm" role="group" :aria-label="t('History')">
                <button
                    type="button"
                    class="btn btn-outline-secondary"
                    :title="t('UndoShortcut')"
                    :aria-label="t('Undo')"
                    :disabled="!state.canUndo"
                    data-cy="toolbar-undo"
                    @click="store.undo()"
                >
                    <i class="fa-solid fa-rotate-left" aria-hidden="true"></i>
                </button>
                <button
                    type="button"
                    class="btn btn-outline-secondary"
                    :title="t('RedoShortcut')"
                    :aria-label="t('Redo')"
                    :disabled="!state.canRedo"
                    data-cy="toolbar-redo"
                    @click="store.redo()"
                >
                    <i class="fa-solid fa-rotate-right" aria-hidden="true"></i>
                </button>
            </div>
        </header>

        <div class="wfd-body">
            <aside v-if="!config.readOnly" class="wfd-toolbox" :aria-label="t('Toolbox')" data-cy="designer-toolbox"></aside>

            <main class="wfd-canvas-host" :aria-label="t('Canvas')" :aria-busy="loading" data-cy="designer-canvas">
                <div v-if="loading" class="wfd-message">
                    <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                    {{ t("Loading") }}
                </div>
                <div v-else-if="loadError" class="wfd-message text-danger" role="alert">{{ loadError }}</div>
                <DesignerCanvas v-else ref="canvas" :store="store" :read-only="config.readOnly" @edit="emit('edit', $event)" />
            </main>

            <aside class="wfd-panel" :aria-label="t('Properties')" data-cy="designer-panel"></aside>
        </div>

        <ToastHost />
    </div>
</template>
