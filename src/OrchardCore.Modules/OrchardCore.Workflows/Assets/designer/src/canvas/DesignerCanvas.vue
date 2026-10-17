<script setup lang="ts">
import { computed, ref, watch } from "vue";
import GraphCanvas from "@bloom/designer/canvas/GraphCanvas.vue";
import type { CanvasEdge, GraphCanvasApi } from "@bloom/designer/canvas/canvasTypes";
import type { MenuItem } from "@bloom/designer/canvas/menu";
import type { NodeMove } from "@bloom/designer/graph/commands";
import { showToast } from "@bloom/designer/ui/toasts";
import type { DesignIssue, DesignerNode, DesignerTransition } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { moveNodesCommand, transitionKey } from "../state/commands";
import ActivityNode from "./ActivityNode.vue";
import TransitionEdge from "./TransitionEdge.vue";
import ConnectDialog from "./ConnectDialog.vue";
import type { CanvasChanges } from "./changes";
import { nodeRect, portAnchor, previewPath, type NodeLayout, type Point } from "./geometry";
import { clearSelection, connectOutcome, deleteSelection, nudgeSelection, selectAll, selectNode, selectTransition, showActivity, toggleStart } from "./useConnect";
import { useCollapsedBranches } from "./useCollapsedBranches";
import { scriptErrorsByActivity } from "./scriptErrors";
import { ACTIVITY_DRAG_TYPE } from "../toolbox/filter";
import { t } from "../i18n";

// The workflow canvas: the activities and their transitions on the shared canvas surface (GraphCanvas), with the
// collapsed branches, the instance being viewed, the differences being compared, and the actions of a workflow.
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
const canvas = ref<GraphCanvasApi<NodeLayout> | null>(null);
const connectDialog = ref<{ sourceId: string; outcome?: string } | null>(null);

const nodeById = computed(() => new Map(state.nodes.map((node) => [node.id, node])));
const { collapsed, hidden, hiddenCounts, countHiddenBy, collapse, expand, expandAll, reveal } = useCollapsedBranches(props.store);
const highlighted = computed(() => new Set(props.highlightedIds));

// The activities the instance shown by the viewer waits on.
// TODO: Phase 5 records the executed activities; highlight the executed path here then.
const blockingIds = computed(() => new Set(state.instance?.blockingActivityIds ?? []));

// What the instance shown by the viewer ran, from its journal.
const executedCounts = computed(() => state.instance?.executedActivityCounts ?? {});
const executedTransitions = computed(() => state.instance?.executedTransitionCounts ?? {});
const faultedId = computed(() => state.instance?.faultedActivityId ?? null);
const scriptErrors = computed(() => scriptErrorsByActivity(state.instance));

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
            edge: transition,
            source: nodeById.value.get(transition.sourceActivityId),
            target: nodeById.value.get(transition.destinationActivityId),
        }))
        .filter(
            (edge): edge is CanvasEdge<DesignerNode, DesignerTransition> =>
                !!edge.source && !!edge.target && !hidden.value.has(edge.source.id) && !hidden.value.has(edge.target.id),
        ),
);

const anchorOf = (node: DesignerNode, layout: NodeLayout | undefined, outcome: string) => {
    const index = node.outcomes.findIndex((item) => item.name === outcome);

    return portAnchor(node, layout, outcome, index, node.outcomes.length);
};

const connectDialogSource = computed(() => (connectDialog.value ? nodeById.value.get(connectDialog.value.sourceId) : undefined));

const focusNode = (activityId: string) => canvas.value?.focusNode(activityId);

// Centering on an activity in a collapsed branch shows it first.
const centerOn = (activityId: string) => {
    reveal([activityId]);
    canvas.value?.centerOn(activityId);
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
        canvas.value?.focus();
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

const onSelectNode = (id: string, additive: boolean, pointer: boolean) => {
    selectNode(props.store, id, additive);

    // A click only selects an activity, to move it around: its panel opens on a double-click or its Edit button.
    if (pointer && !additive) {
        state.activityPanelRequested = false;
    }
};

const onSelectNodes = (ids: string[]) => {
    state.selectedNodeIds = ids;
    state.selectedTransitionKey = null;
};

const onMoveNodes = (moves: NodeMove[], key: string) => {
    props.store.execute(moveNodesCommand(props.store.graph, moves, key));
};

// Moving activities around closes their panel (unless it's pinned), so it doesn't cover them.
const onDragStart = () => {
    state.activityPanelRequested = false;
};

// A click on an outcome that leads nowhere yet adds the next activity after it.
const onPortClick = (activityId: string, outcome: string, clientX: number, clientY: number) => {
    if (!state.transitions.some((transition) => transition.sourceActivityId === activityId && transition.sourceOutcomeName === outcome)) {
        emit("add-after", activityId, outcome, clientX, clientY);
    }
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
    if (!state.selectedNodeIds.includes(node.id)) {
        selectNode(props.store, node.id);
    }

    const at = event ?? { nodeId: node.id };

    if (props.readOnly) {
        canvas.value?.openMenu(branchMenuItems(node), t("ActivityActions", node.title), at);

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

    canvas.value?.openMenu(items, t("ActivityActions", node.title), at);
};

const openEdgeMenu = (key: string, event: MouseEvent) => {
    if (props.readOnly) {
        return;
    }

    selectTransition(props.store, key);
    canvas.value?.openMenu(
        [{ label: t("DeleteConnection"), icon: "fa-solid fa-trash", danger: true, dataCy: "menu-delete-edge", run: deleteSelected }],
        t("ConnectionActions"),
        event,
    );
};

defineExpose({
    fit: () => canvas.value?.fit(),
    centerOn,
    focusNode,
    keepVisible: (activityId: string, inset = 0) => canvas.value?.keepVisible(activityId, inset),
    deleteActivity,
    reveal,
    expandAll,
    focus: () => canvas.value?.focus(),
    zoomIn: () => canvas.value?.zoomIn(),
    zoomOut: () => canvas.value?.zoomOut(),
    resetZoom: () => canvas.value?.resetZoom(),
    visibleCenter: () => canvas.value!.visibleCenter(),
    clientToCanvas: (clientX: number, clientY: number) => canvas.value!.clientToCanvas(clientX, clientY),
    layoutOf: (activityId: string) => canvas.value?.layoutOf(activityId),
});
</script>

<template>
    <GraphCanvas
        ref="canvas"
        :nodes="state.nodes"
        :edges="edges"
        :hidden-ids="hidden"
        :selected-ids="state.selectedNodeIds"
        :viewport="state.viewport"
        :node-rect="nodeRect"
        :port-anchor="anchorOf"
        :preview-path="previewPath"
        :drag-type="ACTIVITY_DRAG_TYPE"
        keyboard-ignore=".wfd-connect-dialog"
        :label="t('CanvasInstructions')"
        :read-only="readOnly"
        @select-node="onSelectNode"
        @select-nodes="onSelectNodes"
        @select-all="selectAll(store, (id) => !hidden.has(id))"
        @clear-selection="clearSelection(store)"
        @show-node="showActivity(store, $event)"
        @drag-start="onDragStart"
        @move-nodes="onMoveNodes"
        @nudge="(dx, dy) => nudgeSelection(store, dx, dy)"
        @delete-selection="deleteSelected"
        @connect="(sourceId, outcome, targetId) => connect(sourceId, outcome, targetId)"
        @port-click="onPortClick"
        @drop="(activityName, point) => emit('drop-activity', activityName, point)"
    >
        <template #edge="{ edge, lifted, sourceLayout, targetLayout }">
            <TransitionEdge
                v-if="lifted"
                :edge-key="edge.key"
                :source="edge.source"
                :target="edge.target"
                :outcome="edge.edge.sourceOutcomeName"
                :source-layout="sourceLayout"
                :target-layout="targetLayout"
                :read-only="readOnly"
            />
            <TransitionEdge
                v-else
                :edge-key="edge.key"
                :source="edge.source"
                :target="edge.target"
                :outcome="edge.edge.sourceOutcomeName"
                :source-layout="sourceLayout"
                :target-layout="targetLayout"
                :selected="state.selectedTransitionKey === edge.key"
                :read-only="readOnly"
                :change="changes?.edges[edge.key] ?? null"
                :executed-count="executedTransitions[edge.key] ?? 0"
                @select="selectTransition(store, edge.key)"
                @delete="deleteSelected"
                @open-menu="openEdgeMenu(edge.key, $event)"
            />
        </template>

        <template #node="{ node, selected, lifted, dropTarget, onNodePointerdown, onPortPointerdown, onMeasure, onFocus }">
            <ActivityNode
                :node="node"
                :selected="selected"
                :lifted="lifted"
                :drop-target="dropTarget"
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
                @node-pointerdown="onNodePointerdown"
                @port-pointerdown="onPortPointerdown"
                @port-activate="(outcome) => openConnectDialog(node, outcome)"
                @measure="onMeasure"
                @open-menu="(event) => openNodeMenu(node, event)"
                @edit="emit('edit', node.id)"
                @focus="onFocus"
                @expand="expand(node.id)"
            />
        </template>

        <div v-if="state.loaded && state.nodes.length === 0" class="ocd-empty" data-cy="canvas-empty">
            {{ readOnly ? t("EmptyWorkflowReadOnly") : t("EmptyWorkflow") }}
        </div>

        <div v-if="hidden.size > 0" class="wfd-hidden-notice small shadow-sm" role="status" data-cy="hidden-notice" @pointerdown.stop>
            <i class="fa-solid fa-eye-slash" aria-hidden="true"></i>
            <span>{{ t("HiddenActivities", hidden.size) }}</span>
            <button type="button" class="btn btn-link btn-sm p-0" data-cy="expand-all" @click="expandAll">{{ t("ShowAll") }}</button>
        </div>

        <ConnectDialog
            v-if="connectDialogSource"
            :source="connectDialogSource"
            :nodes="state.nodes"
            :outcome="connectDialog?.outcome"
            @connect="onConnectDialogConfirm"
            @cancel="onConnectDialogCancel"
        />
    </GraphCanvas>
</template>
