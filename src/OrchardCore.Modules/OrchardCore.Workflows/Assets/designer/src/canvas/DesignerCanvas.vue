<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref, shallowReactive, watch } from "vue";
import type { DesignIssue, DesignerNode } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { moveNodesCommand, transitionKey } from "../state/commands";
import ActivityNode from "./ActivityNode.vue";
import TransitionEdge from "./TransitionEdge.vue";
import ContextMenu from "./ContextMenu.vue";
import ConnectDialog from "./ConnectDialog.vue";
import type { MenuItem } from "./menu";
import type { CanvasChanges } from "./changes";
import { GRID_SIZE, NODE_WIDTH, boundsOf, nodeRect, portAnchor, previewPath, rectFromPoints, rectsIntersect, snap, type NodeLayout, type Point, type Rect } from "./geometry";
import { ZOOM_STEP, canvasToScreen, fitToContent, screenToCanvas, visibleCenter, zoomAt, zoomBy, type Size, type ViewportState } from "./viewport";
import { startPointerDrag } from "./useDrag";
import { clearSelection, connectOutcome, deleteSelection, nudgeSelection, selectAll, selectNode, selectTransition, showActivity, toggleStart } from "./useConnect";
import { useCollapsedBranches } from "./useCollapsedBranches";
import { scriptErrorsByActivity } from "./scriptErrors";
import { showToast } from "../ui/toasts";
import { ACTIVITY_DRAG_TYPE } from "../toolbox/filter";
import { t } from "../i18n";

// Dragged activities and their edges get their own compositing layers while they move, so the rest of the
// canvas isn't re-rasterized every frame (see "Spike findings"). Beyond this many activities the extra
// layers cost more than they save.
const LIFT_LIMIT = 10;
const NO_ISSUES: DesignIssue[] = [];
const NO_OUTCOMES: string[] = [];
const NO_ERRORS: string[] = [];

const props = withDefaults(
    defineProps<{
        store: DesignerStore;
        readOnly?: boolean;
        highlightedIds?: string[];
        // The differences to show, on the compare page.
        changes?: CanvasChanges | null;
    }>(),
    {
        readOnly: false,
        highlightedIds: () => [],
        changes: null,
    },
);

const emit = defineEmits<{
    (event: "edit", activityId: string): void;
    (event: "drop-activity", activityName: string, point: Point): void;
    // A click on an unconnected outcome port: add the next activity after it, at that point of the window.
    (event: "add-after", activityId: string, outcome: string, clientX: number, clientY: number): void;
}>();

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
const connecting = ref<{ sourceId: string; outcome: string; pointer: Point; targetId: string | null } | null>(null);
const menu = ref<{ x: number; y: number; items: MenuItem[]; label: string } | null>(null);
const connectDialog = ref<{ sourceId: string; outcome?: string } | null>(null);

let dragCounter = 0;
let zoomTimer: ReturnType<typeof setTimeout> | undefined;
let pointerFocus = false;

const nodeById = computed(() => new Map(state.nodes.map((node) => [node.id, node])));
const { collapsed, hidden, hiddenCounts, countHiddenBy, collapse, expand, expandAll, reveal } = useCollapsedBranches(props.store);
const visibleNodes = computed(() => (hidden.value.size === 0 ? state.nodes : state.nodes.filter((node) => !hidden.value.has(node.id))));
const highlighted = computed(() => new Set(props.highlightedIds));

// The activities the instance shown by the viewer waits on.
// TODO: Phase 5 records the executed activities; highlight the executed path here then.
const blockingIds = computed(() => new Set(state.instance?.blockingActivityIds ?? []));

// What the instance shown by the viewer ran, from its journal.
const executedCounts = computed(() => state.instance?.executedActivityCounts ?? {});
const executedTransitions = computed(() => state.instance?.executedTransitionCounts ?? {});
const faultedId = computed(() => state.instance?.faultedActivityId ?? null);
const scriptErrors = computed(() => scriptErrorsByActivity(state.instance));
const selectedIds = computed(() => new Set(state.selectedNodeIds));

const issuesByActivity = computed(() => {
    const map = new Map<string, DesignIssue[]>();

    for (const issue of state.issues) {
        if (issue.activityId) {
            map.set(issue.activityId, [...(map.get(issue.activityId) ?? []), issue]);
        }
    }

    return map;
});

const connectedOutcomes = computed(() => {
    const map = new Map<string, string[]>();

    for (const transition of state.transitions) {
        map.set(transition.sourceActivityId, [...(map.get(transition.sourceActivityId) ?? []), transition.sourceOutcomeName]);
    }

    return map;
});

const edges = computed(() =>
    state.transitions
        .map((transition) => ({
            key: transitionKey(transition),
            outcome: transition.sourceOutcomeName,
            source: nodeById.value.get(transition.sourceActivityId),
            target: nodeById.value.get(transition.destinationActivityId),
        }))
        .filter(
            (edge): edge is { key: string; outcome: string; source: DesignerNode; target: DesignerNode } =>
                !!edge.source && !!edge.target && !hidden.value.has(edge.source.id) && !hidden.value.has(edge.target.id),
        ),
);

const isLiftedEdge = (edge: { source: DesignerNode; target: DesignerNode }) => liftedIds.value.has(edge.source.id) || liftedIds.value.has(edge.target.id);
const staticEdges = computed(() => (liftedIds.value.size === 0 ? edges.value : edges.value.filter((edge) => !isLiftedEdge(edge))));
const liftedEdges = computed(() => (liftedIds.value.size === 0 ? [] : edges.value.filter(isLiftedEdge)));

const layerStyle = computed(() => ({
    transform: `translate(${viewport.panX}px, ${viewport.panY}px) scale(${viewport.zoom})`,
    // Promoting the layer makes panning and zooming compositor-only, but keeping it promoted while
    // editing makes every change re-rasterize the whole (scaled) layer.
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

    const index = source.outcomes.findIndex((outcome) => outcome.name === connection.outcome);

    return previewPath(portAnchor(source, layouts.get(source.id), connection.outcome, index, source.outcomes.length), connection.pointer);
});

const marqueeStyle = computed(() =>
    marquee.value
        ? { transform: `translate(${marquee.value.x}px, ${marquee.value.y}px)`, width: `${marquee.value.width}px`, height: `${marquee.value.height}px` }
        : {},
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

const contentBounds = () => boundsOf(visibleNodes.value.map((node) => nodeRect(node, layouts.get(node.id))));

const fit = () => {
    setViewport(fitToContent(contentBounds(), hostSize));
    markZooming();
};

const zoomTo = (zoom: number) => {
    setViewport(zoomAt(viewport, zoom, { x: hostSize.width / 2, y: hostSize.height / 2 }));
    markZooming();
};

// Centering on an activity in a collapsed branch shows it first.
const centerOn = (activityId: string) => {
    const node = nodeById.value.get(activityId);

    if (!node) {
        return;
    }

    reveal([activityId]);

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
 * Pans, only as far as needed, so an activity is in view above the bottom `inset` pixels of the canvas, which the
 * activity panel covers when it opens over it.
 */
const keepVisible = (activityId: string, inset = 0) => {
    const node = nodeById.value.get(activityId);

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

const nodeElement = (activityId: string) => host.value?.querySelector<HTMLElement>(`[data-node-id="${cssEscape(activityId)}"]`);

// An activity that was just shown is rendered on the next tick.
const focusNode = async (activityId: string) => {
    if (!nodeElement(activityId)) {
        await nextTick();
    }

    nodeElement(activityId)?.focus();
};

// A hidden activity is never selected: selecting one (from the Issues tab, an undo...) shows it.
watch(
    () => [...state.selectedNodeIds],
    (ids) => {
        if (ids.some((id) => hidden.value.has(id))) {
            reveal(ids);
        }
    },
);

const nodeIdAt = (clientX: number, clientY: number, excludeId: string) => {
    const element = document.elementFromPoint?.(clientX, clientY)?.closest("[data-node-id]");
    const id = element?.getAttribute("data-node-id");

    return id && id !== excludeId ? id : null;
};

const closeMenu = () => {
    menu.value = null;
};

const notifyDeleted = (result: { nodes: number; transitions: number }) => {
    if (result.nodes === 0 && result.transitions === 0) {
        return;
    }

    showToast({
        message: result.nodes === 1 ? t("DeletedActivity") : result.nodes > 1 ? t("DeletedActivities", result.nodes) : t("DeletedConnection"),
        action: { label: t("Undo"), run: () => props.store.undo() },
    });
};

const deleteSelected = () => {
    if (!props.readOnly) {
        notifyDeleted(deleteSelection(props.store));
        host.value?.focus();
    }
};

/**
 * Deletes an activity, as the Delete key does on the selection, with a toast that can undo it.
 */
const deleteActivity = (activityId: string) => {
    selectNode(props.store, activityId);
    deleteSelected();
};

const connect = (sourceId: string, outcome: string, targetId: string) => {
    // A new connection from a collapsed activity would hide its target; the branch is expanded instead.
    if (collapsed.value.has(sourceId)) {
        expand(sourceId);
    }

    const result = connectOutcome(props.store, { sourceActivityId: sourceId, sourceOutcomeName: outcome, destinationActivityId: targetId });

    if (result.status === "replaced") {
        const source = nodeById.value.get(sourceId);
        const displayName = source?.outcomes.find((item) => item.name === outcome)?.displayName ?? outcome;

        showToast({
            message: t("ConnectionReplaced", displayName),
            variant: "warning",
            action: { label: t("Undo"), run: () => props.store.undo() },
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

            const inside = visibleNodes.value.filter((node) => rectsIntersect(rect, nodeRect(node, layouts.get(node.id)))).map((node) => node.id);
            state.selectedNodeIds = [...new Set([...initialSelection, ...inside])];
            state.selectedTransitionKey = null;
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

    // The viewer has nothing to move: a click shows the activity's panel.
    if (props.readOnly) {
        showActivity(props.store, node.id);

        return;
    }

    // A click only selects an activity, to move it around: its panel opens on a double-click or its Edit button.
    if (!wasSelected) {
        selectNode(props.store, node.id);
        state.activityPanelRequested = false;
    }

    const ids = [...state.selectedNodeIds];
    const origins = new Map(ids.map((id) => [id, { x: nodeById.value.get(id)!.x, y: nodeById.value.get(id)!.y }]));
    const key = `drag-${++dragCounter}`;

    startPointerDrag(event, {
        onStart: () => {
            liftedIds.value = ids.length <= LIFT_LIMIT ? new Set(ids) : new Set();

            // Moving activities around closes their panel (unless it's pinned), so it doesn't cover them.
            state.activityPanelRequested = false;
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
    // Tabbing to an activity selects it, so keyboard users can delete or nudge it; a click already did.
    if (!pointerFocus && !selectedIds.value.has(node.id)) {
        selectNode(props.store, node.id);
    }
};

const onPortPointerDown = (node: DesignerNode, outcome: string, event: PointerEvent) => {
    closeMenu();
    connecting.value = { sourceId: node.id, outcome, pointer: toCanvas(event), targetId: null };

    startPointerDrag(event, {
        threshold: 3,
        onMove: (_dx, _dy, moveEvent) => {
            if (connecting.value) {
                connecting.value.pointer = toCanvas(moveEvent);
                connecting.value.targetId = nodeIdAt(moveEvent.clientX, moveEvent.clientY, node.id);
            }
        },
        onEnd: (moved, endEvent) => {
            const connection = connecting.value;
            connecting.value = null;

            if (!connection || !endEvent) {
                return;
            }

            // A click on an outcome that leads nowhere yet adds the next activity after it.
            if (!moved) {
                if (!state.transitions.some((transition) => transition.sourceActivityId === node.id && transition.sourceOutcomeName === outcome)) {
                    emit("add-after", node.id, outcome, endEvent.clientX, endEvent.clientY);
                }

                return;
            }

            const targetId = nodeIdAt(endEvent.clientX, endEvent.clientY, node.id) ?? connection.targetId;

            if (targetId) {
                connect(node.id, outcome, targetId);
            }
        },
    });
};

const openConnectDialog = (node: DesignerNode, outcome?: string) => {
    if (!props.readOnly && state.nodes.length > 1 && node.outcomes.length > 0) {
        connectDialog.value = { sourceId: node.id, outcome };
    }
};

const onConnectDialogConfirm = (outcome: string, targetId: string) => {
    const sourceId = connectDialog.value!.sourceId;
    connectDialog.value = null;
    connect(sourceId, outcome, targetId);
    focusNode(sourceId);
};

const onConnectDialogCancel = () => {
    const sourceId = connectDialog.value?.sourceId;
    connectDialog.value = null;

    if (sourceId) {
        focusNode(sourceId);
    }
};

// Collapsing and expanding only change the view, so the read-only viewer offers them too.
const branchMenuItems = (node: DesignerNode): MenuItem[] => {
    const items: MenuItem[] = [];
    const hiddenCount = hiddenCounts.value.get(node.id);

    if (hiddenCount !== undefined) {
        items.push({ label: t("ExpandActivities", hiddenCount), icon: "fa-solid fa-square-plus", dataCy: "menu-expand", run: () => expand(node.id) });
    } else {
        const count = countHiddenBy(node.id);

        if (count > 0) {
            items.push({ label: t("CollapseActivities", count), icon: "fa-solid fa-square-minus", dataCy: "menu-collapse", run: () => collapse(node.id) });
        }
    }

    return items;
};

const openNodeMenu = (node: DesignerNode, event: MouseEvent | null) => {
    if (!selectedIds.value.has(node.id)) {
        selectNode(props.store, node.id);
    }

    const position = event
        ? hostPoint(event)
        : canvasToScreen(viewport, { x: node.x + (layouts.get(node.id)?.width ?? NODE_WIDTH) - 24, y: node.y + 24 });

    if (props.readOnly) {
        const items = branchMenuItems(node);
        menu.value = items.length > 0 ? { ...position, items, label: t("ActivityActions", node.title) } : null;

        return;
    }

    const items: MenuItem[] = [];

    if (!node.isMissing) {
        items.push({ label: t("Edit"), icon: "fa-solid fa-pen", dataCy: "menu-edit", run: () => emit("edit", node.id) });
    }

    if (node.isEvent) {
        items.push({
            label: node.isStart ? t("UnsetStart") : t("SetAsStart"),
            icon: "fa-solid fa-flag",
            dataCy: "menu-toggle-start",
            run: () => toggleStart(props.store, node.id),
        });
    }

    if (node.outcomes.length > 0 && state.nodes.length > 1) {
        items.push({ label: t("ConnectOutcomeTo"), icon: "fa-solid fa-link", dataCy: "menu-connect", run: () => openConnectDialog(node) });
    }

    items.push(...branchMenuItems(node));

    for (const transition of state.transitions.filter((item) => item.sourceActivityId === node.id)) {
        const target = nodeById.value.get(transition.destinationActivityId);
        const outcome = node.outcomes.find((item) => item.name === transition.sourceOutcomeName)?.displayName ?? transition.sourceOutcomeName;

        items.push({
            label: t("RemoveConnection", outcome, target?.title ?? transition.destinationActivityId),
            icon: "fa-solid fa-link-slash",
            dataCy: `menu-disconnect-${transition.sourceOutcomeName}`,
            run: () => {
                selectTransition(props.store, transitionKey(transition));
                deleteSelected();
            },
        });
    }

    items.push({
        label: state.selectedNodeIds.length > 1 ? t("DeleteSelected", state.selectedNodeIds.length) : t("Delete"),
        icon: "fa-solid fa-trash",
        danger: true,
        dataCy: "menu-delete",
        run: deleteSelected,
    });

    menu.value = { ...position, items, label: t("ActivityActions", node.title) };
};

const openEdgeMenu = (key: string, event: MouseEvent) => {
    if (props.readOnly) {
        return;
    }

    selectTransition(props.store, key);
    menu.value = {
        ...hostPoint(event),
        label: t("ConnectionActions"),
        items: [{ label: t("DeleteConnection"), icon: "fa-solid fa-trash", danger: true, dataCy: "menu-delete-edge", run: deleteSelected }],
    };
};

const isEditable = (target: EventTarget | null) =>
    target instanceof HTMLElement &&
    (target.isContentEditable ||
        ["INPUT", "TEXTAREA", "SELECT", "BUTTON"].includes(target.tagName) ||
        !!target.closest(".wfd-connect-dialog, .wfd-context-menu"));

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
                selectAll(props.store, (id) => !hidden.value.has(id));
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
    if (!props.readOnly && event.dataTransfer?.types.includes(ACTIVITY_DRAG_TYPE)) {
        event.preventDefault();
        event.dataTransfer.dropEffect = "copy";
    }
};

const onDrop = (event: DragEvent) => {
    const activityName = event.dataTransfer?.getData(ACTIVITY_DRAG_TYPE);

    if (props.readOnly || !activityName) {
        return;
    }

    event.preventDefault();
    emit("drop-activity", activityName, toCanvas(event));
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
    deleteActivity,
    reveal,
    expandAll,
    focus: () => host.value?.focus({ preventScroll: true }),
    zoomIn: () => zoomTo(viewport.zoom * ZOOM_STEP),
    zoomOut: () => zoomTo(viewport.zoom / ZOOM_STEP),
    resetZoom: () => zoomTo(1),
    visibleCenter: () => visibleCenter(viewport, hostSize),
    clientToCanvas: (clientX: number, clientY: number) => toCanvas({ clientX, clientY }),
    layoutOf: (activityId: string) => layouts.get(activityId),
});
</script>

<template>
    <div
        ref="host"
        class="wfd-canvas"
        :class="{ 'is-panning': panning, 'is-connecting': !!connecting, 'is-readonly': readOnly, 'is-space-held': spaceHeld }"
        tabindex="0"
        :aria-label="t('CanvasInstructions')"
        data-cy="canvas-surface"
        @pointerdown="onBackgroundPointerDown"
        @keydown="onKeyDown"
        @keyup="onKeyUp"
        @dragover="onDragOver"
        @drop="onDrop"
        @contextmenu.prevent
    >
        <div class="wfd-grid" :style="gridStyle" aria-hidden="true"></div>

        <div class="wfd-layer" :style="layerStyle">
            <svg class="wfd-edges" width="1" height="1">
                <defs>
                    <marker id="wfd-arrow" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse">
                        <path class="wfd-arrow" d="M0,0 L10,5 L0,10 z" />
                    </marker>
                    <marker id="wfd-arrow-selected" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse">
                        <path class="wfd-arrow is-selected" d="M0,0 L10,5 L0,10 z" />
                    </marker>
                </defs>
                <TransitionEdge
                    v-for="edge in staticEdges"
                    :key="edge.key"
                    :edge-key="edge.key"
                    :source="edge.source"
                    :target="edge.target"
                    :outcome="edge.outcome"
                    :source-layout="layouts.get(edge.source.id)"
                    :target-layout="layouts.get(edge.target.id)"
                    :selected="state.selectedTransitionKey === edge.key"
                    :read-only="readOnly"
                    :change="changes?.edges[edge.key] ?? null"
                    :executed-count="executedTransitions[edge.key] ?? 0"
                    @select="selectTransition(store, edge.key)"
                    @delete="deleteSelected"
                    @open-menu="openEdgeMenu(edge.key, $event)"
                />
                <path v-if="connectPreview" class="wfd-edge-preview" :d="connectPreview" data-cy="connect-preview" />
            </svg>

            <svg v-if="liftedEdges.length > 0" class="wfd-edges is-lifted" width="1" height="1">
                <TransitionEdge
                    v-for="edge in liftedEdges"
                    :key="edge.key"
                    :edge-key="edge.key"
                    :source="edge.source"
                    :target="edge.target"
                    :outcome="edge.outcome"
                    :source-layout="layouts.get(edge.source.id)"
                    :target-layout="layouts.get(edge.target.id)"
                    :read-only="readOnly"
                />
            </svg>

            <ActivityNode
                v-for="node in visibleNodes"
                :key="node.id"
                :node="node"
                :selected="selectedIds.has(node.id)"
                :lifted="liftedIds.has(node.id)"
                :drop-target="connecting?.targetId === node.id"
                :highlighted="highlighted.has(node.id)"
                :read-only="readOnly"
                :blocking="blockingIds.has(node.id)"
                :issues="issuesByActivity.get(node.id) ?? NO_ISSUES"
                :connected-outcomes="connectedOutcomes.get(node.id) ?? NO_OUTCOMES"
                :hidden-count="hiddenCounts.get(node.id) ?? null"
                :change="changes?.nodes[node.id] ?? null"
                :executed-count="executedCounts[node.id] ?? 0"
                :faulted="faultedId === node.id"
                :script-errors="scriptErrors.get(node.id) ?? NO_ERRORS"
                @node-pointerdown="onNodePointerDown(node, $event)"
                @port-pointerdown="(outcome, event) => onPortPointerDown(node, outcome, event)"
                @port-activate="(outcome) => openConnectDialog(node, outcome)"
                @measure="(layout) => layouts.set(node.id, layout)"
                @open-menu="(event) => openNodeMenu(node, event)"
                @edit="emit('edit', node.id)"
                @focus="onNodeFocus(node)"
                @expand="expand(node.id)"
            />

            <div v-if="marquee" class="wfd-marquee" :style="marqueeStyle" data-cy="marquee"></div>
        </div>

        <div v-if="state.loaded && state.nodes.length === 0" class="wfd-empty" data-cy="canvas-empty">
            {{ readOnly ? t("EmptyWorkflowReadOnly") : t("EmptyWorkflow") }}
        </div>

        <div v-if="hidden.size > 0" class="wfd-hidden-notice small shadow-sm" role="status" data-cy="hidden-notice" @pointerdown.stop>
            <i class="fa-solid fa-eye-slash" aria-hidden="true"></i>
            <span>{{ t("HiddenActivities", hidden.size) }}</span>
            <button type="button" class="btn btn-link btn-sm p-0" data-cy="expand-all" @click="expandAll">{{ t("ShowAll") }}</button>
        </div>

        <div class="wfd-zoom-controls btn-group btn-group-sm shadow-sm" role="group" :aria-label="t('Zoom')" @pointerdown.stop>
            <button type="button" class="btn btn-light" :title="t('ZoomOut')" :aria-label="t('ZoomOut')" data-cy="zoom-out" @click="zoomTo(viewport.zoom / ZOOM_STEP)">
                <i class="fa-solid fa-minus" aria-hidden="true"></i>
            </button>
            <button type="button" class="btn btn-light wfd-zoom-level" :title="t('ResetZoom')" data-cy="zoom-reset" aria-live="polite" @click="zoomTo(1)">
                {{ zoomPercent }}
            </button>
            <button type="button" class="btn btn-light" :title="t('ZoomIn')" :aria-label="t('ZoomIn')" data-cy="zoom-in" @click="zoomTo(viewport.zoom * ZOOM_STEP)">
                <i class="fa-solid fa-plus" aria-hidden="true"></i>
            </button>
            <button type="button" class="btn btn-light" :title="t('FitToContent')" :aria-label="t('FitToContent')" data-cy="zoom-fit" @click="fit">
                <i class="fa-solid fa-expand" aria-hidden="true"></i>
            </button>
        </div>

        <ContextMenu v-if="menu" :x="menu.x" :y="menu.y" :items="menu.items" :label="menu.label" @close="closeMenu" />

        <ConnectDialog
            v-if="connectDialogSource"
            :source="connectDialogSource"
            :nodes="state.nodes"
            :outcome="connectDialog?.outcome"
            @connect="onConnectDialogConfirm"
            @cancel="onConnectDialogCancel"
        />
    </div>
</template>
