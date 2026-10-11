<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from "vue";
import type { DesignIssue, DesignerNode } from "../api/types";
import type { NodeLayout } from "./geometry";
import StepPort from "./StepPort.vue";
import { stepIcon } from "../toolbox/filter";
import { stepStatusLabel, type StepOverlay } from "../runs/runOverlay";
import { t } from "../i18n";

const props = withDefaults(
    defineProps<{
        node: DesignerNode;
        selected?: boolean;
        lifted?: boolean;
        dropTarget?: boolean;
        readOnly?: boolean;
        issues?: DesignIssue[];
        connectedInputs?: string[];
        connectedOutputs?: string[];
        // While a connection is dragged: which inputs can receive it, by name (null when no connection is dragged).
        compatibility?: Record<string, boolean> | null;
        hoveredInput?: string | null;
        // The status and counts of the step in the run selected in the Runs tab.
        overlay?: StepOverlay | null;
    }>(),
    {
        selected: false,
        lifted: false,
        dropTarget: false,
        readOnly: false,
        issues: () => [],
        connectedInputs: () => [],
        connectedOutputs: () => [],
        compatibility: null,
        hoveredInput: null,
        overlay: null,
    },
);

const emit = defineEmits<{
    (event: "node-pointerdown", pointerEvent: PointerEvent): void;
    (event: "port-pointerdown", port: string, pointerEvent: PointerEvent): void;
    (event: "port-activate", port: string): void;
    (event: "measure", layout: NodeLayout): void;
    (event: "open-menu", mouseEvent: MouseEvent | null): void;
    (event: "edit"): void;
}>();

const element = ref<HTMLElement | null>(null);

const icon = computed(() => stepIcon(props.node));

const errorCount = computed(() => props.issues.filter((issue) => issue.severity === "Error").length);

const issueTitle = computed(() => props.issues.map((issue) => issue.message).join("\n"));

const categoryLabel = computed(
    () =>
        ({
            Source: () => t("CategorySource", "Source"),
            Transform: () => t("CategoryTransform", "Transform"),
            File: () => t("CategoryFile", "File"),
            Destination: () => t("CategoryDestination", "Destination"),
        })[props.node.category]?.() ?? props.node.category,
);

const accessibleName = computed(() => {
    const parts = [props.node.displayText, categoryLabel.value];

    if (props.node.isMissing) {
        parts.push(t("MissingStep", "This step is not available: its feature is disabled."));
    }

    if (props.overlay) {
        parts.push(stepStatusLabel(props.overlay.status));

        if (props.overlay.label) {
            parts.push(props.overlay.label);
        }
    }

    if (props.issues.length > 0) {
        parts.push(t("IssueCount", "{0} issues", props.issues.length));
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

    const layout: NodeLayout = { width: root.offsetWidth, height: root.offsetHeight, inputs: {}, outputs: {} };

    root.querySelectorAll<HTMLElement>("[data-port-side]").forEach((port) => {
        const dot = port.querySelector<HTMLElement>("[data-port-dot]") ?? port;
        const offset = offsetWithin(dot, root);
        const anchors = port.dataset.portSide === "input" ? layout.inputs : layout.outputs;

        anchors[port.dataset.port!] = { x: offset.x + dot.offsetWidth / 2, y: offset.y + dot.offsetHeight / 2 };
    });

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
    () => [props.node.inputs, props.node.outputs, props.node.designHtml, props.node.displayText, props.overlay?.label],
    () => nextTick(measure),
    { deep: true },
);

const onKeyDown = (event: KeyboardEvent) => {
    if (event.target !== element.value) {
        return;
    }

    if (event.key === "Enter") {
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
        class="dpd-node"
        :class="[
            `is-${node.category.toLowerCase()}`,
            overlay ? `is-run-${overlay.status.toLowerCase()}` : null,
            {
                'is-missing': node.isMissing,
                'is-selected': selected,
                'is-lifted': lifted,
                'is-drop-target': dropTarget,
                'has-errors': errorCount > 0,
            },
        ]"
        :style="{ transform: `translate(${node.x}px, ${node.y}px)` }"
        role="group"
        tabindex="0"
        :aria-label="accessibleName"
        :aria-roledescription="t('Step', 'Step')"
        :data-node-id="node.id"
        :data-cy="`step-${node.id}`"
        :data-x="node.x"
        :data-y="node.y"
        @pointerdown="emit('node-pointerdown', $event)"
        @contextmenu.prevent.stop="emit('open-menu', $event)"
        @dblclick.stop="emit('edit')"
        @keydown="onKeyDown"
    >
        <div class="dpd-node-header">
            <i class="fa-fw" :class="icon" aria-hidden="true"></i>
            <span class="dpd-node-title text-truncate">{{ node.displayText }}</span>
            <span
                v-if="issues.length > 0"
                class="badge dpd-node-badge"
                :class="errorCount > 0 ? 'text-bg-danger' : 'text-bg-warning'"
                :title="issueTitle"
                data-cy="issue-badge"
            >
                <i class="fa-solid fa-triangle-exclamation" aria-hidden="true"></i>
                {{ issues.length }}
            </span>
            <!-- A click on the step only selects it, to move it around; this opens its panel, as a double-click does. -->
            <button
                type="button"
                class="dpd-node-edit"
                :title="readOnly ? t('OpenStepDetails', 'Show the step') : t('OpenStepSettings', 'Open the settings')"
                :aria-label="readOnly ? t('OpenStepDetailsOf', 'Show the step {0}', node.displayText) : t('OpenStepSettingsOf', 'Open the settings of {0}', node.displayText)"
                data-cy="node-edit"
                @pointerdown.stop
                @dblclick.stop
                @click.stop="emit('edit')"
            >
                <i class="fa-solid" :class="readOnly ? 'fa-table' : 'fa-sliders'" aria-hidden="true"></i>
            </button>
        </div>
        <div class="dpd-node-content">
            <div v-if="node.inputs.length > 0" class="dpd-node-ports is-inputs">
                <StepPort
                    v-for="port in node.inputs"
                    :key="port.name"
                    :step-id="node.id"
                    :port="port"
                    side="input"
                    :show-label="node.inputs.length > 1"
                    :read-only="readOnly"
                    :connected="connectedInputs.includes(port.name)"
                    :compatible="compatibility ? (compatibility[port.name] ?? false) : null"
                    :hovered="hoveredInput === port.name"
                />
            </div>
            <div class="dpd-node-body">
                <div class="dpd-node-summary" v-html="node.designHtml"></div>
                <p v-if="node.isMissing" class="dpd-node-missing">{{ t("MissingStepShort", "Not available") }}</p>
                <span v-if="overlay?.label" class="badge dpd-run-badge" :title="overlay.title" data-cy="run-badge">{{ overlay.label }}</span>
                <span v-else-if="overlay" class="badge dpd-run-badge" :title="overlay.title" data-cy="run-badge">{{ stepStatusLabel(overlay.status) }}</span>
            </div>
            <div v-if="node.outputs.length > 0" class="dpd-node-ports is-outputs">
                <StepPort
                    v-for="port in node.outputs"
                    :key="port.name"
                    :step-id="node.id"
                    :port="port"
                    side="output"
                    :show-label="node.outputs.length > 1"
                    :read-only="readOnly"
                    :connected="connectedOutputs.includes(port.name)"
                    @start="emit('port-pointerdown', port.name, $event)"
                    @activate="emit('port-activate', port.name)"
                />
            </div>
        </div>
    </div>
</template>
