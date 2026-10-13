<script setup lang="ts" generic="TNode extends GraphNode, TEdge, TLayout extends NodeSize">
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref, shallowReactive } from "vue";
import type { GraphNode } from "../graph/model";
import ContextMenu from "./ContextMenu.vue";
import type { MenuItem } from "./menu";
import { GRID_SIZE, boundsOf, rectFromPoints, rectsIntersect, snap, type Point, type Rect } from "./geometry";
import { ZOOM_STEP, canvasToScreen, fitToContent, screenToCanvas, visibleCenter, zoomAt, zoomBy, type Size, type ViewportState } from "./viewport";
import { startPointerDrag } from "./useDrag";
import type { NodeMove } from "../graph/commands";
import type { CanvasEdge, ConnectTarget, Connecting, NodeSize } from "./canvasTypes";
import { t } from "../core/i18n";

// The surface of a designer canvas: the grid, the pan and zoom, the marquee, moving nodes, drawing a connection from a
// port, the keyboard, dropping from the toolbox and the context menu. The designer renders its nodes and edges through
// the slots, and changes its graph when this component asks to (the events).

// Dragged nodes and their edges get their own compositing layers while they move, so the rest of the canvas isn't
// re-rasterized every frame. Beyond this many nodes the extra layers cost more than they save.
const LIFT_LIMIT = 10;

const props = withDefaults(
    defineProps<{
        // The nodes and edges to show, the edges with their two nodes.
        nodes: TNode[];
        edges: CanvasEdge<TNode, TEdge>[];
        selectedIds: string[];
        viewport: ViewportState;
        // The rectangle of a node, from its position and its measured size.
        nodeRect: (node: TNode, layout: TLayout | undefined) => Rect;
        // Where the connection drawn from an output starts.
        portAnchor: (node: TNode, layout: TLayout | undefined, port: string) => Point;
        // The path of the connection being drawn, from the port to the pointer.
        previewPath: (from: Point, to: Point) => string;
        // Where a connection from an output dropped on a node goes: the port, and whether it can go there.
        resolveTarget?: (sourceId: string, port: string, target: TNode, targetPort: string | null) => ConnectTarget;
        // The data type of the toolbox items dropped on the canvas.
        dragType?: string | null;
        // The elements in which the canvas leaves the keys alone, such as its dialogs.
        keyboardIgnore?: string;
        label: string;
        readOnly?: boolean;
    }>(),
    {
        resolveTarget: (_sourceId: string, _port: string, _target: unknown, targetPort: string | null) => ({ port: targetPort ?? "", valid: true }),
        dragType: null,
        keyboardIgnore: "",
        readOnly: false,
    },
);

const emit = defineEmits<{
    // Selection: `additive` adds the node to the selection; `pointer` tells a click from a focus.
    (event: "select-node", id: string, additive: boolean, pointer: boolean): void;
    (event: "select-nodes", ids: string[]): void;
    (event: "select-all"): void;
    (event: "clear-selection"): void;
    // A click on a node of the read-only canvas.
    (event: "show-node", id: string): void;
    (event: "drag-start", ids: string[]): void;
    (event: "move-nodes", moves: NodeMove[], coalesceKey: string): void;
    (event: "nudge", dx: number, dy: number): void;
    (event: "delete-selection"): void;
    // A connection drawn from an output to a node; `targetPort` is null when the node has no input for it.
    (event: "connect", sourceId: string, port: string, targetId: string, targetPort: string | null): void;
    // A click on an output, without drawing.
    (event: "port-click", nodeId: string, port: string, clientX: number, clientY: number): void;
    (event: "drop", value: string, point: Point): void;
}>();

defineSlots<{
    // An edge, `lifted` while one of its nodes is dragged.
    edge(props: { edge: CanvasEdge<TNode, TEdge>; lifted: boolean; sourceLayout: TLayout | undefined; targetLayout: TLayout | undefined }): unknown;
    node(props: {
        node: TNode;
        selected: boolean;
        lifted: boolean;
        // Whether a connection being drawn would go to this node.
        dropTarget: boolean;
        connecting: Connecting | null;
        onNodePointerdown: (event: PointerEvent) => void;
        onPortPointerdown: (port: string, event: PointerEvent) => void;
        onMeasure: (layout: TLayout) => void;
        onFocus: () => void;
    }): unknown;
    // What is shown above the canvas: its empty message, notices, dialogs.
    default?(): unknown;
}>();

const viewport = props.viewport;
const host = ref<HTMLElement | null>(null);
const hostSize = reactive<Size>({ width: 0, height: 0 });
const layouts = shallowReactive(new Map<string, TLayout>());
const liftedIds = ref(new Set<string>());
const panning = ref(false);
const zooming = ref(false);
const spaceHeld = ref(false);
const marquee = ref<Rect | null>(null);
const connecting = ref<Connecting | null>(null);
const menu = ref<{ x: number; y: number; items: MenuItem[]; label: string } | null>(null);

let dragCounter = 0;
let zoomTimer: ReturnType<typeof setTimeout> | undefined;
let pointerFocus = false;

const nodeById = computed(() => new Map(props.nodes.map((node) => [node.id, node])));
const selected = computed(() => new Set(props.selectedIds));

const isLiftedEdge = (edge: CanvasEdge<TNode, TEdge>) => liftedIds.value.has(edge.source.id) || liftedIds.value.has(edge.target.id);
const staticEdges = computed(() => (liftedIds.value.size === 0 ? props.edges : props.edges.filter((edge) => !isLiftedEdge(edge))));
const liftedEdges = computed(() => (liftedIds.value.size === 0 ? [] : props.edges.filter(isLiftedEdge)));

const layerStyle = computed(() => ({
    transform: `translate(${viewport.panX}px, ${viewport.panY}px) scale(${viewport.zoom})`,
    // Promoting the layer makes panning and zooming compositor-only, but keeping it promoted while editing makes
    // every change re-rasterize the whole (scaled) layer.
    willChange: panning.value || zooming.value ? "transform" : "auto",
}));

// The dot grid is its own layer, translated by the pan modulo one cell, so panning never repaints it.
const gridStyle = computed(() => {
    const cell = GRID_SIZE * 2 * viewport.zoom;
    const offset = (value: number) => ((value % cell) + cell) % cell;

    return {
        width: `${hostSize.width + cell * 2}px`,
        height: `${hostSize.height + cell * 2}px`,
        backgroundSize: `${cell}px ${cell}px`,
        transform: `translate(${offset(viewport.panX) - cell}px, ${offset(viewport.panY) - cell}px)`,
    };
});

const zoomPercent = computed(() => `${Math.round(viewport.zoom * 100)}%`);

const connectPreview = computed(() => {
    const connection = connecting.value;
    const source = connection ? nodeById.value.get(connection.sourceId) : undefined;

    if (!connection || !source) {
        return null;
    }

    return props.previewPath(props.portAnchor(source, layouts.get(source.id), connection.port), connection.pointer);
});

const marqueeStyle = computed(() =>
    marquee.value ? { transform: `translate(${marquee.value.x}px, ${marquee.value.y}px)`, width: `${marquee.value.width}px`, height: `${marquee.value.height}px` } : {},
);

const rectOf = (node: TNode) => props.nodeRect(node, layouts.get(node.id));

const setViewport = (next: ViewportState) => Object.assign(viewport, next);

const hostPoint = (event: { clientX: number; clientY: number }): Point => {
    const rect = host.value?.getBoundingClientRect();

    return { x: event.clientX - (rect?.left ?? 0), y: event.clientY - (rect?.top ?? 0) };
};

const toCanvas = (event: { clientX: number; clientY: number }) => screenToCanvas(viewport, hostPoint(event));

/**
 * The point of the window at a point of the canvas.
 */
const toClient = (point: Point): Point => {
    const screen = canvasToScreen(viewport, point);
    const rect = host.value?.getBoundingClientRect();

    return { x: (rect?.left ?? 0) + screen.x, y: (rect?.top ?? 0) + screen.y };
};

const markZooming = () => {
    zooming.value = true;
    clearTimeout(zoomTimer);
    zoomTimer = setTimeout(() => (zooming.value = false), 200);
};

const fit = () => {
    setViewport(fitToContent(boundsOf(props.nodes.map(rectOf)), hostSize));
    markZooming();
};

const zoomTo = (zoom: number) => {
    setViewport(zoomAt(viewport, zoom, { x: hostSize.width / 2, y: hostSize.height / 2 }));
    markZooming();
};

const centerOn = (id: string) => {
    const node = nodeById.value.get(id);

    if (!node) {
        return;
    }

    const rect = rectOf(node);
    setViewport({
        zoom: viewport.zoom,
        panX: hostSize.width / 2 - (rect.x + rect.width / 2) * viewport.zoom,
        panY: hostSize.height / 2 - (rect.y + rect.height / 2) * viewport.zoom,
    });
};

// How far to pan along one axis so [start, end] is within [min, max]: its end first, but never past its start.
const panOffset = (start: number, end: number, min: number, max: number) => (end > max ? Math.min(end - max, start - min) : start < min ? start - min : 0);

/**
 * Pans, only as far as needed, so a node is in view above the bottom `inset` pixels of the canvas, which the node's
 * panel covers when it opens over it.
 */
const keepVisible = (id: string, inset = 0) => {
    const node = nodeById.value.get(id);

    if (!node || hostSize.height === 0) {
        return;
    }

    const margin = 24;
    const rect = rectOf(node);
    const offsetX = panOffset(rect.x * viewport.zoom + viewport.panX, (rect.x + rect.width) * viewport.zoom + viewport.panX, margin, hostSize.width - margin);
    const offsetY = panOffset(rect.y * viewport.zoom + viewport.panY, (rect.y + rect.height) * viewport.zoom + viewport.panY, margin, hostSize.height - inset - margin);

    if (offsetX !== 0 || offsetY !== 0) {
        setViewport({ zoom: viewport.zoom, panX: viewport.panX - offsetX, panY: viewport.panY - offsetY });
    }
};

const cssEscape = (value: string) => (typeof CSS !== "undefined" && CSS.escape ? CSS.escape(value) : value.replace(/["\\]/g, "\\$&"));

const nodeElement = (id: string) => host.value?.querySelector<HTMLElement>(`[data-node-id="${cssEscape(id)}"]`);

// A node that was just added or shown is rendered on the next tick.
const focusNode = async (id: string) => {
    if (!nodeElement(id)) {
        await nextTick();
    }

    nodeElement(id)?.focus();
};

const closeMenu = () => {
    menu.value = null;
};

/**
 * Opens the context menu at the pointer, or, from the keyboard, at the top end of a node.
 */
const openMenu = (items: MenuItem[], label: string, at: MouseEvent | { nodeId: string } | null) => {
    if (items.length === 0) {
        menu.value = null;

        return;
    }

    let position: Point = { x: 0, y: 0 };

    if (at instanceof MouseEvent) {
        position = hostPoint(at);
    } else if (at) {
        const node = nodeById.value.get(at.nodeId);
        const rect = node ? rectOf(node) : null;
        position = rect ? canvasToScreen(viewport, { x: rect.x + rect.width - 24, y: rect.y + 24 }) : position;
    }

    menu.value = { ...position, items, label };
};

const startPan = (event: PointerEvent, clearOnClick: boolean) => {
    const start = { panX: viewport.panX, panY: viewport.panY };

    startPointerDrag(event, {
        threshold: 2,
        onStart: () => (panning.value = true),
        onMove: (dx, dy) => {
            viewport.panX = start.panX + dx;
            viewport.panY = start.panY + dy;
        },
        onEnd: (moved) => {
            panning.value = false;

            if (!moved && clearOnClick) {
                emit("clear-selection");
            }
        },
    });
};

const startMarquee = (event: PointerEvent) => {
    const origin = toCanvas(event);
    const initialSelection = [...props.selectedIds];

    startPointerDrag(event, {
        onMove: (_dx, _dy, moveEvent) => {
            const rect = rectFromPoints(origin, toCanvas(moveEvent));
            marquee.value = rect;

            const inside = props.nodes.filter((node) => rectsIntersect(rect, rectOf(node))).map((node) => node.id);
            emit("select-nodes", [...new Set([...initialSelection, ...inside])]);
        },
        onEnd: () => {
            marquee.value = null;
        },
    });
};

const onBackgroundPointerDown = (event: PointerEvent) => {
    closeMenu();
    host.value?.focus({ preventScroll: true });

    if (event.button === 1 || (event.button === 0 && (spaceHeld.value || !event.shiftKey || props.readOnly))) {
        event.preventDefault();
        startPan(event, event.button === 0 && !spaceHeld.value);
    } else if (event.button === 0 && event.shiftKey) {
        event.preventDefault();
        startMarquee(event);
    }
};

const onNodePointerDown = (node: TNode, event: PointerEvent) => {
    closeMenu();

    if (event.button === 1 || spaceHeld.value) {
        event.preventDefault();
        event.stopPropagation();
        startPan(event, false);

        return;
    }

    if (event.button !== 0) {
        return;
    }

    event.stopPropagation();
    // The browser focuses the node after this handler returns; remember that the focus comes from a click.
    pointerFocus = true;
    setTimeout(() => (pointerFocus = false), 0);

    if (event.shiftKey) {
        emit("select-node", node.id, true, true);

        return;
    }

    const wasSelected = selected.value.has(node.id);

    // The read-only canvas has nothing to move: a click shows the node.
    if (props.readOnly) {
        emit("show-node", node.id);

        return;
    }

    // A click only selects a node, to move it around.
    if (!wasSelected) {
        emit("select-node", node.id, false, true);
    }

    const ids = [...props.selectedIds];
    const origins = new Map(ids.flatMap((id) => {
        const item = nodeById.value.get(id);

        return item ? [[id, { x: item.x, y: item.y }] as const] : [];
    }));
    const key = `drag-${++dragCounter}`;

    startPointerDrag(event, {
        onStart: () => {
            liftedIds.value = ids.length <= LIFT_LIMIT ? new Set(ids) : new Set();
            emit("drag-start", ids);
        },
        onMove: (dx, dy) => {
            const moves = [...origins].map(([id, origin]) => ({
                id,
                fromX: origin.x,
                fromY: origin.y,
                toX: snap(origin.x + dx / viewport.zoom),
                toY: snap(origin.y + dy / viewport.zoom),
            }));

            if (moves.some((move) => nodeById.value.get(move.id)?.x !== move.toX || nodeById.value.get(move.id)?.y !== move.toY)) {
                emit("move-nodes", moves, key);
            }
        },
        onEnd: (moved) => {
            liftedIds.value = new Set();

            if (!moved && wasSelected && props.selectedIds.length > 1) {
                emit("select-node", node.id, false, false);
            }
        },
    });
};

const onNodeFocus = (node: TNode) => {
    // Tabbing to a node selects it, so keyboard users can delete or nudge it; a click already did.
    if (!pointerFocus && !selected.value.has(node.id)) {
        emit("select-node", node.id, false, false);
    }
};

/**
 * Where a connection drawn from an output would go at a point of the window: the input port under it, or the input the
 * designer picks on the node under it.
 */
const targetAt = (clientX: number, clientY: number, sourceId: string, port: string) => {
    const element = document.elementFromPoint?.(clientX, clientY);
    const nodeId = element?.closest("[data-node-id]")?.getAttribute("data-node-id");
    const node = nodeId && nodeId !== sourceId ? nodeById.value.get(nodeId) : undefined;

    if (!node) {
        return { targetId: null, targetPort: null, valid: false };
    }

    const portName = element?.closest("[data-port-side=input]")?.getAttribute("data-port") ?? null;
    const target = props.resolveTarget(sourceId, port, node, portName);

    return { targetId: node.id, targetPort: target.port, valid: target.valid };
};

const onPortPointerDown = (node: TNode, port: string, event: PointerEvent) => {
    closeMenu();
    connecting.value = { sourceId: node.id, port, pointer: toCanvas(event), targetId: null, targetPort: null, valid: false };

    startPointerDrag(event, {
        threshold: 3,
        onMove: (_dx, _dy, moveEvent) => {
            if (connecting.value) {
                Object.assign(connecting.value, { pointer: toCanvas(moveEvent) }, targetAt(moveEvent.clientX, moveEvent.clientY, node.id, port));
            }
        },
        onEnd: (moved, endEvent) => {
            const connection = connecting.value;
            connecting.value = null;

            if (!connection || !endEvent) {
                return;
            }

            if (!moved) {
                emit("port-click", node.id, port, endEvent.clientX, endEvent.clientY);

                return;
            }

            const target = targetAt(endEvent.clientX, endEvent.clientY, node.id, port);
            const targetId = target.targetId ?? connection.targetId;
            const targetPort = target.targetId ? target.targetPort : connection.targetPort;

            if (targetId) {
                emit("connect", node.id, port, targetId, targetPort);
            }
        },
    });
};

const isEditable = (target: EventTarget | null) =>
    target instanceof HTMLElement &&
    (target.isContentEditable ||
        ["INPUT", "TEXTAREA", "SELECT", "BUTTON"].includes(target.tagName) ||
        !!target.closest(props.keyboardIgnore ? `.ocd-context-menu, ${props.keyboardIgnore}` : ".ocd-context-menu"));

const onKeyDown = (event: KeyboardEvent) => {
    if (isEditable(event.target)) {
        return;
    }

    const modifier = event.ctrlKey || event.metaKey;

    switch (event.key) {
        case " ":
            spaceHeld.value = true;
            event.preventDefault();
            break;
        case "Delete":
        case "Backspace":
            event.preventDefault();

            if (!props.readOnly) {
                emit("delete-selection");
            }
            break;
        case "a":
        case "A":
            if (modifier) {
                event.preventDefault();
                emit("select-all");
            }
            break;
        case "Escape":
            if (connecting.value || marquee.value) {
                connecting.value = null;
                marquee.value = null;
            } else {
                emit("clear-selection");
            }
            break;
        case "ArrowUp":
        case "ArrowDown":
        case "ArrowLeft":
        case "ArrowRight": {
            if (props.readOnly || props.selectedIds.length === 0) {
                break;
            }

            event.preventDefault();
            const step = event.shiftKey ? 1 : GRID_SIZE;
            const dx = event.key === "ArrowLeft" ? -step : event.key === "ArrowRight" ? step : 0;
            const dy = event.key === "ArrowUp" ? -step : event.key === "ArrowDown" ? step : 0;
            emit("nudge", dx, dy);
            break;
        }
        case "+":
        case "=":
            if (!modifier) {
                zoomTo(viewport.zoom * ZOOM_STEP);
            }
            break;
        case "-":
            if (!modifier) {
                zoomTo(viewport.zoom / ZOOM_STEP);
            }
            break;
    }
};

const onKeyUp = (event: KeyboardEvent) => {
    if (event.key === " ") {
        spaceHeld.value = false;
    }
};

const onWheel = (event: WheelEvent) => {
    event.preventDefault();

    if (event.ctrlKey || event.metaKey) {
        setViewport(zoomBy(viewport, Math.exp(-event.deltaY * 0.0015), hostPoint(event)));
    } else {
        const horizontal = event.shiftKey && event.deltaX === 0;
        viewport.panX -= horizontal ? event.deltaY : event.deltaX;
        viewport.panY -= horizontal ? 0 : event.deltaY;
    }

    markZooming();
};

// Toolbox items are dropped with HTML drag and drop; the drop point is converted to canvas coordinates.
const onDragOver = (event: DragEvent) => {
    if (!props.readOnly && props.dragType && event.dataTransfer?.types.includes(props.dragType)) {
        event.preventDefault();
        event.dataTransfer.dropEffect = "copy";
    }
};

const onDrop = (event: DragEvent) => {
    const value = props.dragType ? event.dataTransfer?.getData(props.dragType) : null;

    if (props.readOnly || !value) {
        return;
    }

    event.preventDefault();
    emit("drop", value, toCanvas(event));
};

let resizeObserver: ResizeObserver | null = null;

onMounted(() => {
    const element = host.value!;
    const updateSize = () => {
        hostSize.width = element.clientWidth;
        hostSize.height = element.clientHeight;
    };

    updateSize();

    if (typeof ResizeObserver !== "undefined") {
        resizeObserver = new ResizeObserver(updateSize);
        resizeObserver.observe(element);
    }

    // Wheel listeners must not be passive, so the page doesn't scroll while zooming.
    element.addEventListener("wheel", onWheel, { passive: false });
});

onBeforeUnmount(() => {
    resizeObserver?.disconnect();
    host.value?.removeEventListener("wheel", onWheel);
    clearTimeout(zoomTimer);
});

defineExpose({
    fit,
    centerOn,
    focusNode,
    keepVisible,
    openMenu,
    closeMenu,
    focus: () => host.value?.focus({ preventScroll: true }),
    zoomIn: () => zoomTo(viewport.zoom * ZOOM_STEP),
    zoomOut: () => zoomTo(viewport.zoom / ZOOM_STEP),
    resetZoom: () => zoomTo(1),
    visibleCenter: () => visibleCenter(viewport, hostSize),
    clientToCanvas: (clientX: number, clientY: number) => toCanvas({ clientX, clientY }),
    canvasToClient: toClient,
    layoutOf: (id: string) => layouts.get(id),
});
</script>

<template>
    <div
        ref="host"
        class="ocd-canvas"
        :class="{ 'is-panning': panning, 'is-connecting': !!connecting, 'is-readonly': readOnly, 'is-space-held': spaceHeld }"
        tabindex="0"
        :aria-label="label"
        data-cy="canvas-surface"
        @pointerdown="onBackgroundPointerDown"
        @keydown="onKeyDown"
        @keyup="onKeyUp"
        @dragover="onDragOver"
        @drop="onDrop"
        @contextmenu.prevent
    >
        <div class="ocd-grid" :style="gridStyle" aria-hidden="true"></div>

        <div class="ocd-layer" :style="layerStyle">
            <svg class="ocd-edges" width="1" height="1">
                <defs>
                    <marker id="ocd-arrow" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse">
                        <path class="ocd-arrow" d="M0,0 L10,5 L0,10 z" />
                    </marker>
                    <marker id="ocd-arrow-selected" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse">
                        <path class="ocd-arrow is-selected" d="M0,0 L10,5 L0,10 z" />
                    </marker>
                </defs>
                <template v-for="edge in staticEdges" :key="edge.key">
                    <slot name="edge" :edge="edge" :lifted="false" :source-layout="layouts.get(edge.source.id)" :target-layout="layouts.get(edge.target.id)"></slot>
                </template>
                <path
                    v-if="connectPreview"
                    class="ocd-edge-preview"
                    :class="{ 'is-invalid': connecting?.targetId && !connecting.valid }"
                    :d="connectPreview"
                    data-cy="connect-preview"
                />
            </svg>

            <svg v-if="liftedEdges.length > 0" class="ocd-edges is-lifted" width="1" height="1">
                <template v-for="edge in liftedEdges" :key="edge.key">
                    <slot name="edge" :edge="edge" :lifted="true" :source-layout="layouts.get(edge.source.id)" :target-layout="layouts.get(edge.target.id)"></slot>
                </template>
            </svg>

            <template v-for="node in nodes" :key="node.id">
                <slot
                    name="node"
                    :node="node"
                    :selected="selected.has(node.id)"
                    :lifted="liftedIds.has(node.id)"
                    :drop-target="connecting?.targetId === node.id && connecting.valid"
                    :connecting="connecting"
                    :on-node-pointerdown="(event: PointerEvent) => onNodePointerDown(node, event)"
                    :on-port-pointerdown="(port: string, event: PointerEvent) => onPortPointerDown(node, port, event)"
                    :on-measure="(layout: TLayout) => layouts.set(node.id, layout)"
                    :on-focus="() => onNodeFocus(node)"
                ></slot>
            </template>

            <div v-if="marquee" class="ocd-marquee" :style="marqueeStyle" data-cy="marquee"></div>
        </div>

        <slot></slot>

        <div class="ocd-zoom-controls btn-group btn-group-sm shadow-sm" role="group" :aria-label="t('Zoom', 'Zoom')" @pointerdown.stop>
            <button type="button" class="btn btn-light" :title="t('ZoomOut', 'Zoom out')" :aria-label="t('ZoomOut', 'Zoom out')" data-cy="zoom-out" @click="zoomTo(viewport.zoom / ZOOM_STEP)">
                <i class="fa-solid fa-minus" aria-hidden="true"></i>
            </button>
            <button type="button" class="btn btn-light ocd-zoom-level" :title="t('ResetZoom', 'Reset the zoom')" data-cy="zoom-reset" aria-live="polite" @click="zoomTo(1)">
                {{ zoomPercent }}
            </button>
            <button type="button" class="btn btn-light" :title="t('ZoomIn', 'Zoom in')" :aria-label="t('ZoomIn', 'Zoom in')" data-cy="zoom-in" @click="zoomTo(viewport.zoom * ZOOM_STEP)">
                <i class="fa-solid fa-plus" aria-hidden="true"></i>
            </button>
            <button type="button" class="btn btn-light" :title="t('FitToContent', 'Fit to content')" :aria-label="t('FitToContent', 'Fit to content')" data-cy="zoom-fit" @click="fit">
                <i class="fa-solid fa-expand" aria-hidden="true"></i>
            </button>
        </div>

        <ContextMenu v-if="menu" :x="menu.x" :y="menu.y" :items="menu.items" :label="menu.label" @close="closeMenu" />
    </div>
</template>
