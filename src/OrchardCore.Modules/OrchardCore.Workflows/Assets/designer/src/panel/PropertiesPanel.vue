<script setup lang="ts">
import { computed, nextTick, onMounted, ref, useId, watch } from "vue";
import type { DesignerApi } from "../api/designerApi";
import { DesignerApiError } from "../api/designerApi";
import type { DesignIssue, EditorApplied, RetryResult, SettingsApplied } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { selectNode } from "../canvas/useConnect";
import ServerFormHost from "./ServerFormHost.vue";
import IssuesList from "./IssuesList.vue";
import OutputBindings from "./OutputBindings.vue";
import VariablesTab from "./VariablesTab.vue";
import JournalTab from "./JournalTab.vue";
import AvailableData from "../available/AvailableData.vue";
import { canInsertAt, insertAtCursor } from "../available/insertion";
import type { FormApplyResult } from "./types";
import type { RevisionTask } from "../services/revisionQueue";
import { showToast } from "../ui/toasts";
import { confirmAction } from "../ui/confirm";
import { usePeek } from "../ui/usePeek";
import { readPreference, writePreference } from "../ui/preferences";
import { t } from "../i18n";

type Tab = "activity" | "variables" | "journal" | "workflow" | "issues";

// The views of the Activity tab: the activity's settings, or the data its expressions can read.
type ActivityView = "settings" | "data";

const ACTIVITY_VIEWS: ActivityView[] = ["settings", "data"];

const MIN_WIDTH = 288;
const MAX_WIDTH = 720;
const WIDTH_KEY = "panel-width";
const COLLAPSED_KEY = "panel-collapsed";

const TAB_ICONS: Record<Tab, string> = {
    activity: "fa-solid fa-sliders",
    variables: "fa-solid fa-square-root-variable",
    journal: "fa-solid fa-list-check",
    workflow: "fa-solid fa-gear",
    issues: "fa-solid fa-triangle-exclamation",
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
    (event: "focus-activity", activityId: string): void;
    (event: "return-focus", activityId: string): void;
    (event: "conflict", error: DesignerApiError): void;
    (event: "retried", result: RetryResult): void;
}>();

const state = props.store.state;
const tab = ref<Tab>("activity");
const activityView = ref<ActivityView>("settings");
const activityViews = ref<HTMLElement | null>(null);
// Read before the first render, so a collapsed panel doesn't open and close on load.
const collapsed = ref(readPreference(COLLAPSED_KEY) === "true");
const width = ref(384);
const editingId = ref<string | null>(null);
const activityHost = ref<InstanceType<typeof ServerFormHost> | null>(null);
const settingsHost = ref<InstanceType<typeof ServerFormHost> | null>(null);
const body = ref<HTMLElement | null>(null);
const tabList = ref<HTMLElement | null>(null);
const panel = ref<HTMLElement | null>(null);
const ids = `wfd-panel-${useId()}`;

const tabs = computed<Tab[]>(() => {
    if (!props.readOnly) {
        return ["activity", "variables", "workflow", "issues"];
    }

    // The viewer of an instance shows its journal.
    return state.instance ? ["activity", "variables", "journal"] : ["activity", "variables"];
});

// Collapsed, the panel is a rail of its tabs; hovering it opens the panel over the canvas (a "peek").
const peek = usePeek((element) => !!element && !!panel.value?.contains(element));
const peeking = computed(() => collapsed.value && peek.open.value);

// Direct t("…") calls, so the translations spec sees every key.
const tabLabel = (item: Tab) =>
    ({ activity: t("ActivityTab"), variables: t("VariablesTab"), journal: t("JournalTab"), workflow: t("WorkflowTab"), issues: t("IssuesTab") })[item];

const tabHint = (item: Tab) => {
    if (item === "activity") {
        return props.readOnly ? t("ActivityTabHintReadOnly") : t("ActivityTabHint");
    }

    if (item === "variables") {
        return props.readOnly ? t("VariablesTabHintReadOnly") : t("VariablesTabHint");
    }

    if (item === "journal") {
        return t("JournalTabHint");
    }

    return item === "workflow" ? t("WorkflowTabHint") : t("IssuesTabHint");
};

// Set by open({ focus: true }): the first field of the activity editor gets the focus once it is loaded.
let focusOnLoad = false;

const selectedId = computed(() => (state.selectedNodeIds.length === 1 ? state.selectedNodeIds[0] : null));
const editingNode = computed(() => (editingId.value ? props.store.getNode(editingId.value) : undefined));

// The field of the activity's editor that had the focus last, where an available value is inserted.
let lastField: Element | null = null;

const onFormFocus = (event: FocusEvent) => {
    if (event.target instanceof Element && event.target.closest(".wfd-form-content")) {
        lastField = event.target;
    }
};

watch(editingId, () => {
    lastField = null;
    activityView.value = "settings";
});

// Direct t("…") calls, so the translations spec sees every key.
const activityViewLabel = (view: ActivityView) => (view === "settings" ? t("ActivitySettings") : t("AvailableData"));

// The views follow the ARIA tabs pattern, like the panel's tabs.
const onActivityViewKeyDown = async (event: KeyboardEvent) => {
    if (!["ArrowLeft", "ArrowRight", "Home", "End"].includes(event.key)) {
        return;
    }

    event.preventDefault();
    activityView.value = event.key === "Home" ? "settings" : event.key === "End" ? "data" : activityView.value === "settings" ? "data" : "settings";
    await nextTick();
    activityViews.value?.querySelector<HTMLElement>(`[data-cy=panel-view-${activityView.value}]`)?.focus();
};

/**
 * Inserts an expression where the cursor was in the activity's settings, which come back into view, or copies it.
 */
const onInsert = async (text: string) => {
    if (canInsertAt(lastField)) {
        activityView.value = "settings";
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
const errorCount = computed(() => state.issues.filter((issue) => issue.severity === "Error").length);
const isBlocking = computed(() => !!editingNode.value && !!state.instance?.blockingActivityIds.includes(editingNode.value.id));

const readWidth = () => {
    const stored = Number(readPreference(WIDTH_KEY));

    return stored >= MIN_WIDTH && stored <= MAX_WIDTH ? stored : null;
};

const storeWidth = () => writePreference(WIDTH_KEY, String(width.value));

const setCollapsed = (value: boolean) => {
    collapsed.value = value;
    peek.close();
    writePreference(COLLAPSED_KEY, String(value));
};

/**
 * Applies the pending changes of the open form. When the form is invalid, asks whether to discard them;
 * resolves to false when the user wants to keep editing.
 */
const settle = async () => {
    const host = tab.value === "workflow" ? settingsHost.value : activityHost.value;

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
        // Keep the invalid activity selected so its errors stay visible.
        selectNode(props.store, editingId.value);

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

const selectTab = async (next: Tab) => {
    if (next !== tab.value && (await settle())) {
        tab.value = next;
    }
};

const toggleCollapsed = async () => {
    if (!collapsed.value && !(await settle())) {
        return;
    }

    setCollapsed(!collapsed.value);
};

/**
 * Expands the collapsed panel, on `next` when it is given.
 */
const expand = async (next?: Tab) => {
    setCollapsed(false);

    if (next) {
        await selectTab(next);
    }
};

// Hovering a tab of the rail shows that tab, unless the open form has changes to apply first.
const peekAt = (next: Tab) => {
    const host = tab.value === "workflow" ? settingsHost.value : activityHost.value;

    if (next !== tab.value && !host?.isDirty() && !host?.isInvalid()) {
        tab.value = next;
    }
};

/**
 * Shows the editor of an activity, for example after a double-click or adding it from the toolbox.
 */
const open = async (activityId: string, options: { focus?: boolean; expand?: boolean } = {}) => {
    if ((options.expand ?? true) && collapsed.value) {
        setCollapsed(false);
    }

    if (tab.value !== "activity" && !(await settle())) {
        return;
    }

    const loaded = tab.value === "activity" && editingId.value === activityId;

    tab.value = "activity";
    activityView.value = "settings";
    focusOnLoad = !!options.focus;
    selectNode(props.store, activityId);
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
 * Moves the focus to the first field of the open form, or to the panel's title.
 */
const focusFirstField = () => {
    const field = Array.from(body.value?.querySelectorAll<HTMLElement>(FIELDS) ?? []).find((element) => !element.hasAttribute("disabled") && isVisible(element));

    (field ?? body.value?.querySelector<HTMLElement>(".wfd-panel-title"))?.focus();
};

const onActivityLoaded = () => {
    if (focusOnLoad) {
        focusOnLoad = false;
        focusFirstField();
    }
};

// Escape goes back to the activity on the canvas, unless a rich editor uses it (suggestions, search).
const onBodyKeyDown = (event: KeyboardEvent) => {
    const target = event.target as Element | null;

    if (event.key !== "Escape" || event.defaultPrevented || target?.closest(".monaco-editor, .CodeMirror")) {
        return;
    }

    // Escape closes the panel opened over the canvas.
    if (peeking.value) {
        event.preventDefault();
        peek.close();
        panel.value?.querySelector<HTMLElement>(`[data-cy=panel-rail-${tab.value}]`)?.focus();

        return;
    }

    if (!editingId.value) {
        return;
    }

    event.preventDefault();
    emit("return-focus", editingId.value);
};

// The tabs follow the ARIA tabs pattern: one tab stop, arrow keys (mirrored in RTL), Home and End.
const onTabKeyDown = async (event: KeyboardEvent) => {
    const index = tabs.value.indexOf(tab.value);
    const rtl = !!tabList.value && getComputedStyle(tabList.value).direction === "rtl";
    let next: number;

    if (event.key === "ArrowRight" || event.key === "ArrowLeft") {
        next = index + ((event.key === "ArrowRight") !== rtl ? 1 : -1);
    } else if (event.key === "Home") {
        next = 0;
    } else if (event.key === "End") {
        next = tabs.value.length - 1;
    } else {
        return;
    }

    event.preventDefault();
    next = (next + tabs.value.length) % tabs.value.length;
    await selectTab(tabs.value[next]);
    await nextTick();
    tabList.value?.querySelector<HTMLElement>(`[data-cy=panel-tab-${tab.value}]`)?.focus();
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

const loadSettings = () => props.api.getSettings();

const submitSettings = (form: FormData) => mutate((revision) => props.api.postSettings(revision, form)) as Promise<FormApplyResult>;

const onSettingsApplied = (result: FormApplyResult & { valid: true }) => {
    const applied = result as unknown as SettingsApplied;

    props.store.applyServerChange(applied.revision, applied.issues);
    state.settings = applied.settings;
};

const onJournalSelected = (activityId: string) => {
    if (props.store.getNode(activityId)) {
        selectNode(props.store, activityId);
        emit("focus-activity", activityId);
    }
};

const retrying = ref(false);
const isFaultedInstance = computed(() => props.canRetry && state.instance?.status === "Faulted");

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

const onIssueSelected = (issue: DesignIssue) => {
    if (issue.activityId) {
        selectNode(props.store, issue.activityId);
        emit("focus-activity", issue.activityId);
    }
};

const startResize = (event: PointerEvent) => {
    const startX = event.clientX;
    const startWidth = width.value;

    // The panel is on the inline-end side, so it grows as the pointer moves toward the inline start.
    const direction = getComputedStyle(event.currentTarget as Element).direction === "rtl" ? 1 : -1;

    const onMove = (moveEvent: PointerEvent) => {
        width.value = Math.min(MAX_WIDTH, Math.max(MIN_WIDTH, startWidth + direction * (moveEvent.clientX - startX)));
    };

    const onUp = () => {
        window.removeEventListener("pointermove", onMove);
        window.removeEventListener("pointerup", onUp);
        storeWidth();
    };

    window.addEventListener("pointermove", onMove);
    window.addEventListener("pointerup", onUp);
    event.preventDefault();
};

const onResizerKeyDown = (event: KeyboardEvent) => {
    const step = event.shiftKey ? 64 : 16;

    if (event.key === "ArrowLeft" || event.key === "ArrowRight") {
        event.preventDefault();
        const grow = (event.key === "ArrowLeft") !== (getComputedStyle(event.currentTarget as Element).direction === "rtl");
        width.value = Math.min(MAX_WIDTH, Math.max(MIN_WIDTH, width.value + (grow ? step : -step)));
        storeWidth();
    }
};

onMounted(() => {
    width.value = readWidth() ?? width.value;
    editingId.value = selectedId.value;
});

/**
 * Forgets the unapplied changes of the open forms, for example before loading the definition again.
 */
const discardChanges = () => {
    activityHost.value?.discard();
    settingsHost.value?.discard();
};

/**
 * Loads the open form again, for example after loading the definition again.
 */
const refresh = async () => {
    await nextTick();
    await (tab.value === "workflow" ? settingsHost.value : activityHost.value)?.reload();
};

/**
 * Whether an open form has changes that weren't applied.
 */
const hasPendingChanges = () => !!(activityHost.value?.isDirty() || settingsHost.value?.isDirty());

defineExpose({ open, settle, selectTab, expand, discardChanges, refresh, hasPendingChanges });
</script>

<template>
    <aside
        ref="panel"
        class="wfd-panel"
        :class="{ 'is-collapsed': collapsed, 'is-peeking': peeking }"
        :style="collapsed ? undefined : { flexBasis: `${width}px`, width: `${width}px` }"
        :aria-label="t('Properties')"
        data-cy="designer-panel"
        @mouseenter="collapsed && peek.enter()"
        @mouseleave="collapsed && peek.leave()"
        @focusout="collapsed && peek.focusOut($event)"
    >
        <div v-if="collapsed" class="wfd-rail" data-cy="panel-rail">
            <button type="button" class="wfd-rail-button" :title="t('ExpandPanel')" :aria-label="t('ExpandPanel')" aria-expanded="false" data-cy="panel-expand" @click="expand()">
                <i class="fa-solid fa-angles-left wfd-mirror-rtl" aria-hidden="true"></i>
            </button>
            <button
                v-for="item in tabs"
                :key="item"
                type="button"
                class="wfd-rail-button"
                :class="{ active: item === tab }"
                :title="tabHint(item)"
                :aria-label="tabLabel(item)"
                :data-cy="`panel-rail-${item}`"
                @click="expand(item)"
                @mouseenter="peekAt(item)"
            >
                <i :class="TAB_ICONS[item]" aria-hidden="true"></i>
                <span v-if="item === 'issues' && state.issues.length > 0" class="badge wfd-rail-badge" :class="errorCount > 0 ? 'text-bg-danger' : 'text-bg-warning'">
                    {{ state.issues.length }}
                </span>
            </button>
        </div>

        <!-- The Set Variable editor's name field suggests the declared variables. -->
        <datalist id="wfd-variables" data-cy="variables-datalist">
            <option v-for="variable in state.variables" :key="variable.name" :value="variable.name"></option>
        </datalist>

        <div v-show="!collapsed || peeking" class="wfd-panel-sheet" :style="peeking ? { width: `${width}px` } : undefined" data-cy="panel-sheet">
            <div
                v-if="!collapsed"
                class="wfd-panel-resizer"
                role="separator"
                aria-orientation="vertical"
                :aria-label="t('ResizePanel')"
                :aria-valuenow="width"
                :aria-valuemin="MIN_WIDTH"
                :aria-valuemax="MAX_WIDTH"
                tabindex="0"
                data-cy="panel-resizer"
                @pointerdown="startResize"
                @keydown="onResizerKeyDown"
            ></div>

            <div class="wfd-panel-header">
                <ul ref="tabList" class="nav nav-tabs wfd-panel-tabs" role="tablist" @keydown="onTabKeyDown">
                    <li v-for="item in tabs" :key="item" class="nav-item" role="presentation">
                        <button
                            :id="`${ids}-tab-${item}`"
                            type="button"
                            role="tab"
                            class="nav-link"
                            :class="{ active: tab === item }"
                            :aria-selected="tab === item"
                            :aria-controls="`${ids}-body`"
                            :tabindex="tab === item ? 0 : -1"
                            :title="tabHint(item)"
                            :data-cy="`panel-tab-${item}`"
                            @click="selectTab(item)"
                        >
                            {{ tabLabel(item) }}
                            <span
                                v-if="item === 'issues' && state.issues.length > 0"
                                class="badge ms-1"
                                :class="errorCount > 0 ? 'text-bg-danger' : 'text-bg-warning'"
                                data-cy="issues-count"
                            >
                                {{ state.issues.length }}
                            </span>
                        </button>
                    </li>
                </ul>
                <button
                    type="button"
                    class="btn btn-sm btn-link wfd-panel-toggle"
                    :title="collapsed ? t('ExpandPanel') : t('CollapsePanel')"
                    :aria-label="collapsed ? t('ExpandPanel') : t('CollapsePanel')"
                    :aria-expanded="!collapsed"
                    data-cy="panel-collapse"
                    @click="collapsed ? expand() : toggleCollapsed()"
                >
                    <i class="fa-solid wfd-mirror-rtl" :class="collapsed ? 'fa-angles-left' : 'fa-angles-right'" aria-hidden="true"></i>
                </button>
            </div>

            <div :id="`${ids}-body`" ref="body" class="wfd-panel-body" role="tabpanel" :aria-labelledby="`${ids}-tab-${tab}`" @keydown="onBodyKeyDown">
                <template v-if="tab === 'activity'">
                    <p v-if="state.selectedNodeIds.length > 1" class="wfd-panel-message" data-cy="panel-multiple">
                        {{ t("MultipleSelected", state.selectedNodeIds.length) }}
                    </p>
                    <p v-else-if="!editingNode" class="wfd-panel-message" data-cy="panel-empty">{{ readOnly ? t("SelectActivityToView") : t("SelectActivityToEdit") }}</p>
                    <div v-else-if="readOnly" class="wfd-panel-summary" data-cy="panel-summary">
                        <h3 class="wfd-panel-title h6" tabindex="-1">
                            <i :class="editingNode.icon || 'fa-solid fa-gear'" aria-hidden="true"></i>
                            {{ editingNode.title }}
                        </h3>
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
                        <OutputBindings :node="editingNode" :store="store" :api="api" read-only />
                        <div v-if="isFaultedInstance" class="wfd-retry" data-cy="retry">
                            <p v-if="state.instance?.faultedActivityId === editingNode.id && state.instance.faultMessage" class="wfd-retry-error" data-cy="fault-message">
                                <i class="fa-solid fa-circle-xmark" aria-hidden="true"></i>
                                {{ state.instance.faultMessage }}
                            </p>
                            <button type="button" class="btn btn-sm btn-outline-primary" :disabled="retrying" data-cy="retry-button" @click="retry">
                                <i class="fa-solid fa-rotate-right" aria-hidden="true"></i>
                                {{ t("RetryFromHere") }}
                            </button>
                        </div>
                    </div>
                    <p v-else-if="editingNode.isMissing" class="wfd-panel-message text-warning" data-cy="panel-missing">{{ t("MissingActivityCannotBeEdited") }}</p>
                    <template v-else>
                        <h3 class="wfd-panel-title h6" tabindex="-1" data-cy="panel-activity-title">
                            <i :class="editingNode.icon || 'fa-solid fa-gear'" aria-hidden="true"></i>
                            {{ editingNode.title }}
                            <small class="text-secondary">{{ editingNode.displayText }}</small>
                        </h3>
                        <div ref="activityViews" class="wfd-activity-views" role="tablist" :aria-label="t('ActivityTab')" @keydown="onActivityViewKeyDown">
                            <button
                                v-for="view in ACTIVITY_VIEWS"
                                :id="`${ids}-view-${view}`"
                                :key="view"
                                type="button"
                                role="tab"
                                class="wfd-activity-view"
                                :class="{ active: activityView === view }"
                                :aria-selected="activityView === view"
                                :aria-controls="`${ids}-view-${view}-panel`"
                                :tabindex="activityView === view ? 0 : -1"
                                :data-cy="`panel-view-${view}`"
                                @click="activityView = view"
                            >
                                <i class="fa-solid" :class="view === 'settings' ? 'fa-sliders' : 'fa-database'" aria-hidden="true"></i>
                                {{ activityViewLabel(view) }}
                            </button>
                        </div>
                        <!-- Hidden rather than removed, so the editor keeps its changes and its cursor. -->
                        <div v-show="activityView === 'settings'" :id="`${ids}-view-settings-panel`" role="tabpanel" :aria-labelledby="`${ids}-view-settings`">
                            <ServerFormHost
                                ref="activityHost"
                                :form-key="editingNode.id"
                                :load="loadActivity"
                                :submit="submitActivity"
                                :label="t('ActivityTab')"
                                data-cy="panel-activity-form"
                                @loaded="onActivityLoaded"
                                @applied="onActivityApplied"
                                @error="onError"
                                @focusin="onFormFocus"
                            />
                            <OutputBindings :node="editingNode" :store="store" :api="api" :mutate="mutate" @error="onError" />
                        </div>
                        <div v-show="activityView === 'data'" :id="`${ids}-view-data-panel`" role="tabpanel" :aria-labelledby="`${ids}-view-data`">
                            <AvailableData :node="editingNode" :store="store" @insert="onInsert" />
                        </div>
                    </template>
                </template>

                <VariablesTab v-else-if="tab === 'variables'" :store="store" :api="api" :read-only="readOnly" :mutate="mutate" @error="onError" />

                <JournalTab v-else-if="tab === 'journal'" :store="store" @select="onJournalSelected" />

                <template v-else-if="tab === 'workflow'">
                    <p class="wfd-panel-intro">{{ t("WorkflowTabHint") }}</p>
                    <ServerFormHost
                        ref="settingsHost"
                        form-key="settings"
                        :load="loadSettings"
                        :submit="submitSettings"
                        :label="t('WorkflowTab')"
                        data-cy="panel-settings-form"
                        @applied="onSettingsApplied"
                        @error="onError"
                    />
                </template>

                <template v-else>
                    <p class="wfd-panel-intro">{{ t("IssuesTabHint") }}</p>
                    <IssuesList :issues="state.issues" :nodes="state.nodes" @select="onIssueSelected" />
                </template>
            </div>
        </div>
    </aside>
</template>
