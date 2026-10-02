<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from "vue";
import type { DesignerConfig } from "./config";
import { DesignerApiError, type DesignerApi } from "./api/designerApi";
import type { DesignIssue, Library } from "./api/types";
import { designerStore, type DesignerStore } from "./state/designerStore";
import { addNodeCommand } from "./state/commands";
import DesignerCanvas from "./canvas/DesignerCanvas.vue";
import ActivityToolbox from "./toolbox/ActivityToolbox.vue";
import PropertiesPanel from "./panel/PropertiesPanel.vue";
import ToastHost from "./ui/ToastHost.vue";
import SaveStatusIndicator from "./draft/SaveStatusIndicator.vue";
import ConflictDialog from "./draft/ConflictDialog.vue";
import PublishDialog from "./draft/PublishDialog.vue";
import DraftBanner from "./draft/DraftBanner.vue";
import { decidePublish, type PublishDecision } from "./draft/publishDecision";
import { createRevisionQueue } from "./services/revisionQueue";
import { createAutosave } from "./services/autosave";
import { showToast } from "./ui/toasts";
import { confirmAction } from "./ui/confirm";
import { selectNode } from "./canvas/useConnect";
import { DEFAULT_NODE_HEIGHT, NODE_WIDTH, snap, type Point } from "./canvas/geometry";
import { t } from "./i18n";

const props = withDefaults(defineProps<{ config: DesignerConfig; api: DesignerApi; store?: DesignerStore }>(), {
    store: () => designerStore,
});

const state = props.store.state;
const loading = ref(true);
const loadError = ref<string | null>(null);
const library = ref<Library | null>(null);
const libraryLoading = ref(false);
const libraryError = ref<string | null>(null);
const canvas = ref<InstanceType<typeof DesignerCanvas> | null>(null);
const panel = ref<InstanceType<typeof PropertiesPanel> | null>(null);
const conflictOpen = ref(false);
const publishDialog = ref<Exclude<PublishDecision, { kind: "publish" }> | null>(null);
const busy = ref(false);
const bannerDismissed = ref(false);

// Every request that changes the draft goes through this queue, so each one has the current revision.
const queue = createRevisionQueue(props.store);
const mutate = queue.run;

// Overwriting saves this graph over the draft; the result is then loaded, since the draft keeps the
// activities the other person added.
let reloadAfterSave = false;

const autosave = createAutosave({
    store: props.store,
    queue,
    save: (payload) => props.api.save(payload),
    onConflict: () => {
        conflictOpen.value = true;
    },
    onSaved: () => {
        if (reloadAfterSave) {
            reloadAfterSave = false;
            void (async () => {
                await panel.value?.settle();
                await reloadDefinition();
            })();
        }
    },
});

const hasChanges = computed(() => state.hasDraft || state.saveStatus !== "saved");
const canPublish = computed(() => hasChanges.value && !busy.value && state.saveStatus !== "conflict");
const showBanner = computed(
    () =>
        !bannerDismissed.value &&
        state.hasDraft &&
        !!state.draftModifiedByUserId &&
        state.draftModifiedByUserId !== (props.config.currentUserId ?? null),
);

// Editing an activity (Enter or double-click on it) moves the focus to its first field; Escape in the
// panel brings it back.
const editActivity = (activityId: string) => {
    void panel.value?.open(activityId, { focus: true });
};

const returnFocus = (activityId: string) => {
    canvas.value?.focusNode(activityId);
};

// After a dialog closes on a control that is now disabled (Publish) or an element that was replaced
// (Reload), the focus would be lost; it goes to the canvas instead.
const keepFocus = async () => {
    await nextTick();

    if (!document.activeElement || document.activeElement === document.body) {
        canvas.value?.focus();
    }
};

const focusActivity = (activityId: string) => {
    canvas.value?.centerOn(activityId);
    canvas.value?.focusNode(activityId);
};

const onRequestError = (error: unknown, message: string) => {
    if (error instanceof DesignerApiError && error.isConflict) {
        autosave.reportConflict(error);
    } else {
        showToast({ message, variant: "danger" });
    }
};

/**
 * Loads the definition again, dropping the local changes.
 */
const reloadDefinition = () =>
    mutate(async () => {
        reloadAfterSave = false;

        const definition = await props.api.getDefinition();

        panel.value?.discardChanges();
        props.store.loadDefinition(definition);
        autosave.reset();
        bannerDismissed.value = false;
        await panel.value?.refresh();
    });

const onReload = async () => {
    conflictOpen.value = false;

    try {
        await reloadDefinition();
        showToast({ message: t("Reloaded"), variant: "info" });
    } catch {
        showToast({ message: t("ReloadFailed"), variant: "danger" });
    }

    await keepFocus();
};

const onOverwrite = async () => {
    conflictOpen.value = false;
    reloadAfterSave = true;
    await autosave.overwrite();
};

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

// Leaving while a change isn't saved asks first (the browser shows its own message).
const onBeforeUnload = (event: BeforeUnloadEvent) => {
    if (!props.config.readOnly && (autosave.hasUnsavedChanges() || panel.value?.hasPendingChanges())) {
        event.preventDefault();
        event.returnValue = "";
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
        const result = await mutate((revision) => props.api.addActivity(revision, activityName, snap(position.x), snap(position.y)));

        props.store.execute(addNodeCommand(props.store.graph, result.node));
        props.store.registerServerNode(result.node, result.revision, result.issues);
        selectNode(props.store, result.node.id);

        await nextTick();
        canvas.value?.focusNode(result.node.id);

        if (result.node.hasEditor) {
            void panel.value?.open(result.node.id);
        }
    } catch (error) {
        onRequestError(error, t("AddActivityFailed"));
    }
};

// A dropped card is placed under the pointer, its header centered on it.
const onDropActivity = (activityName: string, point: Point) => addActivity(activityName, { x: point.x - NODE_WIDTH / 2, y: point.y - 16 });

// A clicked card is placed in the middle of the visible part of the canvas.
const onAddActivity = (activityName: string) => {
    const center = canvas.value?.visibleCenter() ?? { x: NODE_WIDTH, y: DEFAULT_NODE_HEIGHT };

    return addActivity(activityName, { x: center.x - NODE_WIDTH / 2, y: center.y - DEFAULT_NODE_HEIGHT / 2 });
};

const doPublish = async () => {
    publishDialog.value = null;
    busy.value = true;

    try {
        await mutate(async (revision) => {
            const result = await props.api.publish(revision);

            props.store.markPublished(result.issues);
        });

        showToast({ message: t("Published"), variant: "success" });
        await keepFocus();
    } catch (error) {
        if (error instanceof DesignerApiError && error.status === 400 && error.problem.issues) {
            // The draft has errors the designer didn't know about yet.
            state.issues = error.problem.issues;
            publishDialog.value = { kind: "blocked", errors: error.problem.issues.filter((issue) => issue.severity === "Error") };
        } else {
            onRequestError(error, t("PublishFailed"));
        }
    } finally {
        busy.value = false;
    }
};

/**
 * Applies the open form, saves the pending changes, then publishes: right away, or after a dialog when
 * the draft has errors (which block it), warnings, or running instances.
 */
const publish = async () => {
    if (!canPublish.value) {
        return;
    }

    busy.value = true;

    try {
        if (!(await (panel.value?.settle() ?? true))) {
            return;
        }

        if (!(await autosave.flush())) {
            if (state.saveStatus !== "conflict") {
                showToast({ message: t("SaveBeforePublishFailed"), variant: "danger" });
            }

            return;
        }
    } finally {
        busy.value = false;
    }

    const decision = decidePublish(state.issues, state.runningInstanceCount);

    if (decision.kind === "publish") {
        await doPublish();
    } else {
        publishDialog.value = decision;
    }
};

const onPublishIssueSelected = (issue: DesignIssue) => {
    publishDialog.value = null;

    if (issue.activityId) {
        selectNode(props.store, issue.activityId);
        focusActivity(issue.activityId);
    }
};

const discard = async () => {
    const confirmed = await confirmAction({
        title: t("DiscardDraftTitle"),
        message: t("DiscardDraftMessage"),
        okText: t("DiscardDraft"),
        cancelText: t("Cancel"),
    });

    if (!confirmed) {
        return;
    }

    busy.value = true;
    autosave.pause();

    try {
        await mutate(() => props.api.discard());
        await reloadDefinition();
        showToast({ message: t("DraftDiscarded"), variant: "info" });
        await keepFocus();
    } catch (error) {
        onRequestError(error, t("DiscardFailed"));
    } finally {
        autosave.resume();
        busy.value = false;
    }
};

onMounted(async () => {
    document.addEventListener("keydown", onKeyDown);
    window.addEventListener("beforeunload", onBeforeUnload);
    state.currentUserId = props.config.currentUserId ?? null;

    if (!props.config.readOnly) {
        void loadLibrary();
    }

    try {
        props.store.loadDefinition(await props.api.getDefinition());

        if (!props.config.readOnly) {
            autosave.start();
        }
    } catch {
        loadError.value = t("LoadFailed");
    } finally {
        loading.value = false;
    }

    // The viewer shows the whole workflow, which is in a smaller frame than the designer.
    if (props.config.readOnly && !loadError.value) {
        await nextTick();
        canvas.value?.fit();
    }

    // An activity to open, for example from an old activity edit URL.
    const initialActivityId = props.config.initialActivityId;

    if (!props.config.readOnly && initialActivityId && props.store.getNode(initialActivityId)) {
        await nextTick();
        canvas.value?.centerOn(initialActivityId);
        await panel.value?.open(initialActivityId);
    }
});

onBeforeUnmount(() => {
    document.removeEventListener("keydown", onKeyDown);
    window.removeEventListener("beforeunload", onBeforeUnload);
    autosave.stop();
});

defineExpose({ canvas, panel, addActivity, autosave, publish, discard });
</script>

<template>
    <div class="wfd" :class="{ 'wfd-readonly': config.readOnly }" data-cy="workflow-designer">
        <header class="wfd-toolbar" data-cy="designer-toolbar">
            <h2 class="wfd-title text-truncate">{{ state.settings?.name }}</h2>

            <span v-if="config.readOnly && state.instance" class="wfd-legend small text-body-secondary" data-cy="viewer-legend">
                <span class="badge text-bg-info">
                    <i class="fa-solid fa-hourglass-half" aria-hidden="true"></i>
                    {{ t("Blocking") }}
                </span>
                {{ t("BlockingLegend") }}
            </span>

            <template v-if="!config.readOnly && !loading && !loadError">
                <SaveStatusIndicator :status="state.saveStatus" @retry="autosave.save()" @resolve="conflictOpen = true" />

                <div class="btn-group btn-group-sm" role="group" :aria-label="t('History')">
                    <button
                        type="button"
                        class="btn btn-outline-secondary"
                        :title="t('UndoShortcut')"
                        :aria-label="t('Undo')"
                        :disabled="!state.canUndo"
                        data-cy="toolbar-undo"
                        @click="store.undo()"
                    >
                        <i class="fa-solid fa-rotate-left wfd-mirror-rtl" aria-hidden="true"></i>
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
                        <i class="fa-solid fa-rotate-right wfd-mirror-rtl" aria-hidden="true"></i>
                    </button>
                </div>

                <div class="btn-group btn-group-sm" role="group" :aria-label="t('MoreActions')">
                    <a :href="config.instancesUrl" class="btn btn-outline-secondary" data-cy="toolbar-instances">
                        {{ t("Instances") }}
                        <span v-if="state.runningInstanceCount > 0" class="badge text-bg-secondary ms-1" :title="t('RunningInstanceCount', state.runningInstanceCount)">
                            {{ state.runningInstanceCount }}
                        </span>
                    </a>
                    <a :href="config.exportUrl" class="btn btn-outline-secondary" data-url-af="UnsafeUrl" :title="t('ExportHint')" data-cy="toolbar-export">
                        {{ t("Export") }}
                    </a>
                </div>

                <button type="button" class="btn btn-sm btn-outline-danger" :disabled="!hasChanges || busy" data-cy="toolbar-discard" @click="discard">
                    {{ t("DiscardDraft") }}
                </button>
                <button type="button" class="btn btn-sm btn-primary" :disabled="!canPublish" data-cy="toolbar-publish" @click="publish">
                    <span v-if="busy" class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                    {{ t("Publish") }}
                </button>
            </template>
        </header>

        <DraftBanner v-if="showBanner" :modified-by="state.draftModifiedBy" :modified-utc="state.draftModifiedUtc" @dismiss="bannerDismissed = true" />

        <div class="wfd-body">
            <aside v-if="!config.readOnly" class="wfd-toolbox" :aria-label="t('Toolbox')" data-cy="designer-toolbox">
                <ActivityToolbox :library="library" :loading="libraryLoading" :error="libraryError" @add="onAddActivity" />
            </aside>

            <section class="wfd-canvas-host" :aria-label="t('Canvas')" :aria-busy="loading" data-cy="designer-canvas">
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
                    @edit="editActivity"
                    @drop-activity="onDropActivity"
                />
            </section>

            <PropertiesPanel
                v-if="!loading && !loadError"
                ref="panel"
                :store="store"
                :api="api"
                :read-only="config.readOnly"
                :mutate="mutate"
                @focus-activity="focusActivity"
                @return-focus="returnFocus"
                @conflict="autosave.reportConflict"
            />
        </div>

        <ConflictDialog v-if="conflictOpen && autosave.conflict.value" :problem="autosave.conflict.value" @reload="onReload" @overwrite="onOverwrite" @close="conflictOpen = false" />
        <PublishDialog v-if="publishDialog" :decision="publishDialog" :nodes="state.nodes" @publish="doPublish" @close="publishDialog = null" @select-issue="onPublishIssueSelected" />

        <ToastHost />
    </div>
</template>
