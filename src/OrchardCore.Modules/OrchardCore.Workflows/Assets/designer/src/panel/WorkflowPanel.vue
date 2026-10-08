<script setup lang="ts">
import { computed, nextTick, onMounted, ref, useId } from "vue";
import type { DesignerApi } from "../api/designerApi";
import { DesignerApiError } from "../api/designerApi";
import type { DesignIssue, SettingsApplied } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { selectNode } from "../canvas/useConnect";
import ServerFormHost from "./ServerFormHost.vue";
import IssuesList from "./IssuesList.vue";
import VariablesTab from "./VariablesTab.vue";
import JournalTab from "./JournalTab.vue";
import type { FormApplyResult } from "./types";
import type { RevisionTask } from "../services/revisionQueue";
import { showToast } from "../ui/toasts";
import { confirmAction } from "../ui/confirm";
import { usePeek } from "../ui/usePeek";
import { readPreference, writePreference } from "../ui/preferences";
import { t } from "../i18n";

// What concerns the whole workflow; the selected activity has its own panel, below the canvas.
type Tab = "variables" | "journal" | "workflow" | "issues";

const MIN_WIDTH = 288;
const MAX_WIDTH = 720;
const WIDTH_KEY = "panel-width";
const COLLAPSED_KEY = "panel-collapsed";

const TAB_ICONS: Record<Tab, string> = {
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
    }>(),
    {
        readOnly: false,
        mutate: undefined,
    },
);

const mutate = <T,>(task: RevisionTask<T>) => (props.mutate ? props.mutate(task) : task(props.store.state.revision));

const emit = defineEmits<{
    (event: "focus-activity", activityId: string): void;
    (event: "conflict", error: DesignerApiError): void;
}>();

const state = props.store.state;
const tab = ref<Tab>("variables");
// Read before the first render, so a collapsed panel doesn't open and close on load.
const collapsed = ref(readPreference(COLLAPSED_KEY) === "true");
const width = ref(384);
const settingsHost = ref<InstanceType<typeof ServerFormHost> | null>(null);
const tabList = ref<HTMLElement | null>(null);
const panel = ref<HTMLElement | null>(null);
const ids = `wfd-panel-${useId()}`;

const tabs = computed<Tab[]>(() => {
    if (!props.readOnly) {
        return ["variables", "workflow", "issues"];
    }

    // The viewer of an instance shows its journal.
    return state.instance ? ["variables", "journal"] : ["variables"];
});

// Collapsed, the panel is a rail of its tabs; hovering it opens the panel over the canvas (a "peek").
const peek = usePeek((element) => !!element && !!panel.value?.contains(element));
const peeking = computed(() => collapsed.value && peek.open.value);

// Direct t("…") calls, so the translations spec sees every key.
const tabLabel = (item: Tab) => ({ variables: t("VariablesTab"), journal: t("JournalTab"), workflow: t("WorkflowTab"), issues: t("IssuesTab") })[item];

const tabHint = (item: Tab) => {
    if (item === "variables") {
        return props.readOnly ? t("VariablesTabHintReadOnly") : t("VariablesTabHint");
    }

    if (item === "journal") {
        return t("JournalTabHint");
    }

    return item === "workflow" ? t("WorkflowTabHint") : t("IssuesTabHint");
};

const errorCount = computed(() => state.issues.filter((issue) => issue.severity === "Error").length);

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
 * Applies the pending changes of the workflow's settings. When they are invalid, asks whether to discard them;
 * resolves to false when the user wants to keep editing.
 */
const settle = async () => {
    const host = tab.value === "workflow" ? settingsHost.value : null;

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
    const host = tab.value === "workflow" ? settingsHost.value : null;

    if (next !== tab.value && !host?.isDirty() && !host?.isInvalid()) {
        tab.value = next;
    }
};

// Escape closes the panel opened over the canvas, unless a rich editor uses it (suggestions, search).
const onBodyKeyDown = (event: KeyboardEvent) => {
    const target = event.target as Element | null;

    if (event.key !== "Escape" || event.defaultPrevented || !peeking.value || target?.closest(".monaco-editor, .CodeMirror")) {
        return;
    }

    event.preventDefault();
    peek.close();
    panel.value?.querySelector<HTMLElement>(`[data-cy=panel-rail-${tab.value}]`)?.focus();
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

const loadSettings = () => props.api.getSettings();

const submitSettings = (form: FormData) => mutate((revision) => props.api.postSettings(revision, form)) as Promise<FormApplyResult>;

const onSettingsApplied = (result: FormApplyResult & { valid: true }) => {
    const applied = result as unknown as SettingsApplied;

    props.store.applyServerChange(applied.revision, applied.issues);
    state.settings = applied.settings;
};

// A record of the journal opens its activity's panel on that run.
const onJournalSelected = (activityId: string, sequence: number) => {
    if (props.store.getNode(activityId)) {
        state.focusedRunSequence = sequence;
        selectNode(props.store, activityId);
        emit("focus-activity", activityId);
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
});

/**
 * Forgets the unapplied changes of the workflow's settings, for example before loading the definition again.
 */
const discardChanges = () => settingsHost.value?.discard();

/**
 * Loads the workflow's settings again, for example after loading the definition again.
 */
const refresh = async () => {
    await nextTick();
    await settingsHost.value?.reload();
};

/**
 * Whether the workflow's settings have changes that weren't applied.
 */
const hasPendingChanges = () => !!settingsHost.value?.isDirty();

defineExpose({ settle, selectTab, expand, discardChanges, refresh, hasPendingChanges });
</script>

<template>
    <aside
        ref="panel"
        class="wfd-panel"
        :class="{ 'is-collapsed': collapsed, 'is-peeking': peeking }"
        :style="collapsed ? undefined : { flexBasis: `${width}px`, width: `${width}px` }"
        :aria-label="t('WorkflowTab')"
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

            <div :id="`${ids}-body`" class="wfd-panel-body" role="tabpanel" :aria-labelledby="`${ids}-tab-${tab}`" @keydown="onBodyKeyDown">
                <VariablesTab v-if="tab === 'variables'" :store="store" :api="api" :read-only="readOnly" :mutate="mutate" @error="onError" />

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
