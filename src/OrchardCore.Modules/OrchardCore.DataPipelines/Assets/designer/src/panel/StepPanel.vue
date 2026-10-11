<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, useId, watch } from "vue";
import type { DesignerApi } from "../api/designerApi";
import { DesignerApiError } from "../api/designerApi";
import type { EditorApplied } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { clearSelection, showStep } from "../canvas/useConnect";
import { stepIcon } from "../toolbox/filter";
import ServerFormHost from "./ServerFormHost.vue";
import PreviewTab from "./PreviewTab.vue";
import FieldsTab from "./FieldsTab.vue";
import type { FormApplyResult } from "./types";
import type { RevisionTask } from "../services/revisionQueue";
import { showToast } from "../ui/toasts";
import { confirmAction } from "../ui/confirm";
import { readPreference, writePreference } from "../ui/preferences";
import { t } from "../i18n";

// The tabs of the selected step: its settings (a server-rendered editor), a preview of its data, and its fields.
type Tab = "settings" | "data" | "fields";

const MIN_HEIGHT = 128;
// The canvas keeps at least this much room above the panel.
const CANVAS_MIN_HEIGHT = 96;
const HEIGHT_KEY = "step-panel-height";
const PINNED_KEY = "step-panel-pinned";

const TAB_ICONS: Record<Tab, string> = {
    settings: "fa-solid fa-sliders",
    data: "fa-solid fa-table",
    fields: "fa-solid fa-list",
};

const props = withDefaults(
    defineProps<{
        store: DesignerStore;
        api: DesignerApi;
        readOnly?: boolean;
        canPreview?: boolean;
        /**
         * Runs a request that changes the draft, in the designer's revision queue.
         */
        mutate?: <T>(task: RevisionTask<T>) => Promise<T>;
        /**
         * Applies the open forms and saves the graph before a preview; resolves to false when that failed.
         */
        prepare?: () => Promise<boolean>;
    }>(),
    {
        readOnly: false,
        canPreview: false,
        mutate: undefined,
        prepare: () => Promise.resolve(true),
    },
);

const mutate = <T,>(task: RevisionTask<T>) => (props.mutate ? props.mutate(task) : task(props.store.state.revision));

const emit = defineEmits<{
    (event: "return-focus", stepId: string): void;
    (event: "closed"): void;
    (event: "delete", stepId: string): void;
    (event: "conflict", error: DesignerApiError): void;
}>();

const state = props.store.state;
const editingId = ref<string | null>(null);
const tab = ref<Tab>(props.readOnly ? "fields" : "settings");
// Pinned, the panel stays open below the canvas; otherwise it opens over the canvas while a step is selected.
const pinned = ref(readPreference(PINNED_KEY) === "true");
// Until it is resized, the panel takes a part of the designer's height (see the stylesheet).
const height = ref<number | null>(null);
const stepHost = ref<InstanceType<typeof ServerFormHost> | null>(null);
const previewTab = ref<InstanceType<typeof PreviewTab> | null>(null);
const panel = ref<HTMLElement | null>(null);
const body = ref<HTMLElement | null>(null);
const tabList = ref<HTMLElement | null>(null);
const ids = `dpd-step-${useId()}`;
// Increments when the step's settings are applied, so the Fields tab loads them again.
const fieldsVersion = ref(0);

const selectedId = computed(() => (state.selectedNodeIds.length === 1 ? state.selectedNodeIds[0] : null));
const editingNode = computed(() => (editingId.value ? props.store.getNode(editingId.value) : undefined));
// Unpinned, it opens when the selected step's panel is asked for (see stepPanelRequested).
const isOpen = computed(() => pinned.value || (!!editingNode.value && state.stepPanelRequested));

const canEdit = computed(() => !props.readOnly && !!editingNode.value && editingNode.value.hasEditor && !editingNode.value.isMissing);

const tabs = computed<Tab[]>(() => {
    if (!editingNode.value) {
        return [];
    }

    return [...(canEdit.value ? (["settings"] as Tab[]) : []), ...(props.canPreview && !editingNode.value.isMissing ? (["data"] as Tab[]) : []), "fields"];
});

const tabLabel = (item: Tab) => ({ settings: t("StepSettings", "Settings"), data: t("StepData", "Data"), fields: t("StepFields", "Fields") })[item];

const tabHint = (item: Tab) =>
    ({
        settings: t("StepSettingsHint", "The settings of the step. They are applied as you change them."),
        data: t("StepDataHint", "Preview the first rows the step produces."),
        fields: t("StepFieldsHint", "The fields the step receives and produces."),
    })[item];

const defaultTab = (): Tab => tabs.value[0] ?? "fields";

// Set by open({ focus: true }): the first field of the step editor gets the focus once it is loaded.
let focusOnLoad = false;

watch(editingId, () => {
    tab.value = defaultTab();
});

// A step whose editor or preview isn't available no longer has that tab.
watch(tabs, (next) => {
    if (next.length > 0 && !next.includes(tab.value)) {
        tab.value = next[0];
    }
});

/**
 * Applies the pending changes of the step's settings. When they are invalid, asks whether to discard them;
 * resolves to false when the user wants to keep editing.
 */
const settle = async () => {
    const host = stepHost.value;

    if (!host || (await host.apply()) || !host.isInvalid()) {
        return true;
    }

    const discard = await confirmAction({
        title: t("DiscardChangesTitle", "Discard the changes?"),
        message: t("DiscardChangesMessage", "The form has errors, so its changes weren't applied."),
        okText: t("Discard", "Discard"),
        cancelText: t("KeepEditing", "Keep editing"),
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
        // Keep the invalid step selected, and its panel open, so its errors stay visible.
        showStep(props.store, editingId.value);

        return;
    }

    editingId.value = next;
});

// The edited step was deleted (or undone away).
watch(editingNode, (node) => {
    if (!node && editingId.value) {
        stepHost.value?.discard();
        editingId.value = selectedId.value;
    }
});

const setPinned = (value: boolean) => {
    pinned.value = value;
    writePreference(PINNED_KEY, String(value));
};

/**
 * Closes the panel opened over the canvas: the step's changes are applied, and it is no longer selected.
 */
const close = async () => {
    if (await settle()) {
        clearSelection(props.store);
        emit("closed");
    }
};

/**
 * Deletes the step shown, which closes the panel; the canvas offers to undo it. Its unapplied changes are dropped
 * rather than posted for a step that no longer exists.
 */
const deleteStep = () => {
    if (editingId.value) {
        stepHost.value?.discard();
        emit("delete", editingId.value);
    }
};

const isVisible = (element: HTMLElement) => (typeof element.checkVisibility === "function" ? element.checkVisibility() : true);

const FIELDS = ".dpd-form-content input:not([type=hidden]), .dpd-form-content select, .dpd-form-content textarea, .dpd-form-content button";

/**
 * Moves the focus to the first field of the step's editor, or to the panel's title.
 */
const focusFirstField = () => {
    const field = Array.from(body.value?.querySelectorAll<HTMLElement>(FIELDS) ?? []).find((element) => !element.hasAttribute("disabled") && isVisible(element));

    (field ?? panel.value?.querySelector<HTMLElement>(".dpd-panel-title"))?.focus();
};

/**
 * Shows the panel of a step, for example after a double-click or adding it from the toolbox, on `tab` (by default its
 * first one).
 */
const open = async (stepId: string, options: { focus?: boolean; tab?: Tab } = {}) => {
    const loaded = editingId.value === stepId;

    focusOnLoad = !!options.focus;
    showStep(props.store, stepId);
    await nextTick();

    const next = options.tab && tabs.value.includes(options.tab) ? options.tab : defaultTab();
    tab.value = next;

    // The editor of this step is already open, so it won't load again.
    if ((loaded || next !== "settings") && focusOnLoad) {
        focusOnLoad = false;
        await nextTick();
        focusFirstField();
    }
};

/**
 * Opens the Data tab of a step and previews it.
 */
const preview = async (stepId: string) => {
    await open(stepId, { tab: "data" });
    await nextTick();
    await previewTab.value?.run();
};

const onStepLoaded = () => {
    if (focusOnLoad) {
        focusOnLoad = false;
        focusFirstField();
    }
};

// Escape goes back to the step on the canvas, unless a rich editor uses it (suggestions, search).
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
    tabList.value?.querySelector<HTMLElement>(`[data-cy=step-tab-${tab.value}]`)?.focus();
};

const onError = (error: unknown) => {
    if (error instanceof DesignerApiError && error.isConflict) {
        emit("conflict", error);

        return;
    }

    showToast({ message: t("ApplyFailed", "The changes could not be applied."), variant: "danger" });
};

const loadStep = () => props.api.getEditor(editingId.value!);

const submitStep = (form: FormData) => {
    // The request may wait in the queue, so it keeps the step being edited now.
    const stepId = editingId.value!;

    return mutate((revision) => props.api.postEditor(stepId, revision, form)) as Promise<FormApplyResult>;
};

const onStepApplied = (result: FormApplyResult & { valid: true }) => {
    const applied = result as unknown as EditorApplied;

    props.store.applyServerChange(applied.revision, applied.issues);
    const removed = props.store.replaceNode(applied.node, applied.removedConnections ?? []);
    fieldsVersion.value++;

    if (removed > 0) {
        showToast({ message: t("ConnectionsRemoved", "{0} connections were removed: the step no longer has their ports.", removed), variant: "warning" });
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
 * Forgets the unapplied changes of the step's settings, for example before loading the definition again.
 */
const discardChanges = () => stepHost.value?.discard();

/**
 * Loads the step's settings and fields again, for example after loading the definition again.
 */
const refresh = async () => {
    await nextTick();
    fieldsVersion.value++;
    await stepHost.value?.reload();
};

/**
 * Whether the step's settings have changes that weren't applied.
 */
const hasPendingChanges = () => !!stepHost.value?.isDirty();

/**
 * The height of the canvas the panel covers, so the canvas can keep its controls above it.
 */
const coveredHeight = computed(() => (isOpen.value && !pinned.value ? renderedHeight.value : 0));

defineExpose({ open, preview, settle, close, discardChanges, refresh, hasPendingChanges, isOpen, pinned, coveredHeight });
</script>

<template>
    <section
        v-show="isOpen"
        ref="panel"
        class="dpd-step-panel"
        :class="{ 'is-pinned': pinned }"
        :style="height !== null ? { height: `${height}px` } : undefined"
        :aria-label="t('StepPanel', 'Step')"
        data-cy="step-panel"
        @keydown="onKeyDown"
    >
        <div
            class="dpd-step-panel-resizer"
            role="separator"
            aria-orientation="horizontal"
            :aria-label="t('ResizeStepPanel', 'Resize the step panel')"
            :aria-valuenow="height ?? undefined"
            :aria-valuemin="MIN_HEIGHT"
            tabindex="0"
            data-cy="step-panel-resizer"
            @pointerdown="startResize"
            @keydown="onResizerKeyDown"
        ></div>

        <div class="dpd-step-panel-header">
            <h3 v-if="editingNode" class="dpd-panel-title dpd-step-panel-title h6" tabindex="-1" data-cy="panel-step-title">
                <i class="fa-fw" :class="stepIcon(editingNode)" aria-hidden="true"></i>
                <span class="text-truncate">{{ editingNode.displayText }}</span>
            </h3>
            <h3 v-else class="dpd-panel-title dpd-step-panel-title h6" tabindex="-1">
                <i class="fa-solid fa-sliders" aria-hidden="true"></i>
                {{ t("StepPanel", "Step") }}
            </h3>

            <ul v-if="tabs.length > 0" ref="tabList" class="nav nav-tabs dpd-panel-tabs" role="tablist" :aria-label="t('StepPanel', 'Step')" @keydown="onTabKeyDown">
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
                        :data-cy="`step-tab-${item}`"
                        @click="tab = item"
                    >
                        <i :class="TAB_ICONS[item]" aria-hidden="true"></i>
                        <span class="dpd-step-tab-label">{{ tabLabel(item) }}</span>
                    </button>
                </li>
            </ul>

            <div class="dpd-step-panel-actions">
                <button
                    v-if="editingNode && !readOnly"
                    type="button"
                    class="btn btn-sm btn-link link-danger"
                    :title="t('DeleteStep', 'Delete the step')"
                    :aria-label="t('DeleteStep', 'Delete the step')"
                    data-cy="step-panel-delete"
                    @click="deleteStep"
                >
                    <i class="fa-solid fa-trash" aria-hidden="true"></i>
                </button>
                <button
                    type="button"
                    class="btn btn-sm btn-link"
                    :class="{ active: pinned }"
                    :title="pinned ? t('UnpinStepPanel', 'Unpin the panel') : t('PinStepPanel', 'Pin the panel below the canvas')"
                    :aria-label="pinned ? t('UnpinStepPanel', 'Unpin the panel') : t('PinStepPanel', 'Pin the panel below the canvas')"
                    :aria-pressed="pinned"
                    data-cy="step-panel-pin"
                    @click="setPinned(!pinned)"
                >
                    <i class="fa-solid fa-thumbtack" :class="{ 'fa-rotate-90': !pinned }" aria-hidden="true"></i>
                </button>
                <button
                    v-if="!pinned"
                    type="button"
                    class="btn btn-sm btn-link"
                    :title="t('CloseStepPanel', 'Close the panel')"
                    :aria-label="t('CloseStepPanel', 'Close the panel')"
                    data-cy="step-panel-close"
                    @click="close"
                >
                    <i class="fa-solid fa-xmark" aria-hidden="true"></i>
                </button>
            </div>
        </div>

        <div ref="body" class="dpd-step-panel-body">
            <p v-if="state.selectedNodeIds.length > 1" class="dpd-panel-message" data-cy="panel-multiple">
                {{ t("MultipleSelected", "{0} steps are selected.", state.selectedNodeIds.length) }}
            </p>
            <p v-else-if="!editingNode" class="dpd-panel-message" data-cy="panel-empty">{{ t("SelectStep", "Select a step to see its settings and its data.") }}</p>

            <template v-else>
                <p v-if="editingNode.isMissing" class="alert alert-warning m-2 py-2" data-cy="panel-missing">
                    {{ t("MissingStep", "This step is not available: its feature is disabled.") }}
                </p>

                <!-- Hidden rather than removed, so the editor keeps its changes and its cursor. -->
                <div v-if="canEdit" v-show="tab === 'settings'" :id="`${ids}-tab-settings-panel`" role="tabpanel" :aria-labelledby="`${ids}-tab-settings`">
                    <ServerFormHost
                        ref="stepHost"
                        :form-key="editingNode.id"
                        :load="loadStep"
                        :submit="submitStep"
                        :label="t('StepSettings', 'Settings')"
                        data-cy="panel-step-form"
                        @loaded="onStepLoaded"
                        @applied="onStepApplied"
                        @error="onError"
                    />
                </div>
                <div v-if="tabs.includes('data')" v-show="tab === 'data'" :id="`${ids}-tab-data-panel`" role="tabpanel" :aria-labelledby="`${ids}-tab-data`">
                    <PreviewTab :key="editingNode.id" ref="previewTab" :step-id="editingNode.id" :api="api" :prepare="prepare" />
                </div>
                <div v-if="tab === 'fields'" :id="`${ids}-tab-fields-panel`" role="tabpanel" :aria-labelledby="`${ids}-tab-fields`">
                    <FieldsTab :step-id="editingNode.id" :api="api" :refresh-key="fieldsVersion" />
                </div>
            </template>
        </div>
    </section>
</template>
