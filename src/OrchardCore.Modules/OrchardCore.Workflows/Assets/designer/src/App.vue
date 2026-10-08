<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from "vue";
import type { DesignerConfig } from "./config";
import { DesignerApiError, withQuery, type DesignerApi } from "./api/designerApi";
import type { DesignIssue, DesignerVersion, Library, RetryResult } from "./api/types";
import { designerStore, type DesignerStore } from "./state/designerStore";
import { addNodeCommand } from "./state/commands";
import DesignerCanvas from "./canvas/DesignerCanvas.vue";
import ActivityToolbox from "./toolbox/ActivityToolbox.vue";
import WorkflowPanel from "./panel/WorkflowPanel.vue";
import ActivityPanel from "./panel/ActivityPanel.vue";
import { startVariableCompletions } from "./variables/variableCompletions";
import { upstreamValues } from "./available/availableData";
import ToastHost from "./ui/ToastHost.vue";
import SaveStatusIndicator from "./draft/SaveStatusIndicator.vue";
import ConflictDialog from "./draft/ConflictDialog.vue";
import PublishDialog from "./draft/PublishDialog.vue";
import DraftBanner from "./draft/DraftBanner.vue";
import PresenceList from "./realtime/PresenceList.vue";
import RemoteChangeNotice from "./realtime/RemoteChangeNotice.vue";
import { startRealtime, type RealtimeSession, type WorkflowTypeChangedMessage } from "./realtime/realtime";
import VersionsDialog from "./draft/VersionsDialog.vue";
import { formatDateTime } from "./draft/formatDateTime";
import { decidePublish, type PublishDecision } from "./draft/publishDecision";
import { createRevisionQueue } from "./services/revisionQueue";
import { createAutosave } from "./services/autosave";
import { showToast } from "./ui/toasts";
import { confirmAction } from "./ui/confirm";
import { usePeek } from "./ui/usePeek";
import { readPreference, writePreference } from "./ui/preferences";
import { selectNode } from "./canvas/useConnect";
import { scriptErrorsByActivity } from "./canvas/scriptErrors";
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
// The workflow's panel (variables, settings, issues), and the selected activity's panel below the canvas.
const panel = ref<InstanceType<typeof WorkflowPanel> | null>(null);
const activityPanel = ref<InstanceType<typeof ActivityPanel> | null>(null);
const conflictOpen = ref(false);
const publishDialog = ref<Exclude<PublishDecision, { kind: "publish" }> | null>(null);
const busy = ref(false);
const bannerDismissed = ref(false);
const versionsOpen = ref(false);

// Whether an activity of the instance shown by the viewer ran a script that failed.
const hasScriptErrors = computed(() => scriptErrorsByActivity(state.instance).size > 0);

// The version page links back to the designer, and to the comparison with the published version.
const isVersionPage = computed(() => props.config.mode === "version");
const compareWithPublishedUrl = computed(() =>
    props.config.comparePageUrl && state.version && state.publishedVersion && !state.version.isPublished
        ? withQuery(props.config.comparePageUrl, { from: state.version.versionId, to: state.publishedVersion.versionId })
        : null,
);

// The toolbox can be collapsed into a rail, to give the canvas more room; hovering the rail opens it over
// the canvas, and it stays open while an activity is dragged from it.
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

const closeToolboxPeek = () => {
    toolboxPeek.close();
    toolbox.value?.querySelector<HTMLElement>("[data-cy=toolbox-rail-activities]")?.focus();
};

/**
 * Applies the pending changes of both panels' forms; resolves to false when the user wants to keep editing.
 */
const settleForms = async () => (await (activityPanel.value?.settle() ?? true)) && (await (panel.value?.settle() ?? true));

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
                await settleForms();
                await reloadDefinition();
            })();
        }
    },
});

const hasChanges = computed(() => state.hasDraft || state.saveStatus !== "saved");
const canPublish = computed(() => hasChanges.value && !busy.value && state.saveStatus !== "conflict");
const showBanner = computed(
    () => !bannerDismissed.value && state.hasDraft && !!state.draftModifiedByUserId && state.draftModifiedByUserId !== (props.config.currentUserId ?? null),
);

// Editing an activity (Enter or double-click on it) moves the focus to its first field; Escape in its
// panel brings it back.
const editActivity = (activityId: string) => {
    void activityPanel.value?.open(activityId, { focus: true });
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
        activityPanel.value?.discardChanges();
        props.store.loadDefinition(definition);
        autosave.reset();
        bannerDismissed.value = false;
        await Promise.all([panel.value?.refresh(), activityPanel.value?.refresh()]);
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

const isEditable = (target: EventTarget | null) => target instanceof HTMLElement && (target.isContentEditable || ["INPUT", "TEXTAREA", "SELECT"].includes(target.tagName));

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
    if (!props.config.readOnly && (autosave.hasUnsavedChanges() || panel.value?.hasPendingChanges() || activityPanel.value?.hasPendingChanges())) {
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
            void activityPanel.value?.open(result.node.id);
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

            props.store.markPublished(result.issues, result.version ?? null);
        });

        showToast({ message: state.publishedVersion ? t("PublishedVersion", state.publishedVersion.version) : t("Published"), variant: "success" });
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
        if (!(await settleForms())) {
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

/**
 * Copies a version into the draft (asking first when the draft has changes), then loads the draft.
 */
const restoreVersion = async (version: DesignerVersion) => {
    if (
        hasChanges.value &&
        !(await confirmAction({
            title: t("RestoreVersionTitle"),
            message: t("RestoreVersionMessage", version.version),
            okText: t("Restore"),
            cancelText: t("Cancel"),
        }))
    ) {
        return;
    }

    busy.value = true;

    try {
        if (!(await settleForms()) || !(await autosave.flush())) {
            return;
        }

        await mutate((revision) => props.api.restore(version.versionId, revision));
        versionsOpen.value = false;
        await reloadDefinition();
        showToast({ message: t("VersionRestored", version.version), variant: "success" });
    } catch (error) {
        onRequestError(error, t("RestoreFailed"));
    } finally {
        busy.value = false;
    }

    await keepFocus();
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

// Stops the variable completions of the script editors.
let stopCompletions: (() => void) | null = null;

// Live updates, when the OrchardCore.Workflows.SignalR feature is enabled.
let realtime: RealtimeSession | null = null;
let unmounted = false;

const onWorkflowTypeChanged = (change: WorkflowTypeChangedMessage) => {
    // The signed-in user's own changes come back too; another tab of theirs is caught by the revision checks.
    if (change.userId && change.userId === (props.config.currentUserId ?? null)) {
        return;
    }

    if (change.kind === "DraftChanged" && change.revision <= state.revision) {
        return;
    }

    state.remoteChange = change;
};

const onInstanceChanged = async () => {
    try {
        props.store.loadDefinition(await props.api.getDefinition());
        await nextTick();
        canvas.value?.reveal(state.instance?.blockingActivityIds ?? []);
    } catch {
        // The next change tries again.
    }
};

const startLiveUpdates = async () => {
    if (!props.config.hubUrl || (props.config.mode && props.config.mode !== "designer" && props.config.mode !== "instance")) {
        return;
    }

    const session = await startRealtime({
        url: props.config.hubUrl,
        workflowTypeId: props.config.readOnly ? null : state.workflowTypeId,
        workflowId: props.config.readOnly ? (state.instance?.workflowId ?? null) : null,
        onWorkflowTypeChanged,
        onInstanceChanged,
        onPresenceChanged: (presence) => {
            state.presence = presence;
        },
    });

    if (unmounted) {
        await session?.stop();
    } else {
        realtime = session;
    }
};

const onRemoteReload = async () => {
    state.remoteChange = null;
    await onReload();
};

onMounted(async () => {
    document.addEventListener("keydown", onKeyDown);
    window.addEventListener("beforeunload", onBeforeUnload);
    state.currentUserId = props.config.currentUserId ?? null;

    if (!props.config.readOnly) {
        void loadLibrary();
        stopCompletions = startVariableCompletions(() => ({
            variables: state.variables,
            types: state.variableTypes,
            values: upstreamValues(state.nodes, state.transitions, state.variables, state.selectedNodeIds.length === 1 ? state.selectedNodeIds[0] : null),
        }));
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
        // The activities the instance waits on are never left in a collapsed branch.
        canvas.value?.reveal(state.instance?.blockingActivityIds ?? []);
        canvas.value?.fit();
    }

    // An activity to open, for example from an old activity edit URL.
    const initialActivityId = props.config.initialActivityId;

    if (!props.config.readOnly && initialActivityId && props.store.getNode(initialActivityId)) {
        await nextTick();
        canvas.value?.centerOn(initialActivityId);
        await activityPanel.value?.open(initialActivityId);
    }

    if (!loadError.value) {
        void startLiveUpdates();
    }
});

// The instance ran again: show what it did now.
const onRetried = async (result: RetryResult) => {
    try {
        props.store.loadDefinition(await props.api.getDefinition());
    } catch {
        loadError.value = t("LoadFailed");

        return;
    }

    await nextTick();
    canvas.value?.reveal(state.instance?.blockingActivityIds ?? []);
    showToast({ message: t("Retried", result.status), variant: result.status === "Faulted" ? "warning" : "success" });
};

onBeforeUnmount(() => {
    document.removeEventListener("keydown", onKeyDown);
    window.removeEventListener("beforeunload", onBeforeUnload);
    autosave.stop();
    stopCompletions?.();
    unmounted = true;
    void realtime?.stop();
});

defineExpose({ canvas, panel, activityPanel, addActivity, autosave, publish, discard });
</script>

<template>
    <div class="wfd" :class="{ 'wfd-readonly': config.readOnly }" data-cy="workflow-designer">
        <header class="wfd-toolbar" data-cy="designer-toolbar">
            <!-- The designer's page has no breadcrumb, to leave the height to the canvas: the toolbar links back to the list. -->
            <template v-if="!config.readOnly && config.listUrl">
                <a :href="config.listUrl" class="wfd-back link-secondary" data-cy="toolbar-list">{{ t("WorkflowsList") }}</a>
                <span class="wfd-back-separator" aria-hidden="true">/</span>
            </template>
            <h2 class="wfd-title text-truncate">{{ state.settings?.name }}</h2>

            <span
                v-if="config.mode !== 'version' && !config.readOnly && state.publishedVersion"
                class="badge rounded-pill text-bg-light border"
                :title="t('PublishedVersionHint')"
                data-cy="published-version"
            >
                {{ t("VersionNumber", state.publishedVersion.version) }}
            </span>

            <template v-if="isVersionPage && state.version">
                <span class="badge rounded-pill text-bg-light border" data-cy="page-version">{{ t("VersionNumber", state.version.version) }}</span>
                <span v-if="state.version.isPublished" class="badge text-bg-success" data-cy="page-version-published">{{ t("PublishedBadge") }}</span>
                <span class="small text-body-secondary">
                    {{ state.version.createdBy ? t("CreatedOnBy", formatDateTime(state.version.createdUtc), state.version.createdBy) : formatDateTime(state.version.createdUtc) }}
                </span>
                <div class="btn-group btn-group-sm ms-auto" role="group" :aria-label="t('MoreActions')">
                    <a v-if="compareWithPublishedUrl" :href="compareWithPublishedUrl" class="btn btn-outline-secondary" data-cy="page-version-compare">
                        {{ t("CompareWithPublished") }}
                    </a>
                    <a v-if="config.designerUrl" :href="config.designerUrl" class="btn btn-outline-secondary" data-cy="page-version-designer">{{ t("OpenDesigner") }}</a>
                </div>
            </template>

            <span v-if="state.instance && state.version" class="small text-nowrap" data-cy="instance-version">
                {{ t("RunsOnVersion", state.version.version) }}
                <span v-if="!state.version.isPublished" class="text-body-secondary" data-cy="instance-version-not-published">({{ t("NotThePublishedVersion") }})</span>
            </span>

            <span v-if="config.readOnly && state.instance" class="wfd-legend small text-body-secondary" data-cy="viewer-legend">
                <span class="badge text-bg-info">
                    <i class="fa-solid fa-hourglass-half" aria-hidden="true"></i>
                    {{ t("Blocking") }}
                </span>
                {{ t("BlockingLegend") }}
                <span class="wfd-legend-executed" aria-hidden="true"></span>
                {{ t("ExecutedLegend") }}
                <template v-if="hasScriptErrors">
                    <span class="badge text-bg-warning" data-cy="script-error-legend">
                        <i class="fa-solid fa-bug" aria-hidden="true"></i>
                    </span>
                    {{ t("ScriptErrorLegend") }}
                </template>
            </span>

            <template v-if="!config.readOnly && !loading && !loadError">
                <SaveStatusIndicator :status="state.saveStatus" @retry="autosave.save()" @resolve="conflictOpen = true" />
                <PresenceList :presence="state.presence" />

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
                    <button
                        v-if="config.urls.versions"
                        type="button"
                        class="btn btn-outline-secondary"
                        :title="t('VersionsHint')"
                        data-cy="toolbar-versions"
                        @click="versionsOpen = true"
                    >
                        {{ t("Versions") }}
                    </button>
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

        <RemoteChangeNotice v-if="state.remoteChange && !config.readOnly" :change="state.remoteChange" @reload="onRemoteReload" @dismiss="state.remoteChange = null" />
        <DraftBanner v-if="showBanner" :modified-by="state.draftModifiedBy" :modified-utc="state.draftModifiedUtc" @dismiss="bannerDismissed = true" />

        <div class="wfd-body">
            <aside
                v-if="!config.readOnly"
                ref="toolbox"
                class="wfd-toolbox"
                :class="{ 'is-collapsed': toolboxCollapsed, 'is-peeking': toolboxPeeking }"
                :aria-label="t('Toolbox')"
                data-cy="designer-toolbox"
                @mouseenter="toolboxCollapsed && toolboxPeek.enter()"
                @mouseleave="toolboxCollapsed && toolboxPeek.leave()"
                @focusout="toolboxCollapsed && toolboxPeek.focusOut($event)"
                @keydown.esc="toolboxPeeking && closeToolboxPeek()"
            >
                <div v-if="toolboxCollapsed" class="wfd-rail" data-cy="toolbox-rail">
                    <button
                        type="button"
                        class="wfd-rail-button"
                        :title="t('ExpandToolbox')"
                        :aria-label="t('ExpandToolbox')"
                        aria-expanded="false"
                        data-cy="toolbox-expand"
                        @click="setToolboxCollapsed(false)"
                    >
                        <i class="fa-solid fa-angles-right wfd-mirror-rtl" aria-hidden="true"></i>
                    </button>
                    <button
                        type="button"
                        class="wfd-rail-button"
                        :title="t('ActivitiesHint')"
                        :aria-label="t('Activities')"
                        data-cy="toolbox-rail-activities"
                        @click="setToolboxCollapsed(false)"
                    >
                        <i class="fa-solid fa-shapes" aria-hidden="true"></i>
                    </button>
                </div>
                <div
                    v-show="!toolboxCollapsed || toolboxPeeking"
                    class="wfd-toolbox-sheet"
                    data-cy="toolbox-sheet"
                    @dragstart="toolboxPeek.hold()"
                    @dragend="toolboxPeek.release()"
                >
                    <div class="wfd-toolbox-header">
                        <h3 class="wfd-toolbox-title" :title="t('ActivitiesHint')">{{ t("Activities") }}</h3>
                        <button
                            type="button"
                            class="btn btn-sm btn-link wfd-toolbox-toggle"
                            :title="toolboxCollapsed ? t('ExpandToolbox') : t('CollapseToolbox')"
                            :aria-label="toolboxCollapsed ? t('ExpandToolbox') : t('CollapseToolbox')"
                            :aria-expanded="!toolboxCollapsed"
                            data-cy="toolbox-collapse"
                            @click="setToolboxCollapsed(!toolboxCollapsed)"
                        >
                            <i class="fa-solid wfd-mirror-rtl" :class="toolboxCollapsed ? 'fa-angles-right' : 'fa-angles-left'" aria-hidden="true"></i>
                        </button>
                    </div>
                    <div class="wfd-toolbox-scroll">
                        <ActivityToolbox :library="library" :loading="libraryLoading" :error="libraryError" @add="onAddActivity" />
                    </div>
                </div>
            </aside>

            <!-- The canvas, with the selected activity's panel over it, or below it when pinned. -->
            <div class="wfd-main" :style="{ '--wfd-covered-height': `${activityPanel?.coveredHeight ?? 0}px` }">
                <section class="wfd-canvas-host" :aria-label="t('Canvas')" :aria-busy="loading" data-cy="designer-canvas">
                    <div v-if="loading" class="wfd-message">
                        <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                        {{ t("Loading") }}
                    </div>
                    <div v-else-if="loadError" class="wfd-message text-danger" role="alert">{{ loadError }}</div>
                    <DesignerCanvas v-else ref="canvas" :store="store" :read-only="config.readOnly" @edit="editActivity" @drop-activity="onDropActivity" />
                </section>

                <ActivityPanel
                    v-if="!loading && !loadError"
                    ref="activityPanel"
                    :store="store"
                    :api="api"
                    :read-only="config.readOnly"
                    :mutate="mutate"
                    :can-retry="!!config.urls.retry"
                    @retried="onRetried"
                    @return-focus="returnFocus"
                    @closed="canvas?.focus()"
                    @delete="canvas?.deleteActivity($event)"
                    @conflict="autosave.reportConflict"
                />
            </div>

            <WorkflowPanel
                v-if="!loading && !loadError"
                ref="panel"
                :store="store"
                :api="api"
                :read-only="config.readOnly"
                :mutate="mutate"
                @focus-activity="focusActivity"
                @conflict="autosave.reportConflict"
            />
        </div>

        <ConflictDialog
            v-if="conflictOpen && autosave.conflict.value"
            :problem="autosave.conflict.value"
            @reload="onReload"
            @overwrite="onOverwrite"
            @close="conflictOpen = false"
        />
        <VersionsDialog
            v-if="versionsOpen"
            :api="api"
            :version-page-url="config.versionPageUrl"
            :compare-page-url="config.comparePageUrl"
            @close="versionsOpen = false"
            @restore="restoreVersion"
        />
        <PublishDialog
            v-if="publishDialog"
            :decision="publishDialog"
            :nodes="state.nodes"
            @publish="doPublish"
            @close="publishDialog = null"
            @select-issue="onPublishIssueSelected"
        />

        <ToastHost />
    </div>
</template>
