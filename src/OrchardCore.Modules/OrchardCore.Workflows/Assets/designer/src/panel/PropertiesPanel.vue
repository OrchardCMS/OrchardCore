<script setup lang="ts">
import { computed, nextTick, onMounted, ref, useId, watch } from "vue";
import type { DesignerApi } from "../api/designerApi";
import { DesignerApiError } from "../api/designerApi";
import type { DesignIssue, EditorApplied, SettingsApplied } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { selectNode } from "../canvas/useConnect";
import ServerFormHost from "./ServerFormHost.vue";
import IssuesList from "./IssuesList.vue";
import type { FormApplyResult } from "./types";
import type { RevisionTask } from "../services/revisionQueue";
import { showToast } from "../ui/toasts";
import { confirmAction } from "../ui/confirm";
import { t } from "../i18n";

type Tab = "activity" | "workflow" | "issues";

const MIN_WIDTH = 288;
const MAX_WIDTH = 720;
const WIDTH_KEY = "orchardcore:workflows-designer:panel-width";

const props = withDefaults(
    defineProps<{
        store: DesignerStore;
        api: DesignerApi;
        readOnly?: boolean;
        /**
         * Runs a request that changes the draft, in the designer's revision queue.
         */
        mutate?: <T>(task: RevisionTask<T>) => Promise<T>;
    }>(),
    {
        readOnly: false,
        mutate: undefined,
    },
);

const mutate = <T,>(task: RevisionTask<T>) => (props.mutate ? props.mutate(task) : task(props.store.state.revision));

const emit = defineEmits<{
    (event: "focus-activity", activityId: string): void;
    (event: "return-focus", activityId: string): void;
    (event: "conflict", error: DesignerApiError): void;
}>();

const state = props.store.state;
const tab = ref<Tab>("activity");
const collapsed = ref(false);
const width = ref(384);
const editingId = ref<string | null>(null);
const activityHost = ref<InstanceType<typeof ServerFormHost> | null>(null);
const settingsHost = ref<InstanceType<typeof ServerFormHost> | null>(null);
const body = ref<HTMLElement | null>(null);
const tabList = ref<HTMLElement | null>(null);
const ids = `wfd-panel-${useId()}`;

const tabs = computed<Tab[]>(() => (props.readOnly ? ["activity"] : ["activity", "workflow", "issues"]));

// Set by open({ focus: true }): the first field of the activity editor gets the focus once it is loaded.
let focusOnLoad = false;

const selectedId = computed(() => (state.selectedNodeIds.length === 1 ? state.selectedNodeIds[0] : null));
const editingNode = computed(() => (editingId.value ? props.store.getNode(editingId.value) : undefined));
const errorCount = computed(() => state.issues.filter((issue) => issue.severity === "Error").length);
const isBlocking = computed(() => !!editingNode.value && !!state.instance?.blockingActivityIds.includes(editingNode.value.id));

const readWidth = () => {
    try {
        const stored = Number(window.localStorage.getItem(WIDTH_KEY));

        return stored >= MIN_WIDTH && stored <= MAX_WIDTH ? stored : null;
    } catch {
        return null;
    }
};

const storeWidth = () => {
    try {
        window.localStorage.setItem(WIDTH_KEY, String(width.value));
    } catch {
        // The width is a per-viewer convenience; ignore storage failures.
    }
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

    collapsed.value = !collapsed.value;
};

/**
 * Shows the editor of an activity, for example after a double-click or adding it from the toolbox.
 */
const open = async (activityId: string, options: { focus?: boolean } = {}) => {
    collapsed.value = false;

    if (tab.value !== "activity" && !(await settle())) {
        return;
    }

    const loaded = tab.value === "activity" && editingId.value === activityId;

    tab.value = "activity";
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

    if (event.key !== "Escape" || event.defaultPrevented || !editingId.value || target?.closest(".monaco-editor, .CodeMirror")) {
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

defineExpose({ open, settle, selectTab, discardChanges, refresh, hasPendingChanges });
</script>

<template>
    <aside
        class="wfd-panel"
        :class="{ 'is-collapsed': collapsed }"
        :style="collapsed ? undefined : { flexBasis: `${width}px`, width: `${width}px` }"
        :aria-label="t('Properties')"
        data-cy="designer-panel"
    >
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
            <ul v-if="!collapsed" ref="tabList" class="nav nav-tabs wfd-panel-tabs" role="tablist" @keydown="onTabKeyDown">
                <li class="nav-item" role="presentation">
                    <button
                        :id="`${ids}-tab-activity`"
                        type="button"
                        role="tab"
                        class="nav-link"
                        :class="{ active: tab === 'activity' }"
                        :aria-selected="tab === 'activity'"
                        :aria-controls="`${ids}-body`"
                        :tabindex="tab === 'activity' ? 0 : -1"
                        data-cy="panel-tab-activity"
                        @click="selectTab('activity')"
                    >
                        {{ t("ActivityTab") }}
                    </button>
                </li>
                <li v-if="!readOnly" class="nav-item" role="presentation">
                    <button
                        :id="`${ids}-tab-workflow`"
                        type="button"
                        role="tab"
                        class="nav-link"
                        :class="{ active: tab === 'workflow' }"
                        :aria-selected="tab === 'workflow'"
                        :aria-controls="`${ids}-body`"
                        :tabindex="tab === 'workflow' ? 0 : -1"
                        data-cy="panel-tab-workflow"
                        @click="selectTab('workflow')"
                    >
                        {{ t("WorkflowTab") }}
                    </button>
                </li>
                <li v-if="!readOnly" class="nav-item" role="presentation">
                    <button
                        :id="`${ids}-tab-issues`"
                        type="button"
                        role="tab"
                        class="nav-link"
                        :class="{ active: tab === 'issues' }"
                        :aria-selected="tab === 'issues'"
                        :aria-controls="`${ids}-body`"
                        :tabindex="tab === 'issues' ? 0 : -1"
                        data-cy="panel-tab-issues"
                        @click="selectTab('issues')"
                    >
                        {{ t("IssuesTab") }}
                        <span v-if="state.issues.length > 0" class="badge ms-1" :class="errorCount > 0 ? 'text-bg-danger' : 'text-bg-warning'" data-cy="issues-count">
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
                @click="toggleCollapsed"
            >
                <i class="fa-solid wfd-mirror-rtl" :class="collapsed ? 'fa-angles-left' : 'fa-angles-right'" aria-hidden="true"></i>
            </button>
        </div>

        <div
            v-show="!collapsed"
            :id="`${ids}-body`"
            ref="body"
            class="wfd-panel-body"
            role="tabpanel"
            :aria-labelledby="`${ids}-tab-${tab}`"
            @keydown="onBodyKeyDown"
        >
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
                </div>
                <p v-else-if="editingNode.isMissing" class="wfd-panel-message text-warning" data-cy="panel-missing">{{ t("MissingActivityCannotBeEdited") }}</p>
                <template v-else>
                    <h3 class="wfd-panel-title h6" tabindex="-1" data-cy="panel-activity-title">
                        <i :class="editingNode.icon || 'fa-solid fa-gear'" aria-hidden="true"></i>
                        {{ editingNode.title }}
                        <small class="text-secondary">{{ editingNode.displayText }}</small>
                    </h3>
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
                    />
                </template>
            </template>

            <ServerFormHost
                v-else-if="tab === 'workflow'"
                ref="settingsHost"
                form-key="settings"
                :load="loadSettings"
                :submit="submitSettings"
                :label="t('WorkflowTab')"
                data-cy="panel-settings-form"
                @applied="onSettingsApplied"
                @error="onError"
            />

            <IssuesList v-else :issues="state.issues" :nodes="state.nodes" @select="onIssueSelected" />
        </div>
    </aside>
</template>
