<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref, shallowReactive } from "vue";
import type { DesignIssue, DesignerConnection, DesignerNode, PortKind } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { moveNodesCommand } from "../state/commands";
import { connectionKey, findOutput, firstCompatibleInput, rejectConnection, type ConnectRejection } from "../state/ports";
import StepNode from "./StepNode.vue";
import ConnectionEdge from "./ConnectionEdge.vue";
import ContextMenu from "./ContextMenu.vue";
import ConnectDialog from "./ConnectDialog.vue";
import type { MenuItem } from "./menu";
import { GRID_SIZE, NODE_WIDTH, boundsOf, nodeRect, portAnchor, previewPath, rectFromPoints, rectsIntersect, snap, type NodeLayout, type Point, type Rect } from "./geometry";
import { ZOOM_STEP, canvasToScreen, fitToContent, screenToCanvas, visibleCenter, zoomAt, zoomBy, type Size, type ViewportState } from "./viewport";
import { startPointerDrag } from "./useDrag";
import { clearSelection, connectPorts, deleteSelection, nudgeSelection, selectAll, selectConnection, selectNode, showStep } from "./useConnect";
import { runOverlay } from "../runs/runOverlay";
import { showToast } from "../ui/toasts";
import { STEP_DRAG_TYPE } from "../toolbox/filter";
import { t } from "../i18n";

// Dragged steps and their connections get their own compositing layers while they move, so the rest of the canvas
// isn't re-rasterized every frame. Beyond this many steps the extra layers cost more than they save.
const LIFT_LIMIT = 10;
const NO_ISSUES: DesignIssue[] = [];
const NO_PORTS: string[] = [];

const props = withDefaults(defineProps<{ store: DesignerStore; readOnly?: boolean }>(), { readOnly: false });

const emit = defineEmits<{
    (event: "edit", stepId: string): void;
    (event: "preview", stepId: string): void;
    (event: "drop-step", type: string, point: Point): void;
    // A click on an unconnected output: add the next step after it, at that point of the window.
    (event: "add-after", stepId: string, port: string, clientX: number, clientY: number): void;
}>();

interface Connecting {
    sourceId: string;
    port: string;
    kind: PortKind;
    pointer: Point;
    // The step and input under the pointer, and whether the connection can go there.
    targetId: string | null;
    targetPort: string | null;
    valid: boolean;
}

const state = props.store.state;
const viewport = state.viewport;
const host = ref<HTMLElement | null>(null);
const hostSize = reactive<Size>({ width: 0, height: 0 });
const layouts = shallowReactive(new Map<string, NodeLayout>());
const liftedIds = ref(new Set<string>());
const panning = ref(false);
const zooming = ref(false);
const spaceHeld = ref(false);
const marquee = ref<Rect | null>(null);
const connecting = ref<Connecting | null>(null);
const menu = ref<{ x: number; y: number; items: MenuItem[]; label: string } | null>(null);
const connectDialog = ref<{ sourceId: string; port?: string } | null>(null);

let dragCounter = 0;
let zoomTimer: ReturnType<typeof setTimeout> | undefined;
let pointerFocus = false;

const nodeById = computed(() => new Map(state.nodes.map((node) => [node.id, node])));
const selectedIds = computed(() => new Set(state.selectedNodeIds));
const overlays = computed(() => runOverlay(state.overlayRun, state.nodes));

const issuesByStep = computed(() => {
    const map = new Map<string, DesignIssue[]>();

    for (const issue of state.issues) {
        if (issue.stepId) {
            map.set(issue.stepId, [...(map.get(issue.stepId) ?? []), issue]);
        }
    }

    return map;
});

const connectedPorts = computed(() => {
    const inputs = new Map<string, string[]>();
    const outputs = new Map<string, string[]>();

    for (const connection of state.connections) {
        outputs.set(connection.sourceStepId, [...(outputs.get(connection.sourceStepId) ?? []), connection.sourcePort]);
        inputs.set(connection.targetStepId, [...(inputs.get(connection.targetStepId) ?? []), connection.targetPort]);
    }

    return { inputs, outputs };
});

// While a connection is dragged, which inputs of each step can receive it.
const compatibility = computed(() => {
    const connection = connecting.value;

    if (!connection) {
        return null;
    }

    const map = new Map<string, Record<string, boolean>>();

    for (const node of state.nodes) {
        if (node.id === connection.sourceId) {
            continue;
        }

        map.set(
            node.id,
            Object.fromEntries(
                node.inputs.map((input) => [
                    input.name,
                    rejectConnection(state.nodes, state.connections, { sourceStepId: connection.sourceId, sourcePort: connection.port, targetStepId: node.id, targetPort: input.name }) === null,
                ]),
            ),
        );
    }

    return map;
});

const edges = computed(() =>
    state.connections
        .map((connection) => ({
            key: connectionKey(connection),
            connection,
            source: nodeById.value.get(connection.sourceStepId),
            target: nodeById.value.get(connection.targetStepId),
        }))
        .filter((edge): edge is { key: string; connection: DesignerConnection; source: DesignerNode; target: DesignerNode } => !!edge.source && !!edge.target),
);

const isLiftedEdge = (edge: { source: DesignerNode; target: DesignerNode }) => liftedIds.value.has(edge.source.id) || liftedIds.value.has(edge.target.id);
const staticEdges = computed(() => (liftedIds.value.size === 0 ? edges.value : edges.value.filter((edge) => !isLiftedEdge(edge))));
const liftedEdges = computed(() => (liftedIds.value.size === 0 ? [] : edges.value.filter(isLiftedEdge)));

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

    const index = source.outputs.findIndex((port) => port.name === connection.port);

    return previewPath(portAnchor(source, layouts.get(source.id), "output", connection.port, index, source.outputs.length), connection.pointer);
});

const marqueeStyle = computed(() =>
    marquee.value ? { transform: `translate(${marquee.value.x}px, ${marquee.value.y}px)`, width: `${marquee.value.width}px`, height: `${marquee.value.height}px` } : {},
);

const connectDialogSource = computed(() => (connectDialog.value ? nodeById.value.get(connectDialog.value.sourceId) : undefined));

const setViewport = (next: ViewportState) => Object.assign(viewport, next);

const hostPoint = (event: { clientX: number; clientY: number }): Point => {
    const rect = host.value?.getBoundingClientRect();

    return { x: event.clientX - (rect?.left ?? 0), y: event.clientY - (rect?.top ?? 0) };
};

const toCanvas = (event: { clientX: number; clientY: number }) => screenToCanvas(viewport, hostPoint(event));

const markZooming = () => {
    zooming.value = true;
    clearTimeout(zoomTimer);
    zoomTimer = setTimeout(() => (zooming.value = false), 200);
};

const contentBounds = () => boundsOf(state.nodes.map((node) => nodeRect(node, layouts.get(node.id))));

const fit = () => {
    setViewport(fitToContent(contentBounds(), hostSize));
    markZooming();
};

const zoomTo = (zoom: number) => {
    setViewport(zoomAt(viewport, zoom, { x: hostSize.width / 2, y: hostSize.height / 2 }));
    markZooming();
};

const centerOn = (stepId: string) => {
    const node = nodeById.value.get(stepId);

    if (!node) {
        return;
    }

    const rect = nodeRect(node, layouts.get(node.id));
    setViewport({
        zoom: viewport.zoom,
        panX: hostSize.width / 2 - (rect.x + rect.width / 2) * viewport.zoom,
        panY: hostSize.height / 2 - (rect.y + rect.height / 2) * viewport.zoom,
    });
};

// How far to pan along one axis so [start, end] is within [min, max]: its end first, but never past its start.
const panOffset = (start: number, end: number, min: number, max: number) => (end > max ? Math.min(end - max, start - min) : start < min ? start - min : 0);

/**
 * Pans, only as far as needed, so a step is in view above the bottom `inset` pixels of the canvas, which the step
 * panel covers when it opens over it.
 */
const keepVisible = (stepId: string, inset = 0) => {
    const node = nodeById.value.get(stepId);

    if (!node || hostSize.height === 0) {
        return;
    }

    const margin = 24;
    const rect = nodeRect(node, layouts.get(node.id));
    const offsetX = panOffset(rect.x * viewport.zoom + viewport.panX, (rect.x + rect.width) * viewport.zoom + viewport.panX, margin, hostSize.width - margin);
    const offsetY = panOffset(rect.y * viewport.zoom + viewport.panY, (rect.y + rect.height) * viewport.zoom + viewport.panY, margin, hostSize.height - inset - margin);

    if (offsetX !== 0 || offsetY !== 0) {
        setViewport({ zoom: viewport.zoom, panX: viewport.panX - offsetX, panY: viewport.panY - offsetY });
    }
};

const cssEscape = (value: string) => (typeof CSS !== "undefined" && CSS.escape ? CSS.escape(value) : value.replace(/["\\]/g, "\\$&"));

const nodeElement = (stepId: string) => host.value?.querySelector<HTMLElement>(`[data-node-id="${cssEscape(stepId)}"]`);

// A step that was just added is rendered on the next tick.
const focusNode = async (stepId: string) => {
    if (!nodeElement(stepId)) {
        await nextTick();
    }

    nodeElement(stepId)?.focus();
};

const closeMenu = () => {
    menu.value = null;
};

const notifyDeleted = (result: { nodes: number; connections: number }) => {
    if (result.nodes === 0 && result.connections === 0) {
        return;
    }

    showToast({
        message:
            result.nodes === 1
                ? t("DeletedStep", "Step deleted.")
                : result.nodes > 1
                  ? t("DeletedSteps", "{0} steps deleted.", result.nodes)
                  : t("DeletedConnection", "Connection deleted."),
        action: { label: t("Undo", "Undo"), run: () => props.store.undo() },
    });
};

const deleteSelected = () => {
    if (!props.readOnly) {
        notifyDeleted(deleteSelection(props.store));
        host.value?.focus();
    }
};

/**
 * Deletes a step, as the Delete key does on the selection, with a toast that can undo it.
 */
const deleteStep = (stepId: string) => {
    selectNode(props.store, stepId);
    deleteSelected();
};

const rejectionMessage = (reason: ConnectRejection) =>
    ({
        kind: () => t("RejectedKind", "Rows can only go to an input of rows, and files to an input of files."),
        cycle: () => t("RejectedCycle", "This connection would make the pipeline loop."),
        self: () => t("RejectedSelf", "A step can't be connected to itself."),
        missing: () => t("RejectedMissing", "This input doesn't exist."),
    })[reason]();

const connect = (connection: DesignerConnection) => {
    const result = connectPorts(props.store, connection);

    if (result.status === "rejected") {
        showToast({ message: rejectionMessage(result.reason), variant: "warning" });
    } else if (result.status === "replaced") {
        const target = nodeById.value.get(connection.targetStepId);
        const input = target?.inputs.find((port) => port.name === connection.targetPort);

        showToast({
            message: t("ConnectionReplaced", "The input {0} of {1} accepts one connection: the previous one was replaced.", input?.displayName ?? connection.targetPort, target?.displayText ?? ""),
            variant: "warning",
            action: { label: t("Undo", "Undo"), run: () => props.store.undo() },
        });
    }

    return result;
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
                clearSelection(props.store);
            }
        },
    });
};

const startMarquee = (event: PointerEvent) => {
    const origin = toCanvas(event);
    const initialSelection = [...state.selectedNodeIds];

    startPointerDrag(event, {
        onMove: (_dx, _dy, moveEvent) => {
            const rect = rectFromPoints(origin, toCanvas(moveEvent));
            marquee.value = rect;

            const inside = state.nodes.filter((node) => rectsIntersect(rect, nodeRect(node, layouts.get(node.id)))).map((node) => node.id);
            state.selectedNodeIds = [...new Set([...initialSelection, ...inside])];
            state.selectedConnectionKey = null;
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

const onNodePointerDown = (node: DesignerNode, event: PointerEvent) => {
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
        selectNode(props.store, node.id, true);

        return;
    }

    const wasSelected = selectedIds.value.has(node.id);

    // The read-only view has nothing to move: a click shows the step's panel.
    if (props.readOnly) {
        showStep(props.store, node.id);

        return;
    }

    // A click only selects a step, to move it around: its panel opens on a double-click or its Edit button.
    if (!wasSelected) {
        selectNode(props.store, node.id);
        state.stepPanelRequested = false;
    }

    const ids = [...state.selectedNodeIds];
    const origins = new Map(ids.map((id) => [id, { x: nodeById.value.get(id)!.x, y: nodeById.value.get(id)!.y }]));
    const key = `drag-${++dragCounter}`;

    startPointerDrag(event, {
        onStart: () => {
            liftedIds.value = ids.length <= LIFT_LIMIT ? new Set(ids) : new Set();

            // Moving steps around closes their panel (unless it's pinned), so it doesn't cover them.
            state.stepPanelRequested = false;
        },
        onMove: (dx, dy) => {
            const moves = ids.map((id) => {
                const origin = origins.get(id)!;

                return { id, fromX: origin.x, fromY: origin.y, toX: snap(origin.x + dx / viewport.zoom), toY: snap(origin.y + dy / viewport.zoom) };
            });

            if (moves.some((move) => nodeById.value.get(move.id)?.x !== move.toX || nodeById.value.get(move.id)?.y !== move.toY)) {
                props.store.execute(moveNodesCommand(props.store.graph, moves, key));
            }
        },
        onEnd: (moved) => {
            liftedIds.value = new Set();

            if (!moved && wasSelected && state.selectedNodeIds.length > 1) {
                selectNode(props.store, node.id);
            }
        },
    });
};

const onNodeFocus = (node: DesignerNode) => {
    // Tabbing to a step selects it, so keyboard users can delete or nudge it; a click already did.
    if (!pointerFocus && !selectedIds.value.has(node.id)) {
        selectNode(props.store, node.id);
    }
};

/**
 * The input a connection dragged from `connection.sourceId` would reach at a point of the window: the input port under
 * it, or the first input of the right kind of the step under it.
 */
const targetAt = (clientX: number, clientY: number, connection: Pick<Connecting, "sourceId" | "port" | "kind">) => {
    const element = document.elementFromPoint?.(clientX, clientY);
    const nodeId = element?.closest("[data-node-id]")?.getAttribute("data-node-id");
    const node = nodeId && nodeId !== connection.sourceId ? nodeById.value.get(nodeId) : undefined;

    if (!node) {
        return { targetId: null, targetPort: null, valid: false };
    }

    const portName = element?.closest("[data-port-side=input]")?.getAttribute("data-port") ?? firstCompatibleInput(node, connection.kind, state.connections)?.name ?? null;
    const valid =
        !!portName &&
        rejectConnection(state.nodes, state.connections, { sourceStepId: connection.sourceId, sourcePort: connection.port, targetStepId: node.id, targetPort: portName }) === null;

    return { targetId: node.id, targetPort: portName, valid };
};

const onPortPointerDown = (node: DesignerNode, port: string, event: PointerEvent) => {
    const output = findOutput(node, port);

    if (!output) {
        return;
    }

    closeMenu();
    connecting.value = { sourceId: node.id, port, kind: output.kind, pointer: toCanvas(event), targetId: null, targetPort: null, valid: false };

    startPointerDrag(event, {
        threshold: 3,
        onMove: (_dx, _dy, moveEvent) => {
            if (connecting.value) {
                Object.assign(connecting.value, { pointer: toCanvas(moveEvent) }, targetAt(moveEvent.clientX, moveEvent.clientY, connecting.value));
            }
        },
        onEnd: (moved, endEvent) => {
            const connection = connecting.value;
            connecting.value = null;

            if (!connection || !endEvent) {
                return;
            }

            // A click on an output that leads nowhere yet adds the next step after it.
            if (!moved) {
                if (!state.connections.some((item) => item.sourceStepId === node.id && item.sourcePort === port)) {
                    emit("add-after", node.id, port, endEvent.clientX, endEvent.clientY);
                }

                return;
            }

            const target = targetAt(endEvent.clientX, endEvent.clientY, connection);
            const targetId = target.targetId ?? connection.targetId;
            const targetPort = target.targetId ? target.targetPort : connection.targetPort;

            if (targetId && targetPort) {
                connect({ sourceStepId: node.id, sourcePort: port, targetStepId: targetId, targetPort });
            } else if (targetId) {
                showToast({ message: t("NoCompatibleInput", "This step has no input for this output."), variant: "warning" });
            }
        },
    });
};

const openConnectDialog = (node: DesignerNode, port?: string) => {
    if (!props.readOnly && state.nodes.length > 1 && node.outputs.length > 0) {
        connectDialog.value = { sourceId: node.id, port };
    }
};

const onConnectDialogConfirm = (connection: DesignerConnection) => {
    connectDialog.value = null;
    connect(connection);
    focusNode(connection.sourceStepId);
};

const onConnectDialogCancel = () => {
    const sourceId = connectDialog.value?.sourceId;
    connectDialog.value = null;

    if (sourceId) {
        focusNode(sourceId);
    }
};

// Where the quick-add list opens for an output chosen from the keyboard or the context menu: beside its port.
const addAfter = (node: DesignerNode, port: string) => {
    const index = node.outputs.findIndex((item) => item.name === port);
    const anchor = canvasToScreen(viewport, portAnchor(node, layouts.get(node.id), "output", port, index, node.outputs.length));
    const rect = host.value?.getBoundingClientRect();

    emit("add-after", node.id, port, (rect?.left ?? 0) + anchor.x, (rect?.top ?? 0) + anchor.y);
};

const openNodeMenu = (node: DesignerNode, event: MouseEvent | null) => {
    if (!selectedIds.value.has(node.id)) {
        selectNode(props.store, node.id);
    }

    const position = event ? hostPoint(event) : canvasToScreen(viewport, { x: node.x + (layouts.get(node.id)?.width ?? NODE_WIDTH) - 24, y: node.y + 24 });
    const items: MenuItem[] = [];
    const label = t("StepActions", "Actions of {0}", node.displayText);

    items.push({
        label: props.readOnly ? t("ShowStep", "Show the step") : t("EditStep", "Edit"),
        icon: props.readOnly ? "fa-solid fa-table" : "fa-solid fa-pen",
        dataCy: "menu-edit",
        run: () => emit("edit", node.id),
    });

    if (!props.readOnly) {
        items.push({ label: t("PreviewData", "Preview the data"), icon: "fa-solid fa-eye", dataCy: "menu-preview", run: () => emit("preview", node.id) });

        for (const output of node.outputs) {
            items.push({
                label: node.outputs.length > 1 ? t("AddStepAfterPort", "Add a step after {0}…", output.displayName) : t("AddStepAfter", "Add a step after it…"),
                icon: "fa-solid fa-plus",
                dataCy: `menu-add-after-${output.name}`,
                run: () => addAfter(node, output.name),
            });
        }

        if (node.outputs.length > 0 && state.nodes.length > 1) {
            items.push({ label: t("ConnectOutputTo", "Connect an output to…"), icon: "fa-solid fa-link", dataCy: "menu-connect", run: () => openConnectDialog(node) });
        }

        for (const connection of state.connections.filter((item) => item.sourceStepId === node.id || item.targetStepId === node.id)) {
            const outgoing = connection.sourceStepId === node.id;
            const other = nodeById.value.get(outgoing ? connection.targetStepId : connection.sourceStepId);

            items.push({
                label: outgoing
                    ? t("RemoveConnectionTo", "Disconnect from {0}", other?.displayText ?? connection.targetStepId)
                    : t("RemoveConnectionFrom", "Disconnect from {0}", other?.displayText ?? connection.sourceStepId),
                icon: "fa-solid fa-link-slash",
                dataCy: `menu-disconnect-${connectionKey(connection)}`,
                run: () => {
                    selectConnection(props.store, connectionKey(connection));
                    deleteSelected();
                },
            });
        }

        items.push({
            label: state.selectedNodeIds.length > 1 ? t("DeleteSelected", "Delete the {0} selected steps", state.selectedNodeIds.length) : t("Delete", "Delete"),
            icon: "fa-solid fa-trash",
            danger: true,
            dataCy: "menu-delete",
            run: deleteSelected,
        });
    }

    menu.value = { ...position, items, label };
};

const openEdgeMenu = (key: string, event: MouseEvent) => {
    if (props.readOnly) {
        return;
    }

    selectConnection(props.store, key);
    menu.value = {
        ...hostPoint(event),
        label: t("ConnectionActions", "Actions of the connection"),
        items: [{ label: t("DeleteConnection", "Delete the connection"), icon: "fa-solid fa-trash", danger: true, dataCy: "menu-delete-edge", run: deleteSelected }],
    };
};

const isEditable = (target: EventTarget | null) =>
    target instanceof HTMLElement &&
    (target.isContentEditable || ["INPUT", "TEXTAREA", "SELECT", "BUTTON"].includes(target.tagName) || !!target.closest(".dpd-connect-dialog, .dpd-context-menu"));

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
            deleteSelected();
            break;
        case "a":
        case "A":
            if (modifier) {
                event.preventDefault();
                selectAll(props.store);
            }
            break;
        case "Escape":
            if (connecting.value || marquee.value) {
                connecting.value = null;
                marquee.value = null;
            } else {
                clearSelection(props.store);
            }
            break;
        case "ArrowUp":
        case "ArrowDown":
        case "ArrowLeft":
        case "ArrowRight": {
            if (props.readOnly || state.selectedNodeIds.length === 0) {
                break;
            }

            event.preventDefault();
            const step = event.shiftKey ? 1 : GRID_SIZE;
            const dx = event.key === "ArrowLeft" ? -step : event.key === "ArrowRight" ? step : 0;
            const dy = event.key === "ArrowUp" ? -step : event.key === "ArrowDown" ? step : 0;
            nudgeSelection(props.store, dx, dy);
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

// Toolbox cards are dropped with HTML drag and drop; the drop point is converted to canvas coordinates.
const onDragOver = (event: DragEvent) => {
    if (!props.readOnly && event.dataTransfer?.types.includes(STEP_DRAG_TYPE)) {
        event.preventDefault();
        event.dataTransfer.dropEffect = "copy";
    }
};

const onDrop = (event: DragEvent) => {
    const type = event.dataTransfer?.getData(STEP_DRAG_TYPE);

    if (props.readOnly || !type) {
        return;
    }

    event.preventDefault();
    emit("drop-step", type, toCanvas(event));
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
    deleteStep,
    focus: () => host.value?.focus({ preventScroll: true }),
    zoomIn: () => zoomTo(viewport.zoom * ZOOM_STEP),
    zoomOut: () => zoomTo(viewport.zoom / ZOOM_STEP),
    resetZoom: () => zoomTo(1),
    visibleCenter: () => visibleCenter(viewport, hostSize),
    clientToCanvas: (clientX: number, clientY: number) => toCanvas({ clientX, clientY }),
    layoutOf: (stepId: string) => layouts.get(stepId),
});
</script>

<template>
    <div
        ref="host"
        class="dpd-canvas"
        :class="{ 'is-panning': panning, 'is-connecting': !!connecting, 'is-readonly': readOnly, 'is-space-held': spaceHeld }"
        tabindex="0"
        :aria-label="
            t(
                'CanvasInstructions',
                'Pipeline canvas. Tab to a step; Enter opens it, the arrow keys move it, Delete removes it, Shift+F10 shows its actions. Ctrl+Z undoes.',
            )
        "
        data-cy="canvas-surface"
        @pointerdown="onBackgroundPointerDown"
        @keydown="onKeyDown"
        @keyup="onKeyUp"
        @dragover="onDragOver"
        @drop="onDrop"
        @contextmenu.prevent
    >
        <div class="dpd-grid" :style="gridStyle" aria-hidden="true"></div>

        <div class="dpd-layer" :style="layerStyle">
            <svg class="dpd-edges" width="1" height="1">
                <defs>
                    <marker id="dpd-arrow" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse">
                        <path class="dpd-arrow" d="M0,0 L10,5 L0,10 z" />
                    </marker>
                    <marker id="dpd-arrow-selected" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse">
                        <path class="dpd-arrow is-selected" d="M0,0 L10,5 L0,10 z" />
                    </marker>
                </defs>
                <ConnectionEdge
                    v-for="edge in staticEdges"
                    :key="edge.key"
                    :edge-key="edge.key"
                    :connection="edge.connection"
                    :source="edge.source"
                    :target="edge.target"
                    :source-layout="layouts.get(edge.source.id)"
                    :target-layout="layouts.get(edge.target.id)"
                    :selected="state.selectedConnectionKey === edge.key"
                    :read-only="readOnly"
                    @select="selectConnection(store, edge.key)"
                    @delete="deleteSelected"
                    @open-menu="openEdgeMenu(edge.key, $event)"
                />
                <path v-if="connectPreview" class="dpd-edge-preview" :class="{ 'is-invalid': connecting?.targetId && !connecting.valid }" :d="connectPreview" data-cy="connect-preview" />
            </svg>

            <svg v-if="liftedEdges.length > 0" class="dpd-edges is-lifted" width="1" height="1">
                <ConnectionEdge
                    v-for="edge in liftedEdges"
                    :key="edge.key"
                    :edge-key="edge.key"
                    :connection="edge.connection"
                    :source="edge.source"
                    :target="edge.target"
                    :source-layout="layouts.get(edge.source.id)"
                    :target-layout="layouts.get(edge.target.id)"
                    :read-only="readOnly"
                />
            </svg>

            <StepNode
                v-for="node in state.nodes"
                :key="node.id"
                :node="node"
                :selected="selectedIds.has(node.id)"
                :lifted="liftedIds.has(node.id)"
                :drop-target="connecting?.targetId === node.id && connecting.valid"
                :read-only="readOnly"
                :issues="issuesByStep.get(node.id) ?? NO_ISSUES"
                :connected-inputs="connectedPorts.inputs.get(node.id) ?? NO_PORTS"
                :connected-outputs="connectedPorts.outputs.get(node.id) ?? NO_PORTS"
                :compatibility="compatibility?.get(node.id) ?? null"
                :hovered-input="connecting?.targetId === node.id ? connecting.targetPort : null"
                :overlay="overlays[node.id] ?? null"
                @node-pointerdown="onNodePointerDown(node, $event)"
                @port-pointerdown="(port, event) => onPortPointerDown(node, port, event)"
                @port-activate="(port) => openConnectDialog(node, port)"
                @measure="(layout) => layouts.set(node.id, layout)"
                @open-menu="(event) => openNodeMenu(node, event)"
                @edit="emit('edit', node.id)"
                @focus="onNodeFocus(node)"
            />

            <div v-if="marquee" class="dpd-marquee" :style="marqueeStyle" data-cy="marquee"></div>
        </div>

        <div v-if="state.loaded && state.nodes.length === 0" class="dpd-empty" data-cy="canvas-empty">
            {{ readOnly ? t("EmptyPipelineReadOnly", "This pipeline has no steps.") : t("EmptyPipeline", "Drag a source from the toolbox to start the pipeline.") }}
        </div>

        <div class="dpd-zoom-controls btn-group btn-group-sm shadow-sm" role="group" :aria-label="t('Zoom', 'Zoom')" @pointerdown.stop>
            <button type="button" class="btn btn-light" :title="t('ZoomOut', 'Zoom out')" :aria-label="t('ZoomOut', 'Zoom out')" data-cy="zoom-out" @click="zoomTo(viewport.zoom / ZOOM_STEP)">
                <i class="fa-solid fa-minus" aria-hidden="true"></i>
            </button>
            <button type="button" class="btn btn-light dpd-zoom-level" :title="t('ResetZoom', 'Reset the zoom')" data-cy="zoom-reset" aria-live="polite" @click="zoomTo(1)">
                {{ zoomPercent }}
            </button>
            <button type="button" class="btn btn-light" :title="t('ZoomIn', 'Zoom in')" :aria-label="t('ZoomIn', 'Zoom in')" data-cy="zoom-in" @click="zoomTo(viewport.zoom * ZOOM_STEP)">
                <i class="fa-solid fa-plus" aria-hidden="true"></i>
            </button>
            <button type="button" class="btn btn-light" :title="t('FitToContent', 'Fit to the steps')" :aria-label="t('FitToContent', 'Fit to the steps')" data-cy="zoom-fit" @click="fit">
                <i class="fa-solid fa-expand" aria-hidden="true"></i>
            </button>
        </div>

        <ContextMenu v-if="menu" :x="menu.x" :y="menu.y" :items="menu.items" :label="menu.label" @close="closeMenu" />

        <ConnectDialog
            v-if="connectDialogSource"
            :source="connectDialogSource"
            :nodes="state.nodes"
            :connections="state.connections"
            :port="connectDialog?.port"
            @connect="onConnectDialogConfirm"
            @cancel="onConnectDialogCancel"
        />
    </div>
</template>
