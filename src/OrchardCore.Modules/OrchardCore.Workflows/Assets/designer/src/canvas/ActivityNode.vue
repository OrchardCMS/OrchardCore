<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from "vue";
import type { DesignIssue, DesignerNode } from "../api/types";
import type { NodeLayout } from "./geometry";
import OutcomePort from "./OutcomePort.vue";
import { t } from "../i18n";

const props = withDefaults(
    defineProps<{
        node: DesignerNode;
        selected?: boolean;
        lifted?: boolean;
        dropTarget?: boolean;
        highlighted?: boolean;
        readOnly?: boolean;
        // The instance shown by the viewer waits on this activity.
        blocking?: boolean;
        issues?: DesignIssue[];
        connectedOutcomes?: string[];
        // The number of activities hidden after this collapsed activity, or null when it isn't collapsed.
        hiddenCount?: number | null;
    }>(),
    {
        selected: false,
        lifted: false,
        dropTarget: false,
        highlighted: false,
        readOnly: false,
        blocking: false,
        issues: () => [],
        connectedOutcomes: () => [],
        hiddenCount: null,
    },
);

const emit = defineEmits<{
    (event: "node-pointerdown", pointerEvent: PointerEvent): void;
    (event: "port-pointerdown", outcome: string, pointerEvent: PointerEvent): void;
    (event: "port-activate", outcome: string): void;
    (event: "measure", layout: NodeLayout): void;
    (event: "open-menu", mouseEvent: MouseEvent | null): void;
    (event: "edit"): void;
    (event: "expand"): void;
}>();

const element = ref<HTMLElement | null>(null);

const icon = computed(() => props.node.icon || (props.node.isEvent ? "fa-solid fa-bolt" : "fa-solid fa-gear"));

const errorCount = computed(() => props.issues.filter((issue) => issue.severity === "Error").length);

const issueTitle = computed(() => props.issues.map((issue) => issue.message).join("\n"));

const accessibleName = computed(() => {
    const parts = [props.node.title, props.node.displayText];

    if (props.node.isStart) {
        parts.push(t("StartActivity"));
    }

    if (props.blocking) {
        parts.push(t("BlockingActivity"));
    }

    if (props.issues.length > 0) {
        parts.push(t("IssueCount", props.issues.length));
    }

    if (props.hiddenCount !== null) {
        parts.push(t("HiddenActivitiesAfter", props.hiddenCount));
    }

    return parts.join(", ");
});

// Offsets are layout values (before the canvas transform), so the measurement doesn't depend on the zoom.
const offsetWithin = (child: HTMLElement, ancestor: HTMLElement) => {
    let x = 0;
    let y = 0;
    let current: HTMLElement | null = child;

    while (current && current !== ancestor) {
        x += current.offsetLeft;
        y += current.offsetTop;
        current = current.offsetParent as HTMLElement | null;
    }

    return { x, y };
};

let lastLayout = "";

const measure = () => {
    const root = element.value;

    if (!root) {
        return;
    }

    const ports: NodeLayout["ports"] = {};

    root.querySelectorAll<HTMLElement>("[data-outcome]").forEach((port) => {
        const dot = port.querySelector<HTMLElement>("[data-port-dot]") ?? port;
        const offset = offsetWithin(dot, root);
        ports[port.dataset.outcome!] = { x: offset.x + dot.offsetWidth / 2, y: offset.y + dot.offsetHeight / 2 };
    });

    const layout: NodeLayout = { width: root.offsetWidth, height: root.offsetHeight, ports };
    const serialized = JSON.stringify(layout);

    if (serialized !== lastLayout) {
        lastLayout = serialized;
        emit("measure", layout);
    }
};

let observer: ResizeObserver | null = null;

onMounted(() => {
    measure();

    if (typeof ResizeObserver !== "undefined" && element.value) {
        observer = new ResizeObserver(() => measure());
        observer.observe(element.value);
    }
});

onBeforeUnmount(() => observer?.disconnect());

watch(
    () => [props.node.outcomes, props.node.designHtml, props.node.isStart, props.blocking],
    () => nextTick(measure),
    { deep: true },
);

const onKeyDown = (event: KeyboardEvent) => {
    if (event.target !== element.value) {
        return;
    }

    if (event.key === "Enter" && !props.readOnly) {
        event.preventDefault();
        emit("edit");
    } else if (event.key === "ContextMenu" || (event.key === "F10" && event.shiftKey)) {
        event.preventDefault();
        emit("open-menu", null);
    }
};
</script>

<template>
    <div
        ref="element"
        class="wfd-node"
        :class="{
            'is-event': node.isEvent,
            'is-task': !node.isEvent,
            'is-start': node.isStart,
            'is-missing': node.isMissing,
            'is-selected': selected,
            'is-lifted': lifted,
            'is-drop-target': dropTarget,
            'is-highlighted': highlighted,
            'is-blocking': blocking,
            'is-collapsed': hiddenCount !== null,
            'has-errors': errorCount > 0,
        }"
        :style="{ transform: `translate(${node.x}px, ${node.y}px)` }"
        role="group"
        tabindex="0"
        :aria-label="accessibleName"
        :aria-roledescription="t('Activity')"
        :data-node-id="node.id"
        :data-cy="`activity-${node.id}`"
        :data-x="node.x"
        :data-y="node.y"
        @pointerdown="emit('node-pointerdown', $event)"
        @contextmenu.prevent.stop="emit('open-menu', $event)"
        @dblclick.stop="!readOnly && emit('edit')"
        @keydown="onKeyDown"
    >
        <div class="wfd-node-header">
            <i :class="icon" aria-hidden="true"></i>
            <span class="wfd-node-type text-truncate">{{ node.displayText }}</span>
            <span v-if="node.isStart" class="badge text-bg-success wfd-node-badge" data-cy="start-badge">{{ t("Start") }}</span>
            <span v-if="blocking" class="badge text-bg-info wfd-node-badge" :title="t('BlockingActivityHint')" data-cy="blocking-badge">
                <i class="fa-solid fa-hourglass-half" aria-hidden="true"></i>
                {{ t("Blocking") }}
            </span>
            <span
                v-if="issues.length > 0"
                class="badge wfd-node-badge"
                :class="errorCount > 0 ? 'text-bg-danger' : 'text-bg-warning'"
                :title="issueTitle"
                data-cy="issue-badge"
            >
                <i class="fa-solid fa-triangle-exclamation" aria-hidden="true"></i>
                {{ issues.length }}
            </span>
        </div>
        <div class="wfd-node-content">
            <div class="wfd-node-body" v-html="node.designHtml"></div>
            <div v-if="node.outcomes.length > 0" class="wfd-node-ports">
                <OutcomePort
                    v-for="outcome in node.outcomes"
                    :key="outcome.name"
                    :activity-id="node.id"
                    :outcome="outcome"
                    :read-only="readOnly"
                    :connected="connectedOutcomes.includes(outcome.name)"
                    @start="emit('port-pointerdown', outcome.name, $event)"
                    @activate="emit('port-activate', outcome.name)"
                />
            </div>
        </div>
        <button
            v-if="hiddenCount !== null"
            type="button"
            class="wfd-node-expand badge rounded-pill text-bg-primary"
            :title="t('ExpandActivities', hiddenCount)"
            :aria-label="t('ExpandActivities', hiddenCount)"
            :data-cy="`expand-${node.id}`"
            @pointerdown.stop
            @dblclick.stop
            @click.stop="emit('expand')"
        >
            <i class="fa-solid fa-plus" aria-hidden="true"></i>
            {{ hiddenCount }}
        </button>
    </div>
</template>
