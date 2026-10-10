<script setup lang="ts">
import { computed } from "vue";
import type { DesignerConnection, DesignerNode } from "../api/types";
import { edgeGeometry, nodeRect, portAnchor, type NodeLayout } from "./geometry";
import { t } from "../i18n";

const props = withDefaults(
    defineProps<{
        edgeKey: string;
        connection: DesignerConnection;
        source: DesignerNode;
        target: DesignerNode;
        sourceLayout?: NodeLayout;
        targetLayout?: NodeLayout;
        selected?: boolean;
        readOnly?: boolean;
    }>(),
    { sourceLayout: undefined, targetLayout: undefined, selected: false, readOnly: false },
);

const emit = defineEmits<{
    (event: "select", pointerEvent: PointerEvent): void;
    (event: "delete"): void;
    (event: "open-menu", mouseEvent: MouseEvent): void;
}>();

const outputIndex = computed(() => Math.max(0, props.source.outputs.findIndex((port) => port.name === props.connection.sourcePort)));
const inputIndex = computed(() => Math.max(0, props.target.inputs.findIndex((port) => port.name === props.connection.targetPort)));
const output = computed(() => props.source.outputs[outputIndex.value]);

const kind = computed(() => (output.value?.kind ?? "Records").toLowerCase());

// The port's name is shown when the step has several outputs (such as Matched and Unmatched).
const label = computed(() => (props.source.outputs.length > 1 ? (output.value?.displayName ?? props.connection.sourcePort) : ""));

// Reading the node coordinates here (not in the canvas) means only the connections of a moved step re-render.
const geometry = computed(() =>
    edgeGeometry(
        portAnchor(props.source, props.sourceLayout, "output", props.connection.sourcePort, outputIndex.value, props.source.outputs.length),
        portAnchor(props.target, props.targetLayout, "input", props.connection.targetPort, inputIndex.value, props.target.inputs.length),
        nodeRect(props.source, props.sourceLayout),
        nodeRect(props.target, props.targetLayout),
    ),
);

const onPointerDown = (event: PointerEvent) => {
    event.stopPropagation();

    if (event.button === 0) {
        emit("select", event);
    }
};
</script>

<template>
    <g
        class="dpd-edge"
        :class="[`is-${kind}`, { 'is-selected': selected }]"
        :data-cy="`edge-${edgeKey}`"
        :data-edge-key="edgeKey"
        @pointerdown="onPointerDown"
        @contextmenu.prevent.stop="emit('open-menu', $event)"
    >
        <path class="dpd-edge-hit" :d="geometry.path" />
        <path class="dpd-edge-line" :d="geometry.path" :marker-end="selected ? 'url(#dpd-arrow-selected)' : 'url(#dpd-arrow)'" />
        <text v-if="label" class="dpd-edge-label" :x="geometry.mid.x" :y="geometry.mid.y - 6" text-anchor="middle">{{ label }}</text>
        <g
            v-if="selected && !readOnly"
            class="dpd-edge-delete"
            role="button"
            tabindex="0"
            :aria-label="t('DeleteConnection', 'Delete the connection')"
            :transform="`translate(${geometry.mid.x}, ${geometry.mid.y + 10})`"
            data-cy="edge-delete"
            @pointerdown.stop
            @click.stop="emit('delete')"
            @keydown.enter.prevent.stop="emit('delete')"
        >
            <title>{{ t("DeleteConnection", "Delete the connection") }}</title>
            <circle r="9" />
            <path d="M-3.5,-3.5 L3.5,3.5 M3.5,-3.5 L-3.5,3.5" />
        </g>
    </g>
</template>
