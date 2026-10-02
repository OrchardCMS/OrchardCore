<script setup lang="ts">
import { nextTick, onBeforeUnmount, onMounted, ref } from "vue";
import type { DesignerConfig } from "./config";
import type { DesignerApi } from "./api/designerApi";
import type { Library } from "./api/types";
import { designerStore, type DesignerStore } from "./state/designerStore";
import { addNodeCommand } from "./state/commands";
import DesignerCanvas from "./canvas/DesignerCanvas.vue";
import ActivityToolbox from "./toolbox/ActivityToolbox.vue";
import ToastHost from "./ui/ToastHost.vue";
import { showToast } from "./ui/toasts";
import { selectNode } from "./canvas/useConnect";
import { DEFAULT_NODE_HEIGHT, NODE_WIDTH, snap, type Point } from "./canvas/geometry";
import { t } from "./i18n";

const props = withDefaults(defineProps<{ config: DesignerConfig; api: DesignerApi; store?: DesignerStore }>(), {
    store: () => designerStore,
});

const emit = defineEmits<{ (event: "edit", activityId: string): void }>();

const state = props.store.state;
const loading = ref(true);
const loadError = ref<string | null>(null);
const library = ref<Library | null>(null);
const libraryLoading = ref(false);
const libraryError = ref<string | null>(null);
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

const loadLibrary = async () => {
    libraryLoading.value = true;

    try {
        library.value = await props.api.getLibrary();
    } catch {
        libraryError.value = t("LibraryLoadFailed");
    } finally {
        libraryLoading.value = false;
    }
};

/**
 * Adds an activity with its top-left corner at `position` (canvas coordinates, snapped to the grid). The
 * activity is created on the server first, so it has an id and properties; the canvas change is undoable.
 */
const addActivity = async (activityName: string, position: Point) => {
    try {
        const result = await props.api.addActivity(state.revision, activityName, snap(position.x), snap(position.y));

        props.store.execute(addNodeCommand(props.store.graph, result.node));
        props.store.registerServerNode(result.node, result.revision, result.issues);
        selectNode(props.store, result.node.id);

        await nextTick();
        canvas.value?.focusNode(result.node.id);

        if (result.node.hasEditor) {
            emit("edit", result.node.id);
        }
    } catch {
        showToast({ message: t("AddActivityFailed"), variant: "danger" });
    }
};

// A dropped card is placed under the pointer, its header centered on it.
const onDropActivity = (activityName: string, point: Point) => addActivity(activityName, { x: point.x - NODE_WIDTH / 2, y: point.y - 16 });

// A clicked card is placed in the middle of the visible part of the canvas.
const onAddActivity = (activityName: string) => {
    const center = canvas.value?.visibleCenter() ?? { x: NODE_WIDTH, y: DEFAULT_NODE_HEIGHT };

    return addActivity(activityName, { x: center.x - NODE_WIDTH / 2, y: center.y - DEFAULT_NODE_HEIGHT / 2 });
};

onMounted(async () => {
    document.addEventListener("keydown", onKeyDown);

    if (!props.config.readOnly) {
        void loadLibrary();
    }

    try {
        props.store.loadDefinition(await props.api.getDefinition());
    } catch {
        loadError.value = t("LoadFailed");
    } finally {
        loading.value = false;
    }
});

onBeforeUnmount(() => document.removeEventListener("keydown", onKeyDown));

defineExpose({ canvas, addActivity });
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
            <aside v-if="!config.readOnly" class="wfd-toolbox" :aria-label="t('Toolbox')" data-cy="designer-toolbox">
                <ActivityToolbox :library="library" :loading="libraryLoading" :error="libraryError" @add="onAddActivity" />
            </aside>

            <main class="wfd-canvas-host" :aria-label="t('Canvas')" :aria-busy="loading" data-cy="designer-canvas">
                <div v-if="loading" class="wfd-message">
                    <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                    {{ t("Loading") }}
                </div>
                <div v-else-if="loadError" class="wfd-message text-danger" role="alert">{{ loadError }}</div>
                <DesignerCanvas
                    v-else
                    ref="canvas"
                    :store="store"
                    :read-only="config.readOnly"
                    @edit="emit('edit', $event)"
                    @drop-activity="onDropActivity"
                />
            </main>

            <aside class="wfd-panel" :aria-label="t('Properties')" data-cy="designer-panel"></aside>
        </div>

        <ToastHost />
    </div>
</template>
