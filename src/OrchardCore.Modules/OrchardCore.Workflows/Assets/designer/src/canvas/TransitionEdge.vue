<script setup lang="ts">
import { computed } from "vue";
import type { DesignerNode } from "../api/types";
import { edgeGeometry, nodeRect, portAnchor, type NodeLayout } from "./geometry";
import type { ChangeKind } from "./changes";
import { t } from "../i18n";

const props = withDefaults(
    defineProps<{
        edgeKey: string;
        source: DesignerNode;
        target: DesignerNode;
        outcome: string;
        sourceLayout?: NodeLayout;
        targetLayout?: NodeLayout;
        selected?: boolean;
        readOnly?: boolean;
        // How the transition differs from the compared definition, on the compare page.
        change?: ChangeKind | null;
    }>(),
    { sourceLayout: undefined, targetLayout: undefined, selected: false, readOnly: false, change: null },
);

const emit = defineEmits<{
    (event: "select", pointerEvent: PointerEvent): void;
    (event: "delete"): void;
    (event: "open-menu", mouseEvent: MouseEvent): void;
}>();

const outcomeIndex = computed(() => Math.max(0, props.source.outcomes.findIndex((outcome) => outcome.name === props.outcome)));

const label = computed(() => props.source.outcomes.find((outcome) => outcome.name === props.outcome)?.displayName ?? props.outcome);

// Reading the node coordinates here (not in the canvas) means only the edges of a moved node re-render.
const geometry = computed(() =>
    edgeGeometry(
        portAnchor(props.source, props.sourceLayout, props.outcome, outcomeIndex.value, props.source.outcomes.length),
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
        class="wfd-edge"
        :class="[{ 'is-selected': selected }, change ? `is-${change}` : null]"
        :data-cy="`edge-${edgeKey}`"
        :data-edge-key="edgeKey"
        @pointerdown="onPointerDown"
        @contextmenu.prevent.stop="emit('open-menu', $event)"
    >
        <path class="wfd-edge-hit" :d="geometry.path" />
        <path class="wfd-edge-line" :d="geometry.path" :marker-end="selected ? 'url(#wfd-arrow-selected)' : 'url(#wfd-arrow)'" />
        <text class="wfd-edge-label" :x="geometry.mid.x" :y="geometry.mid.y - 6" text-anchor="middle">{{ label }}</text>
        <g
            v-if="selected && !readOnly"
            class="wfd-edge-delete"
            role="button"
            tabindex="0"
            :aria-label="t('DeleteConnection')"
            :transform="`translate(${geometry.mid.x}, ${geometry.mid.y + 10})`"
            data-cy="edge-delete"
            @pointerdown.stop
            @click.stop="emit('delete')"
            @keydown.enter.prevent.stop="emit('delete')"
        >
            <title>{{ t("DeleteConnection") }}</title>
            <circle r="9" />
            <path d="M-3.5,-3.5 L3.5,3.5 M3.5,-3.5 L-3.5,3.5" />
        </g>
    </g>
</template>
