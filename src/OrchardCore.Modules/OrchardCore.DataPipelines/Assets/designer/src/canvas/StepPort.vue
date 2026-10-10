<script setup lang="ts">
import { computed } from "vue";
import type { DesignerPort } from "../api/types";
import type { PortSide } from "./geometry";
import { t } from "../i18n";

// A port of a step: inputs on its left edge, outputs on its right edge. Record ports are circles, file ports squares.
// Dragging from an output draws a connection; a click on an unconnected output offers the steps to add after it.
const props = withDefaults(
    defineProps<{
        stepId: string;
        port: DesignerPort;
        side: PortSide;
        // Whether the label is shown: only when the side has several ports.
        showLabel: boolean;
        readOnly: boolean;
        connected: boolean;
        // While a connection is dragged: whether this input can receive it (true), can't (false), or isn't concerned (null).
        compatible?: boolean | null;
        // The input under the pointer while a connection is dragged.
        hovered?: boolean;
    }>(),
    { compatible: null, hovered: false },
);

const emit = defineEmits<{
    (event: "start", pointerEvent: PointerEvent): void;
    (event: "activate"): void;
}>();

const kindLabel = computed(() => (props.port.kind === "Files" ? t("FilesPort", "Files") : t("RecordsPort", "Rows")));

const title = computed(() => {
    const parts = [`${props.port.displayName} (${kindLabel.value})`];

    if (props.side === "input") {
        if (props.port.isRequired) {
            parts.push(t("RequiredInput", "Required"));
        }

        parts.push(props.port.allowsMany ? t("AcceptsSeveral", "Accepts several connections") : t("AcceptsOne", "Accepts one connection"));
    } else if (!props.readOnly) {
        parts.push(props.connected ? t("DragToConnect", "Drag to connect it to another step") : t("ClickToAdd", "Click to add the next step, or drag to connect"));
    }

    return parts.join(" · ");
});

const accessibleName = computed(() =>
    props.side === "output" ? t("ConnectOutput", "Connect the output {0}", props.port.displayName) : t("InputPort", "Input {0}", props.port.displayName),
);

const onPointerDown = (event: PointerEvent) => {
    // A port starts a connection, never a node drag.
    event.stopPropagation();

    if (props.side === "output" && !props.readOnly && event.button === 0) {
        event.preventDefault();
        emit("start", event);
    }
};
</script>

<template>
    <button
        v-if="side === 'output'"
        type="button"
        class="dpd-port is-output"
        :class="[`is-${port.kind.toLowerCase()}`, { 'is-connected': connected, 'has-label': showLabel }]"
        :disabled="readOnly"
        data-port-side="output"
        :data-port="port.name"
        :data-cy="`port-${stepId}-out-${port.name}`"
        :aria-label="accessibleName"
        :title="title"
        @pointerdown="onPointerDown"
        @click.stop
        @keydown.enter.prevent.stop="emit('activate')"
        @keydown.space.prevent.stop="emit('activate')"
    >
        <span v-if="showLabel" class="dpd-port-label">{{ port.displayName }}</span>
        <span class="dpd-port-dot" data-port-dot aria-hidden="true"></span>
    </button>
    <span
        v-else
        class="dpd-port is-input"
        :class="[
            `is-${port.kind.toLowerCase()}`,
            {
                'is-connected': connected,
                'is-required': port.isRequired,
                'has-label': showLabel,
                'is-compatible': compatible === true,
                'is-incompatible': compatible === false,
                'is-hovered': hovered,
            },
        ]"
        data-port-side="input"
        :data-port="port.name"
        :data-cy="`port-${stepId}-in-${port.name}`"
        :title="title"
    >
        <span class="dpd-port-dot" data-port-dot aria-hidden="true"></span>
        <span v-if="showLabel" class="dpd-port-label">{{ port.displayName }}</span>
        <span class="visually-hidden">{{ accessibleName }}</span>
    </span>
</template>
