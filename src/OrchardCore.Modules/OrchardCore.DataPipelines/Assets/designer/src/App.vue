<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from "vue";
import type { DesignerConfig } from "./config";
import { DesignerApiError, type DesignerApi } from "./api/designerApi";
import type { DesignIssue, DesignerVersion, Library, PortKind } from "./api/types";
import { designerStore, type DesignerStore } from "./state/designerStore";
import { addNodeCommand } from "./state/commands";
import { findOutput } from "./state/ports";
import DesignerCanvas from "./canvas/DesignerCanvas.vue";
import StepToolbox from "./toolbox/StepToolbox.vue";
import PipelinePanel from "./panel/PipelinePanel.vue";
import StepPanel from "./panel/StepPanel.vue";
import QuickAddMenu from "./canvas/QuickAddMenu.vue";
import StartPicker from "./canvas/StartPicker.vue";
import ToastHost from "./ui/ToastHost.vue";
import SaveStatusIndicator from "./draft/SaveStatusIndicator.vue";
import ConflictDialog from "./draft/ConflictDialog.vue";
import PublishDialog from "./draft/PublishDialog.vue";
import DraftBanner from "./draft/DraftBanner.vue";
import VersionsDialog from "./draft/VersionsDialog.vue";
import { formatDateTime } from "./draft/formatDateTime";
import { decidePublish, hasErrors, type PublishDecision } from "./draft/publishDecision";
import { createRevisionQueue } from "./services/revisionQueue";
import { createAutosave } from "./services/autosave";
import { createRunTracker } from "./runs/runTracker";
import { showToast } from "./ui/toasts";
import { confirmAction } from "./ui/confirm";
import { usePeek } from "./ui/usePeek";
import { readPreference, writePreference } from "./ui/preferences";
import { selectNode, showStep } from "./canvas/useConnect";
import { DEFAULT_NODE_HEIGHT, NODE_WIDTH, findFreeSpot, nodeRect, snap, type Point } from "./canvas/geometry";
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
// The pipeline's panel (settings, issues, runs), and the selected step's panel below the canvas.
const panel = ref<InstanceType<typeof PipelinePanel> | null>(null);
const stepPanel = ref<InstanceType<typeof StepPanel> | null>(null);
const conflictOpen = ref(false);
const publishDialog = ref<Exclude<PublishDecision, { kind: "publish" }> | null>(null);
const busy = ref(false);
const bannerDismissed = ref(false);
const versionsOpen = ref(false);

// A version page shows a published version: nothing can be edited.
const isVersionPage = computed(() => props.config.mode === "version");
const readOnly = computed(() => props.config.readOnly || isVersionPage.value);

// The toolbox can be collapsed into a rail, to give the canvas more room; hovering the rail opens it over the canvas,
// and it stays open while a step is dragged from it.
const TOOLBOX_COLLAPSED_KEY = "toolbox-collapsed";
const toolbox = ref<HTMLElement | null>(null);
const toolboxCollapsed = ref(readPreference(TOOLBOX_COLLAPSED_KEY) === "true");
const toolboxPeek = usePeek((element) => !!element && !!toolbox.value?.contains(element));
const toolboxPeeking = computed(() => toolboxCollapsed.value && toolboxPeek.open.value);

const setToolboxCollapsed = (value: boolean) => {
    toolboxCollapsed.value = value;
    toolboxPeek.close();
    writePreference(TOOLBOX_COLLAPSED_KEY, String(value));
};

// While a step is dragged from the toolbox, the toolbox stays open and the canvas takes the whole drop.
const draggingStep = ref(false);

const onToolboxDragStart = () => {
    draggingStep.value = true;
    toolboxPeek.hold();
};

const onToolboxDragEnd = () => {
    draggingStep.value = false;
    toolboxPeek.release();
};

const closeToolboxPeek = () => {
    toolboxPeek.close();
    toolbox.value?.querySelector<HTMLElement>("[data-cy=toolbox-rail-steps]")?.focus();
};

/**
 * Applies the pending changes of both panels' forms; resolves to false when the user wants to keep editing.
 */
const settleForms = async () => (await (stepPanel.value?.settle() ?? true)) && (await (panel.value?.settle() ?? true));

// Every request that changes the draft goes through this queue, so each one has the current revision.
const queue = createRevisionQueue(props.store);
const mutate = queue.run;

// Overwriting saves this graph over the draft; the result is then loaded, since the draft keeps what the other person
// changed in the steps' settings.
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
                await settleForms();
                await reloadDefinition();
            })();
        }
    },
});

// The runs of the pipeline, when the user can see them.
const tracker = props.config.urls.runs
    ? createRunTracker({
          store: props.store,
          getRuns: () => props.api.getRuns(),
          getRun: (runId) => props.api.getRun(runId),
          run: props.config.urls.run ? () => props.api.run() : undefined,
          cancelRun: props.config.urls.cancelRun ? (runId) => props.api.cancelRun(runId) : undefined,
      })
    : null;

const hasChanges = computed(() => state.hasDraft || state.saveStatus !== "saved");
const draftHasErrors = computed(() => hasErrors(state.issues));
const canPublish = computed(() => hasChanges.value && !busy.value && state.saveStatus !== "conflict" && !draftHasErrors.value);
const showBanner = computed(
    () => !readOnly.value && !bannerDismissed.value && state.hasDraft && !!state.draftModifiedByUserId && state.draftModifiedByUserId !== (props.config.currentUserId ?? null),
);

// Runs use the published version, so a pipeline that was never published can't run.
const runOffered = computed(() => !isVersionPage.value && !!props.config.urls.run && !!tracker && state.canRun);
const canRunNow = computed(() => runOffered.value && !!state.publishedVersion);
const runDisabledReason = computed(() => (runOffered.value && !state.publishedVersion ? t("RunNeedsPublish", "Publish the pipeline to run it.") : null));
const runStarting = computed(() => tracker?.state.starting ?? false);

const publishTitle = computed(() =>
    draftHasErrors.value ? t("PublishBlockedHint", "Fix the errors listed in the Issues tab to publish.") : t("PublishHint", "Runs use the published version."),
);

// Editing a step (Enter or double-click on it) moves the focus to its first field; Escape in its panel brings it back.
const editStep = (stepId: string) => {
    void stepPanel.value?.open(stepId, { focus: true });
};

const previewStep = (stepId: string) => {
    void stepPanel.value?.preview(stepId);
};

// The step panel opens over the bottom of the canvas: the selected step stays in view above it.
watch(
    () => [stepPanel.value?.coveredHeight ?? 0, state.selectedNodeIds.length === 1 ? state.selectedNodeIds[0] : null] as const,
    ([covered, stepId]) => {
        if (covered > 0 && stepId) {
            canvas.value?.keepVisible(stepId, covered);
        }
    },
);

const returnFocus = (stepId: string) => {
    canvas.value?.focusNode(stepId);
};

// After a dialog closes on a control that is now disabled (Publish) or an element that was replaced (Reload), the
// focus would be lost; it goes to the canvas instead.
const keepFocus = async () => {
    await nextTick();

    if (!document.activeElement || document.activeElement === document.body) {
        canvas.value?.focus();
    }
};

const focusStep = (stepId: string) => {
    canvas.value?.centerOn(stepId);
    canvas.value?.focusNode(stepId);
};

const problemMessage = (error: unknown, fallback: string) => (error instanceof DesignerApiError && error.problem.detail ? error.problem.detail : fallback);

const onRequestError = (error: unknown, message: string) => {
    if (error instanceof DesignerApiError && error.isConflict) {
        autosave.reportConflict(error);
    } else {
        showToast({ message: problemMessage(error, message), variant: "danger" });
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
        stepPanel.value?.discardChanges();
        props.store.loadDefinition(definition);
        autosave.reset();
        bannerDismissed.value = false;
        await Promise.all([panel.value?.refresh(), stepPanel.value?.refresh()]);
    });

const onReload = async () => {
    conflictOpen.value = false;

    try {
        await reloadDefinition();
        showToast({ message: t("Reloaded", "The pipeline was reloaded."), variant: "info" });
    } catch {
        showToast({ message: t("ReloadFailed", "The pipeline could not be reloaded."), variant: "danger" });
    }

    await keepFocus();
};

const onOverwrite = async () => {
    conflictOpen.value = false;
    reloadAfterSave = true;
    await autosave.overwrite();
};

const isEditable = (target: EventTarget | null) => target instanceof HTMLElement && (target.isContentEditable || ["INPUT", "TEXTAREA", "SELECT"].includes(target.tagName));

// Undo and redo work anywhere in the designer except in form fields, where they edit the field.
const onKeyDown = (event: KeyboardEvent) => {
    if (readOnly.value || !(event.ctrlKey || event.metaKey) || isEditable(event.target)) {
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
    if (!readOnly.value && (autosave.hasUnsavedChanges() || panel.value?.hasPendingChanges() || stepPanel.value?.hasPendingChanges())) {
        event.preventDefault();
        event.returnValue = "";
    }
};

const loadLibrary = async () => {
    libraryLoading.value = true;

    try {
        library.value = await props.api.getLibrary();
    } catch {
        libraryError.value = t("LibraryLoadFailed", "The steps could not be loaded.");
    } finally {
        libraryLoading.value = false;
    }
};

/**
 * Adds a step with its top-left corner at `position` (canvas coordinates, snapped to the grid), connected after an
 * output when `connectFrom` is set. The step is created on the server first, so it has an id and settings; the canvas
 * change is undoable.
 */
const addStep = async (type: string, position: Point, connectFrom: { stepId: string; port: string } | null = null): Promise<string | null> => {
    try {
        const result = await mutate((revision) => props.api.addStep({ revision, type, x: snap(position.x), y: snap(position.y), connectFrom }));

        props.store.execute(addNodeCommand(props.store.graph, result.node, result.connection));
        props.store.registerServerNode(result.node, result.revision, result.issues);
        selectNode(props.store, result.node.id);

        await nextTick();
        canvas.value?.focusNode(result.node.id);

        if (result.node.hasEditor) {
            void stepPanel.value?.open(result.node.id);
        }

        canvas.value?.keepVisible(result.node.id, stepPanel.value?.coveredHeight ?? 0);

        return result.node.id;
    } catch (error) {
        onRequestError(error, t("AddStepFailed", "The step could not be added."));

        return null;
    }
};

// Clicking an output that leads nowhere yet offers the steps that can come after it.
const main = ref<HTMLElement | null>(null);
const quickAdd = ref<{ sourceId: string; port: string; portLabel: string; kind: PortKind; left: number; top: number } | null>(null);

const onAddAfter = (sourceId: string, port: string, clientX: number, clientY: number) => {
    const output = findOutput(props.store.getNode(sourceId), port);
    const bounds = main.value?.getBoundingClientRect();

    if (!output || !bounds || readOnly.value) {
        return;
    }

    // Beside the port, within the canvas area.
    const left = Math.max(8, Math.min(clientX - bounds.left + 12, bounds.width - 320));
    const top = Math.max(8, Math.min(clientY - bounds.top - 16, bounds.height - 340));

    quickAdd.value = { sourceId, port, portLabel: output.displayName, kind: output.kind, left, top };
};

/**
 * Adds the step picked after the output: to the right of its step, below the cards in the way, connected to it.
 */
const onQuickAddPick = async (type: string) => {
    const target = quickAdd.value;
    quickAdd.value = null;
    const source = target ? props.store.getNode(target.sourceId) : undefined;

    if (!target || !source) {
        return;
    }

    const sourceRect = nodeRect(source, canvas.value?.layoutOf(source.id));
    const rects = state.nodes.map((node) => nodeRect(node, canvas.value?.layoutOf(node.id)));
    const position = findFreeSpot({ x: sourceRect.x + sourceRect.width + 100, y: sourceRect.y }, { width: NODE_WIDTH, height: DEFAULT_NODE_HEIGHT }, rects);

    await addStep(type, position, { stepId: source.id, port: target.port });
};

const onQuickAddCancel = () => {
    const target = quickAdd.value;
    quickAdd.value = null;

    if (target) {
        canvas.value?.focusNode(target.sourceId);
    }
};

// A dropped card is placed under the pointer, its header centered on it.
const onDropStep = (type: string, point: Point) => addStep(type, { x: point.x - NODE_WIDTH / 2, y: point.y - 16 });

// A clicked card is placed in the middle of the visible part of the canvas.
const onAddStep = (type: string) => {
    const center = canvas.value?.visibleCenter() ?? { x: NODE_WIDTH, y: DEFAULT_NODE_HEIGHT };

    return addStep(type, { x: center.x - NODE_WIDTH / 2, y: center.y - DEFAULT_NODE_HEIGHT / 2 });
};

/**
 * Applies the open forms and saves the pending changes, before a preview or a publish. Resolves to false when that
 * failed or the user wants to keep editing.
 */
const prepare = async () => {
    if (readOnly.value) {
        return true;
    }

    if (!(await settleForms())) {
        return false;
    }

    if (!(await autosave.flush())) {
        if (state.saveStatus !== "conflict") {
            showToast({ message: t("SaveFirstFailed", "The pipeline could not be saved."), variant: "danger" });
        }

        return false;
    }

    return true;
};

const doPublish = async () => {
    publishDialog.value = null;
    busy.value = true;

    try {
        await mutate(async (revision) => {
            const result = await props.api.publish(revision);

            props.store.markPublished(result.issues, result.version ?? null);
        });

        showToast({
            message: state.publishedVersion ? t("PublishedVersion", "Version {0} was published.", state.publishedVersion.number) : t("Published", "The pipeline was published."),
            variant: "success",
        });
        await keepFocus();
    } catch (error) {
        if (error instanceof DesignerApiError && error.status === 400 && error.problem.issues) {
            // The draft has errors the designer didn't know about yet.
            state.issues = error.problem.issues;
            publishDialog.value = { kind: "blocked", errors: error.problem.issues.filter((issue) => issue.severity === "Error") };
        } else {
            onRequestError(error, t("PublishFailed", "The pipeline could not be published."));
        }
    } finally {
        busy.value = false;
    }
};

/**
 * Applies the open form, saves the pending changes, then publishes: right away, or after a dialog when the draft has
 * errors (which block it) or warnings.
 */
const publish = async () => {
    if (!canPublish.value) {
        return;
    }

    busy.value = true;

    try {
        if (!(await prepare())) {
            return;
        }
    } finally {
        busy.value = false;
    }

    const decision = decidePublish(state.issues);

    if (decision.kind === "publish") {
        await doPublish();
    } else {
        publishDialog.value = decision;
    }
};

const onPublishIssueSelected = (issue: DesignIssue) => {
    publishDialog.value = null;

    if (issue.stepId) {
        showStep(props.store, issue.stepId);
        focusStep(issue.stepId);
    }
};

/**
 * Copies a version into the draft (asking first when the draft has changes), then loads the draft.
 */
const restoreVersion = async (version: DesignerVersion) => {
    if (
        hasChanges.value &&
        !(await confirmAction({
            title: t("RestoreVersionTitle", "Restore this version?"),
            message: t("RestoreVersionMessage", "The draft will be replaced with version {0}.", version.number),
            okText: t("Restore", "Restore"),
            cancelText: t("Cancel", "Cancel"),
        }))
    ) {
        return;
    }

    busy.value = true;

    try {
        if (!(await prepare())) {
            return;
        }

        await mutate((revision) => props.api.restore(version.versionId, revision));
        versionsOpen.value = false;
        await reloadDefinition();
        showToast({ message: t("VersionRestored", "Version {0} was copied into the draft.", version.number), variant: "success" });
    } catch (error) {
        onRequestError(error, t("RestoreFailed", "The version could not be restored."));
    } finally {
        busy.value = false;
    }

    await keepFocus();
};

const discard = async () => {
    const confirmed = await confirmAction({
        title: t("DiscardDraftTitle", "Discard the draft?"),
        message: t("DiscardDraftMessage", "The changes made since the pipeline was last published will be lost."),
        okText: t("DiscardDraft", "Discard draft"),
        cancelText: t("Cancel", "Cancel"),
    });

    if (!confirmed) {
        return;
    }

    busy.value = true;
    autosave.pause();

    try {
        await mutate(() => props.api.discard());
        await reloadDefinition();
        showToast({ message: t("DraftDiscarded", "The draft was discarded."), variant: "info" });
        await keepFocus();
    } catch (error) {
        onRequestError(error, t("DiscardFailed", "The draft could not be discarded."));
    } finally {
        autosave.resume();
        busy.value = false;
    }
};

/**
 * Queues a run of the published version and shows it in the Runs tab, and its steps on the canvas.
 */
const startRun = async () => {
    if (!tracker || !canRunNow.value) {
        return;
    }

    try {
        await tracker.start();
        await panel.value?.expand("runs");
        showToast({
            message: state.hasDraft
                ? t("RunQueuedPublished", "The run was queued. It runs the published version, not the draft's changes.")
                : t("RunQueuedMessage", "The run was queued."),
            variant: "success",
        });
    } catch (error) {
        showToast({ message: problemMessage(error, t("RunFailedToStart", "The pipeline could not be run.")), variant: "danger" });
    }
};

onMounted(async () => {
    document.addEventListener("keydown", onKeyDown);
    window.addEventListener("beforeunload", onBeforeUnload);
    state.currentUserId = props.config.currentUserId ?? null;

    if (!readOnly.value) {
        void loadLibrary();
    }

    try {
        props.store.loadDefinition(await props.api.getDefinition());

        if (!readOnly.value) {
            autosave.start();
        }
    } catch {
        loadError.value = t("LoadFailed", "The pipeline could not be loaded.");
    } finally {
        loading.value = false;
    }

    if (loadError.value) {
        return;
    }

    // The read-only view shows the whole pipeline.
    if (readOnly.value) {
        await nextTick();
        canvas.value?.fit();
    }

    // A step to open, for example from a link to one of its issues.
    const initialStepId = props.config.initialStepId;

    if (initialStepId && props.store.getNode(initialStepId)) {
        await nextTick();
        canvas.value?.centerOn(initialStepId);
        await stepPanel.value?.open(initialStepId);
    }
});

onBeforeUnmount(() => {
    document.removeEventListener("keydown", onKeyDown);
    window.removeEventListener("beforeunload", onBeforeUnload);
    autosave.stop();
    tracker?.stop();
});

defineExpose({ canvas, panel, stepPanel, addStep, autosave, publish, discard, startRun, tracker });
</script>

<template>
    <div class="dpd" :class="{ 'dpd-readonly': readOnly }" data-cy="data-pipeline-designer">
        <header class="dpd-toolbar" data-cy="designer-toolbar">
            <!-- The designer's page has no breadcrumb, to leave the height to the canvas: the toolbar links back to the list. -->
            <template v-if="config.listUrl">
                <a :href="config.listUrl" class="dpd-back link-secondary" data-cy="toolbar-list">{{ t("PipelinesList", "Data pipelines") }}</a>
                <span class="dpd-back-separator" aria-hidden="true">/</span>
            </template>
            <h2 class="dpd-title text-truncate" :title="state.settings?.description ?? undefined">{{ state.settings?.name }}</h2>

            <span
                v-if="!isVersionPage && state.publishedVersion"
                class="badge rounded-pill text-bg-light border"
                :title="t('PublishedVersionHint', 'The version runs use.')"
                data-cy="published-version"
            >
                {{ t("VersionNumber", "Version {0}", state.publishedVersion.number) }}
            </span>

            <template v-if="isVersionPage && state.version">
                <span class="badge rounded-pill text-bg-light border" data-cy="page-version">{{ t("VersionNumber", "Version {0}", state.version.number) }}</span>
                <span v-if="state.version.isPublished" class="badge text-bg-success" data-cy="page-version-published">{{ t("PublishedBadge", "Published") }}</span>
                <span class="small text-body-secondary">
                    {{
                        state.version.publishedBy
                            ? t("PublishedOnBy", "Published on {0} by {1}", formatDateTime(state.version.publishedUtc), state.version.publishedBy)
                            : t("PublishedOn", "Published on {0}", formatDateTime(state.version.publishedUtc))
                    }}
                </span>
                <a v-if="config.designerUrl" :href="config.designerUrl" class="btn btn-sm btn-outline-secondary ms-auto" data-cy="page-version-designer">{{ t("OpenDesigner", "Open the designer") }}</a>
            </template>

            <template v-if="!readOnly && !loading && !loadError">
                <SaveStatusIndicator
                    :status="state.saveStatus"
                    :has-draft="state.hasDraft"
                    :is-published="!!state.publishedVersion"
                    @retry="autosave.save()"
                    @resolve="conflictOpen = true"
                />

                <div class="btn-group btn-group-sm" role="group" :aria-label="t('History', 'History')">
                    <button
                        type="button"
                        class="btn btn-outline-secondary"
                        :title="t('UndoShortcut', 'Undo (Ctrl+Z)')"
                        :aria-label="t('Undo', 'Undo')"
                        :disabled="!state.canUndo"
                        data-cy="toolbar-undo"
                        @click="store.undo()"
                    >
                        <i class="fa-solid fa-rotate-left dpd-mirror-rtl" aria-hidden="true"></i>
                    </button>
                    <button
                        type="button"
                        class="btn btn-outline-secondary"
                        :title="t('RedoShortcut', 'Redo (Ctrl+Y)')"
                        :aria-label="t('Redo', 'Redo')"
                        :disabled="!state.canRedo"
                        data-cy="toolbar-redo"
                        @click="store.redo()"
                    >
                        <i class="fa-solid fa-rotate-right dpd-mirror-rtl" aria-hidden="true"></i>
                    </button>
                </div>

                <button
                    v-if="config.urls.versions"
                    type="button"
                    class="btn btn-sm btn-outline-secondary"
                    :title="t('VersionsHint', 'The published versions of the pipeline')"
                    data-cy="toolbar-versions"
                    @click="versionsOpen = true"
                >
                    {{ t("Versions", "Versions") }}
                </button>
                <button
                    v-if="runOffered"
                    type="button"
                    class="btn btn-sm btn-outline-success"
                    :disabled="!canRunNow || runStarting"
                    :title="runDisabledReason ?? t('RunNowHint', 'Runs the published version now.')"
                    data-cy="toolbar-run"
                    @click="startRun"
                >
                    <span v-if="runStarting" class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                    <i v-else class="fa-solid fa-play" aria-hidden="true"></i>
                    {{ t("Run", "Run") }}
                </button>
                <button v-if="config.urls.discard" type="button" class="btn btn-sm btn-outline-danger" :disabled="!hasChanges || busy" data-cy="toolbar-discard" @click="discard">
                    {{ t("DiscardDraft", "Discard draft") }}
                </button>
                <button v-if="config.urls.publish" type="button" class="btn btn-sm btn-primary" :disabled="!canPublish" :title="publishTitle" data-cy="toolbar-publish" @click="publish">
                    <span v-if="busy" class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                    {{ t("Publish", "Publish") }}
                </button>
            </template>
        </header>

        <DraftBanner v-if="showBanner" :modified-by="state.draftModifiedBy" :modified-utc="state.draftModifiedUtc" @dismiss="bannerDismissed = true" />

        <div class="dpd-body">
            <aside
                v-if="!readOnly"
                ref="toolbox"
                class="dpd-toolbox"
                :class="{ 'is-collapsed': toolboxCollapsed, 'is-peeking': toolboxPeeking }"
                :aria-label="t('Toolbox', 'Toolbox')"
                data-cy="designer-toolbox"
                @mouseenter="toolboxCollapsed && toolboxPeek.enter()"
                @mouseleave="toolboxCollapsed && toolboxPeek.leave()"
                @focusout="toolboxCollapsed && toolboxPeek.focusOut($event)"
                @keydown.esc="toolboxPeeking && closeToolboxPeek()"
            >
                <div v-if="toolboxCollapsed" class="dpd-rail" data-cy="toolbox-rail">
                    <button
                        type="button"
                        class="dpd-rail-button"
                        :title="t('ExpandToolbox', 'Expand the toolbox')"
                        :aria-label="t('ExpandToolbox', 'Expand the toolbox')"
                        aria-expanded="false"
                        data-cy="toolbox-expand"
                        @click="setToolboxCollapsed(false)"
                    >
                        <i class="fa-solid fa-angles-right dpd-mirror-rtl" aria-hidden="true"></i>
                    </button>
                    <button
                        type="button"
                        class="dpd-rail-button"
                        :title="t('StepsHint', 'Drag a step to the canvas, or click it to add it.')"
                        :aria-label="t('Steps', 'Steps')"
                        data-cy="toolbox-rail-steps"
                        @click="setToolboxCollapsed(false)"
                    >
                        <i class="fa-solid fa-shapes" aria-hidden="true"></i>
                    </button>
                </div>
                <div v-show="!toolboxCollapsed || toolboxPeeking" class="dpd-toolbox-sheet" data-cy="toolbox-sheet" @dragstart="onToolboxDragStart" @dragend="onToolboxDragEnd">
                    <div class="dpd-toolbox-header">
                        <h3 class="dpd-toolbox-title" :title="t('StepsHint', 'Drag a step to the canvas, or click it to add it.')">{{ t("Steps", "Steps") }}</h3>
                        <button
                            type="button"
                            class="btn btn-sm btn-link dpd-toolbox-toggle"
                            :title="toolboxCollapsed ? t('ExpandToolbox', 'Expand the toolbox') : t('CollapseToolbox', 'Collapse the toolbox')"
                            :aria-label="toolboxCollapsed ? t('ExpandToolbox', 'Expand the toolbox') : t('CollapseToolbox', 'Collapse the toolbox')"
                            :aria-expanded="!toolboxCollapsed"
                            data-cy="toolbox-collapse"
                            @click="setToolboxCollapsed(!toolboxCollapsed)"
                        >
                            <i class="fa-solid dpd-mirror-rtl" :class="toolboxCollapsed ? 'fa-angles-right' : 'fa-angles-left'" aria-hidden="true"></i>
                        </button>
                    </div>
                    <div class="dpd-toolbox-scroll">
                        <StepToolbox :library="library" :loading="libraryLoading" :error="libraryError" @add="onAddStep" />
                    </div>
                </div>
            </aside>

            <!-- The canvas, with the selected step's panel over it, or below it when pinned. -->
            <div ref="main" class="dpd-main" :style="{ '--dpd-covered-height': `${stepPanel?.coveredHeight ?? 0}px` }">
                <section class="dpd-canvas-host" :aria-label="t('Canvas', 'Canvas')" :aria-busy="loading" data-cy="designer-canvas">
                    <div v-if="loading" class="dpd-message">
                        <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                        {{ t("Loading", "Loading…") }}
                    </div>
                    <div v-else-if="loadError" class="dpd-message text-danger" role="alert">{{ loadError }}</div>
                    <DesignerCanvas
                        v-else
                        ref="canvas"
                        :store="store"
                        :read-only="readOnly"
                        @edit="editStep"
                        @preview="previewStep"
                        @drop-step="onDropStep"
                        @add-after="onAddAfter"
                    />
                    <!-- An empty pipeline starts with a source: they are offered. -->
                    <StartPicker v-if="!readOnly && !loading && !loadError && state.nodes.length === 0 && !draggingStep" :library="library" @pick="onAddStep" />
                </section>

                <QuickAddMenu
                    v-if="quickAdd"
                    :library="library"
                    :kind="quickAdd.kind"
                    :port="quickAdd.portLabel"
                    :left="quickAdd.left"
                    :top="quickAdd.top"
                    @pick="onQuickAddPick"
                    @cancel="onQuickAddCancel"
                />

                <StepPanel
                    v-if="!loading && !loadError"
                    ref="stepPanel"
                    :store="store"
                    :api="api"
                    :read-only="readOnly"
                    :can-preview="!!config.urls.preview"
                    :mutate="mutate"
                    :prepare="prepare"
                    @return-focus="returnFocus"
                    @closed="canvas?.focus()"
                    @delete="canvas?.deleteStep($event)"
                    @conflict="autosave.reportConflict"
                />
            </div>

            <PipelinePanel
                v-if="!loading && !loadError"
                ref="panel"
                :store="store"
                :api="api"
                :read-only="readOnly"
                :can-edit-settings="!!config.urls.settings"
                :tracker="tracker"
                :can-run="canRunNow"
                :run-disabled-reason="runDisabledReason"
                :starting="runStarting"
                :runs-page-url="config.runsPageUrl"
                :mutate="mutate"
                @focus-step="focusStep"
                @conflict="autosave.reportConflict"
                @run="startRun"
            />
        </div>

        <ConflictDialog v-if="conflictOpen && autosave.conflict.value" :problem="autosave.conflict.value" @reload="onReload" @overwrite="onOverwrite" @close="conflictOpen = false" />
        <VersionsDialog
            v-if="versionsOpen"
            :api="api"
            :version-page-url="config.versionPageUrl"
            :can-restore="!!config.urls.restore"
            @close="versionsOpen = false"
            @restore="restoreVersion"
        />
        <PublishDialog v-if="publishDialog" :decision="publishDialog" :nodes="state.nodes" @publish="doPublish" @close="publishDialog = null" @select-issue="onPublishIssueSelected" />

        <ToastHost />
    </div>
</template>
