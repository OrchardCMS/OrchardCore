<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, useId, watch } from "vue";
import type { DesignerApi } from "../api/designerApi";
import { DesignerApiError } from "../api/designerApi";
import type { EditorApplied, RetryResult } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { clearSelection, showActivity } from "../canvas/useConnect";
import ServerFormHost from "./ServerFormHost.vue";
import OutputBindings from "./OutputBindings.vue";
import AvailableData from "../available/AvailableData.vue";
import RunsTab from "./RunsTab.vue";
import { formatDateTime } from "../draft/formatDateTime";
import { canInsertAt, insertAtCursor } from "../available/insertion";
import type { FormApplyResult } from "./types";
import type { RevisionTask } from "../services/revisionQueue";
import { showToast } from "../ui/toasts";
import { confirmAction } from "../ui/confirm";
import { readPreference, writePreference } from "../ui/preferences";
import { t } from "../i18n";

// The tabs of the selected activity: its settings, the variables that store its outputs, and the data its
// expressions can read; the viewer shows its details instead of its settings, and its runs by the instance.
type Tab = "settings" | "outputs" | "data" | "details" | "runs";

const MIN_HEIGHT = 128;
// The canvas keeps at least this much room above the panel.
const CANVAS_MIN_HEIGHT = 96;
const HEIGHT_KEY = "activity-panel-height";
const PINNED_KEY = "activity-panel-pinned";

const TAB_ICONS: Record<Tab, string> = {
    settings: "fa-solid fa-sliders",
    outputs: "fa-solid fa-right-from-bracket",
    data: "fa-solid fa-database",
    details: "fa-solid fa-circle-info",
    runs: "fa-solid fa-clock-rotate-left",
};

const props = withDefaults(
    defineProps<{
        store: DesignerStore;
        api: DesignerApi;
        readOnly?: boolean;
        /**
         * Runs a request that changes the draft, in the designer's revision queue.
         */
        mutate?: <T>(task: RevisionTask<T>) => Promise<T>;
        /**
         * Whether the viewer offers to retry a faulted instance.
         */
        canRetry?: boolean;
    }>(),
    {
        readOnly: false,
        mutate: undefined,
        canRetry: false,
    },
);

const mutate = <T,>(task: RevisionTask<T>) => (props.mutate ? props.mutate(task) : task(props.store.state.revision));

const emit = defineEmits<{
    (event: "return-focus", activityId: string): void;
    (event: "closed"): void;
    (event: "delete", activityId: string): void;
    (event: "conflict", error: DesignerApiError): void;
    (event: "retried", result: RetryResult): void;
}>();

const state = props.store.state;
const editingId = ref<string | null>(null);
const tab = ref<Tab>(props.readOnly ? "details" : "settings");
// Pinned, the panel stays open below the canvas; otherwise it opens over the canvas while an activity is selected.
const pinned = ref(readPreference(PINNED_KEY) === "true");
// Until it is resized, the panel takes a part of the designer's height (see the stylesheet).
const height = ref<number | null>(null);
const activityHost = ref<InstanceType<typeof ServerFormHost> | null>(null);
const panel = ref<HTMLElement | null>(null);
const body = ref<HTMLElement | null>(null);
const tabList = ref<HTMLElement | null>(null);
const ids = `wfd-activity-${useId()}`;

const selectedId = computed(() => (state.selectedNodeIds.length === 1 ? state.selectedNodeIds[0] : null));
const editingNode = computed(() => (editingId.value ? props.store.getNode(editingId.value) : undefined));
// Unpinned, it opens when the selected activity's panel is asked for (a click, not a drag; see activityPanelRequested).
const isOpen = computed(() => pinned.value || (!!editingNode.value && state.activityPanelRequested));

const tabs = computed<Tab[]>(() => {
    const node = editingNode.value;

    if (!node) {
        return [];
    }

    const hasOutputs = (node.outputs ?? []).length > 0;

    if (props.readOnly) {
        const ran = (state.instance?.journal ?? []).some((record) => record.activityId === node.id);

        return ["details", ...(ran ? (["runs"] as Tab[]) : []), ...(hasOutputs ? (["outputs"] as Tab[]) : [])];
    }

    if (node.isMissing) {
        return [];
    }

    return hasOutputs ? ["settings", "outputs", "data"] : ["settings", "data"];
});

// Direct t("…") calls, so the translations spec sees every key.
const tabLabel = (item: Tab) => ({ settings: t("ActivitySettings"), outputs: t("Outputs"), data: t("AvailableData"), details: t("ActivityDetails"), runs: t("RunsTab") })[item];

const tabHint = (item: Tab) =>
    ({
        settings: t("ActivityTabHint"),
        outputs: props.readOnly ? t("OutputsHintReadOnly") : t("OutputsHint"),
        data: t("AvailableDataHint"),
        details: t("ActivityTabHintReadOnly"),
        runs: t("RunsTabHint"),
    })[item];

// Set by open({ focus: true }): the first field of the activity editor gets the focus once it is loaded.
let focusOnLoad = false;

// The field of the activity's editor that had the focus last, where an available value is inserted.
let lastField: Element | null = null;

const onFormFocus = (event: FocusEvent) => {
    if (event.target instanceof Element && event.target.closest(".wfd-form-content")) {
        lastField = event.target;
    }
};

// Whether the run selected in the journal is one of the edited activity's.
const isFocusedRun = () =>
    state.focusedRunSequence !== null && (state.instance?.journal ?? []).some((record) => record.sequence === state.focusedRunSequence && record.activityId === editingId.value);

watch(editingId, () => {
    lastField = null;
    tab.value = props.readOnly ? (isFocusedRun() ? "runs" : "details") : "settings";
});

// A run selected in the journal opens on the Runs tab of its activity.
watch(
    () => state.focusedRunSequence,
    () => {
        if (props.readOnly && isFocusedRun()) {
            tab.value = "runs";
        }
    },
);

// An activity whose outputs were removed no longer has their tab.
watch(tabs, (next) => {
    if (next.length > 0 && !next.includes(tab.value)) {
        tab.value = next[0];
    }
});

/**
 * Inserts an expression where the cursor was in the activity's settings, which come back into view, or copies it.
 */
const onInsert = async (text: string) => {
    if (canInsertAt(lastField)) {
        tab.value = "settings";
        await nextTick();

        if (await insertAtCursor(lastField, text)) {
            return;
        }
    }

    try {
        await navigator.clipboard.writeText(text);
        showToast({ message: t("ExpressionCopied", text), variant: "success" });
    } catch {
        showToast({ message: t("ExpressionCopyFailed"), variant: "warning" });
    }
};

const isBlocking = computed(() => !!editingNode.value && !!state.instance?.blockingActivityIds.includes(editingNode.value.id));

/**
 * Applies the pending changes of the activity's settings. When they are invalid, asks whether to discard them;
 * resolves to false when the user wants to keep editing.
 */
const settle = async () => {
    const host = activityHost.value;

    if (!host || (await host.apply()) || !host.isInvalid()) {
        return true;
    }

    const discard = await confirmAction({
        title: t("DiscardChangesTitle"),
        message: t("DiscardChangesMessage"),
        okText: t("Discard"),
        cancelText: t("KeepEditing"),
    });

    if (discard) {
        host.discard();
    }

    return discard;
};

watch(selectedId, async (next) => {
    if (next === editingId.value) {
        return;
    }

    if (editingId.value && !(await settle())) {
        // Keep the invalid activity selected, and its panel open, so its errors stay visible.
        showActivity(props.store, editingId.value);

        return;
    }

    editingId.value = next;
});

// The edited activity was deleted (or undone away).
watch(editingNode, (node) => {
    if (!node && editingId.value) {
        activityHost.value?.discard();
        editingId.value = selectedId.value;
    }
});

const setPinned = (value: boolean) => {
    pinned.value = value;
    writePreference(PINNED_KEY, String(value));
};

/**
 * Closes the panel opened over the canvas: the activity's changes are applied, and it is no longer selected.
 */
const close = async () => {
    if (await settle()) {
        clearSelection(props.store);
        emit("closed");
    }
};

/**
 * Deletes the activity shown, which closes the panel; the canvas offers to undo it. Its unapplied changes are dropped
 * rather than posted for an activity that no longer exists.
 */
const deleteActivity = () => {
    if (editingId.value) {
        activityHost.value?.discard();
        emit("delete", editingId.value);
    }
};

/**
 * Shows the editor of an activity, for example after a double-click or adding it from the toolbox.
 */
const open = async (activityId: string, options: { focus?: boolean } = {}) => {
    const loaded = editingId.value === activityId;

    tab.value = props.readOnly ? "details" : "settings";
    focusOnLoad = !!options.focus;
    showActivity(props.store, activityId);
    await nextTick();

    // The editor of this activity is already open, so it won't load again.
    if (loaded && focusOnLoad) {
        focusOnLoad = false;
        focusFirstField();
    }
};

const isVisible = (element: HTMLElement) => (typeof element.checkVisibility === "function" ? element.checkVisibility() : true);

const FIELDS = ".wfd-form-content input:not([type=hidden]), .wfd-form-content select, .wfd-form-content textarea, .wfd-form-content button";

/**
 * Moves the focus to the first field of the activity's editor, or to the panel's title.
 */
const focusFirstField = () => {
    const field = Array.from(body.value?.querySelectorAll<HTMLElement>(FIELDS) ?? []).find((element) => !element.hasAttribute("disabled") && isVisible(element));

    (field ?? panel.value?.querySelector<HTMLElement>(".wfd-panel-title"))?.focus();
};

const onActivityLoaded = () => {
    if (focusOnLoad) {
        focusOnLoad = false;
        focusFirstField();
    }
};

// Escape goes back to the activity on the canvas, unless a rich editor uses it (suggestions, search).
const onKeyDown = (event: KeyboardEvent) => {
    const target = event.target as Element | null;

    if (event.key !== "Escape" || event.defaultPrevented || !editingId.value || target?.closest(".monaco-editor, .CodeMirror")) {
        return;
    }

    event.preventDefault();
    emit("return-focus", editingId.value);
};

// The tabs follow the ARIA tabs pattern: one tab stop, arrow keys (mirrored in RTL), Home and End.
const onTabKeyDown = async (event: KeyboardEvent) => {
    const items = tabs.value;
    const index = items.indexOf(tab.value);
    const rtl = !!tabList.value && getComputedStyle(tabList.value).direction === "rtl";
    let next: number;

    if (event.key === "ArrowRight" || event.key === "ArrowLeft") {
        next = index + ((event.key === "ArrowRight") !== rtl ? 1 : -1);
    } else if (event.key === "Home") {
        next = 0;
    } else if (event.key === "End") {
        next = items.length - 1;
    } else {
        return;
    }

    event.preventDefault();
    tab.value = items[(next + items.length) % items.length];
    await nextTick();
    tabList.value?.querySelector<HTMLElement>(`[data-cy=activity-tab-${tab.value}]`)?.focus();
};

const onError = (error: unknown) => {
    if (error instanceof DesignerApiError && error.isConflict) {
        emit("conflict", error);

        return;
    }

    showToast({ message: t("ApplyFailed"), variant: "danger" });
};

const loadActivity = () => props.api.getEditor(editingId.value!);

const submitActivity = (form: FormData) => {
    // The request may wait in the queue, so it keeps the activity being edited now.
    const activityId = editingId.value!;

    return mutate((revision) => props.api.postEditor(activityId, revision, form)) as Promise<FormApplyResult>;
};

const onActivityApplied = (result: FormApplyResult & { valid: true }) => {
    const applied = result as unknown as EditorApplied;

    props.store.applyServerChange(applied.revision, applied.issues);
    const removed = props.store.replaceNode(applied.node);

    if (removed.length > 0) {
        showToast({ message: t("TransitionsRemoved", removed.length), variant: "warning" });
    }
};

const retrying = ref(false);
const isFaultedInstance = computed(() => props.canRetry && state.instance?.status === "Faulted");

// The next attempt of the shown task, when the instance waits to retry it.
const pendingRetry = computed(() => {
    const retry = state.instance?.status === "Faulted" ? state.instance.pendingRetry : null;

    return retry && retry.activityId === editingNode.value?.id ? retry : null;
});

/**
 * Runs the faulted instance again from the activity shown, after a confirmation.
 */
const retry = async () => {
    const node = editingNode.value;
    const instance = state.instance;

    if (!node || !instance) {
        return;
    }

    const confirmed = await confirmAction({
        title: t("RetryTitle"),
        message: t("RetryMessage", node.title),
        okText: t("Retry"),
        cancelText: t("Cancel"),
    });

    if (!confirmed) {
        return;
    }

    retrying.value = true;

    try {
        emit("retried", await props.api.retry(instance.id, node.id));
    } catch {
        showToast({ message: t("RetryFailed"), variant: "danger" });
    } finally {
        retrying.value = false;
    }
};

const maxHeight = () => Math.max(MIN_HEIGHT, (panel.value?.parentElement?.clientHeight ?? 0) - CANVAS_MIN_HEIGHT);

const setHeight = (value: number) => {
    height.value = Math.round(Math.min(maxHeight(), Math.max(MIN_HEIGHT, value)));
};

const storeHeight = () => {
    if (height.value !== null) {
        writePreference(HEIGHT_KEY, String(height.value));
    }
};

const startResize = (event: PointerEvent) => {
    const startY = event.clientY;
    const startHeight = panel.value?.offsetHeight ?? MIN_HEIGHT;

    // The panel is at the bottom, so it grows as the pointer moves up.
    const onMove = (moveEvent: PointerEvent) => setHeight(startHeight - (moveEvent.clientY - startY));

    const onUp = () => {
        window.removeEventListener("pointermove", onMove);
        window.removeEventListener("pointerup", onUp);
        storeHeight();
    };

    window.addEventListener("pointermove", onMove);
    window.addEventListener("pointerup", onUp);
    event.preventDefault();
};

const onResizerKeyDown = (event: KeyboardEvent) => {
    if (event.key !== "ArrowUp" && event.key !== "ArrowDown") {
        return;
    }

    event.preventDefault();
    const step = event.shiftKey ? 64 : 16;
    setHeight((height.value ?? panel.value?.offsetHeight ?? MIN_HEIGHT) + (event.key === "ArrowUp" ? step : -step));
    storeHeight();
};

// The height the panel has on screen (0 while it is closed).
const renderedHeight = ref(0);
let resizeObserver: ResizeObserver | null = null;

onMounted(() => {
    const stored = Number(readPreference(HEIGHT_KEY));

    height.value = stored >= MIN_HEIGHT ? stored : null;
    editingId.value = selectedId.value;

    if (typeof ResizeObserver === "function" && panel.value) {
        resizeObserver = new ResizeObserver(() => (renderedHeight.value = panel.value?.offsetHeight ?? 0));
        resizeObserver.observe(panel.value);
    }
});

onBeforeUnmount(() => resizeObserver?.disconnect());

/**
 * Forgets the unapplied changes of the activity's settings, for example before loading the definition again.
 */
const discardChanges = () => activityHost.value?.discard();

/**
 * Loads the activity's settings again, for example after loading the definition again.
 */
const refresh = async () => {
    await nextTick();
    await activityHost.value?.reload();
};

/**
 * Whether the activity's settings have changes that weren't applied.
 */
const hasPendingChanges = () => !!activityHost.value?.isDirty();

/**
 * The height of the canvas the panel covers, so the canvas can keep its controls above it.
 */
const coveredHeight = computed(() => (isOpen.value && !pinned.value ? renderedHeight.value : 0));

defineExpose({ open, settle, close, discardChanges, refresh, hasPendingChanges, isOpen, pinned, coveredHeight });
</script>

<template>
    <section
        v-show="isOpen"
        ref="panel"
        class="wfd-activity-panel"
        :class="{ 'is-pinned': pinned }"
        :style="height !== null ? { height: `${height}px` } : undefined"
        :aria-label="t('ActivityTab')"
        data-cy="activity-panel"
        @keydown="onKeyDown"
    >
        <div
            class="wfd-activity-panel-resizer"
            role="separator"
            aria-orientation="horizontal"
            :aria-label="t('ResizeActivityPanel')"
            :aria-valuenow="height ?? undefined"
            :aria-valuemin="MIN_HEIGHT"
            tabindex="0"
            data-cy="activity-panel-resizer"
            @pointerdown="startResize"
            @keydown="onResizerKeyDown"
        ></div>

        <div class="wfd-activity-panel-header">
            <h3 v-if="editingNode" class="wfd-panel-title wfd-activity-panel-title h6" tabindex="-1" data-cy="panel-activity-title">
                <i :class="editingNode.icon || 'fa-solid fa-gear'" aria-hidden="true"></i>
                <span class="text-truncate">{{ editingNode.title }}</span>
                <small class="text-secondary text-truncate">{{ editingNode.displayText }}</small>
            </h3>
            <h3 v-else class="wfd-panel-title wfd-activity-panel-title h6" tabindex="-1">
                <i class="fa-solid fa-sliders" aria-hidden="true"></i>
                {{ t("ActivityTab") }}
            </h3>

            <ul v-if="tabs.length > 0" ref="tabList" class="nav nav-tabs wfd-panel-tabs" role="tablist" :aria-label="t('ActivityTab')" @keydown="onTabKeyDown">
                <li v-for="item in tabs" :key="item" class="nav-item" role="presentation">
                    <button
                        :id="`${ids}-tab-${item}`"
                        type="button"
                        role="tab"
                        class="nav-link"
                        :class="{ active: tab === item }"
                        :aria-selected="tab === item"
                        :aria-controls="`${ids}-tab-${item}-panel`"
                        :tabindex="tab === item ? 0 : -1"
                        :title="tabHint(item)"
                        :aria-label="tabLabel(item)"
                        :data-cy="`activity-tab-${item}`"
                        @click="tab = item"
                    >
                        <i :class="TAB_ICONS[item]" aria-hidden="true"></i>
                        <span class="wfd-activity-tab-label">{{ tabLabel(item) }}</span>
                        <span v-if="item === 'outputs'" class="badge text-bg-secondary ms-1">{{ editingNode?.outputs?.length }}</span>
                    </button>
                </li>
            </ul>

            <div class="wfd-activity-panel-actions">
                <!-- Retrying a faulted instance from the activity shown, whichever tab is open. -->
                <button
                    v-if="editingNode && isFaultedInstance"
                    type="button"
                    class="btn btn-sm btn-outline-primary"
                    :disabled="retrying"
                    data-cy="retry-button"
                    @click="retry"
                >
                    <i class="fa-solid fa-rotate-right" aria-hidden="true"></i>
                    {{ t("RetryFromHere") }}
                </button>
                <button
                    v-if="editingNode && !readOnly"
                    type="button"
                    class="btn btn-sm btn-link link-danger"
                    :title="t('DeleteActivity')"
                    :aria-label="t('DeleteActivity')"
                    data-cy="activity-panel-delete"
                    @click="deleteActivity"
                >
                    <i class="fa-solid fa-trash" aria-hidden="true"></i>
                </button>
                <button
                    type="button"
                    class="btn btn-sm btn-link"
                    :class="{ active: pinned }"
                    :title="pinned ? t('UnpinActivityPanel') : t('PinActivityPanel')"
                    :aria-label="pinned ? t('UnpinActivityPanel') : t('PinActivityPanel')"
                    :aria-pressed="pinned"
                    data-cy="activity-panel-pin"
                    @click="setPinned(!pinned)"
                >
                    <i class="fa-solid fa-thumbtack" :class="{ 'fa-rotate-90': !pinned }" aria-hidden="true"></i>
                </button>
                <button
                    v-if="!pinned"
                    type="button"
                    class="btn btn-sm btn-link"
                    :title="t('CloseActivityPanel')"
                    :aria-label="t('CloseActivityPanel')"
                    data-cy="activity-panel-close"
                    @click="close"
                >
                    <i class="fa-solid fa-xmark" aria-hidden="true"></i>
                </button>
            </div>
        </div>

        <div ref="body" class="wfd-activity-panel-body">
            <p v-if="state.selectedNodeIds.length > 1" class="wfd-panel-message" data-cy="panel-multiple">
                {{ t("MultipleSelected", state.selectedNodeIds.length) }}
            </p>
            <p v-else-if="!editingNode" class="wfd-panel-message" data-cy="panel-empty">{{ readOnly ? t("SelectActivityToView") : t("SelectActivityToEdit") }}</p>

            <template v-else-if="readOnly">
                <div
                    v-show="tab === 'details'"
                    :id="`${ids}-tab-details-panel`"
                    class="wfd-panel-summary"
                    role="tabpanel"
                    :aria-labelledby="`${ids}-tab-details`"
                    data-cy="panel-summary"
                >
                    <dl class="wfd-summary-list">
                        <dt>{{ t("ActivityType") }}</dt>
                        <dd>{{ editingNode.displayText }}</dd>
                        <template v-if="state.instance">
                            <dt>{{ t("InstanceStatus") }}</dt>
                            <dd data-cy="panel-summary-blocking">
                                <span v-if="isBlocking" class="badge text-bg-info">
                                    <i class="fa-solid fa-hourglass-half" aria-hidden="true"></i>
                                    {{ t("WaitingOnActivity") }}
                                </span>
                                <span v-else>{{ t("NotWaitingOnActivity") }}</span>
                            </dd>
                        </template>
                    </dl>
                    <p v-if="editingNode.isMissing" class="text-warning small">{{ t("MissingActivity") }}</p>
                    <div class="wfd-node-body" v-html="editingNode.designHtml"></div>
                    <div v-if="isFaultedInstance && state.instance?.faultedActivityId === editingNode.id && state.instance.faultMessage" class="wfd-retry" data-cy="retry">
                        <p class="wfd-retry-error" data-cy="fault-message">
                            <i class="fa-solid fa-circle-xmark" aria-hidden="true"></i>
                            {{ state.instance.faultMessage }}
                        </p>
                    </div>
                    <p v-if="pendingRetry" class="wfd-pending-retry" data-cy="pending-retry">
                        <i class="fa-solid fa-rotate-right" aria-hidden="true"></i>
                        {{ t("NextRetryDue", pendingRetry.failedAttempts, pendingRetry.maxRetries, formatDateTime(pendingRetry.dueUtc)) }}
                    </p>
                </div>
                <div v-if="tab === 'runs'" :id="`${ids}-tab-runs-panel`" role="tabpanel" :aria-labelledby="`${ids}-tab-runs`">
                    <RunsTab :store="store" :api="api" :activity-id="editingNode.id" />
                </div>
                <div v-show="tab === 'outputs'" :id="`${ids}-tab-outputs-panel`" role="tabpanel" :aria-labelledby="`${ids}-tab-outputs`">
                    <OutputBindings :node="editingNode" :store="store" :api="api" read-only />
                </div>
            </template>

            <p v-else-if="editingNode.isMissing" class="wfd-panel-message text-warning" data-cy="panel-missing">{{ t("MissingActivityCannotBeEdited") }}</p>

            <template v-else>
                <!-- Hidden rather than removed, so the editor keeps its changes and its cursor. -->
                <div v-show="tab === 'settings'" :id="`${ids}-tab-settings-panel`" role="tabpanel" :aria-labelledby="`${ids}-tab-settings`">
                    <ServerFormHost
                        ref="activityHost"
                        :form-key="editingNode.id"
                        :load="loadActivity"
                        :submit="submitActivity"
                        :label="t('ActivitySettings')"
                        data-cy="panel-activity-form"
                        @loaded="onActivityLoaded"
                        @applied="onActivityApplied"
                        @error="onError"
                        @focusin="onFormFocus"
                    />
                </div>
                <div v-show="tab === 'outputs'" :id="`${ids}-tab-outputs-panel`" role="tabpanel" :aria-labelledby="`${ids}-tab-outputs`">
                    <OutputBindings :node="editingNode" :store="store" :api="api" :mutate="mutate" @error="onError" />
                </div>
                <div v-show="tab === 'data'" :id="`${ids}-tab-data-panel`" role="tabpanel" :aria-labelledby="`${ids}-tab-data`">
                    <AvailableData :node="editingNode" :store="store" @insert="onInsert" />
                </div>
            </template>
        </div>
    </section>
</template>
