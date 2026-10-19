<script setup lang="ts">
import { computed, ref } from "vue";
import GraphCanvas from "@bloom/designer/canvas/GraphCanvas.vue";
import type { CanvasEdge, ConnectTarget, Connecting, GraphCanvasApi } from "@bloom/designer/canvas/canvasTypes";
import type { MenuItem } from "@bloom/designer/canvas/menu";
import { showToast } from "@bloom/designer/ui/toasts";
import type { DesignIssue, DesignerConnection, DesignerNode } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { moveNodesCommand, type NodeMove } from "../state/commands";
import { connectionKey, findOutput, firstCompatibleInput, rejectConnection, type ConnectRejection } from "../state/ports";
import StepNode from "./StepNode.vue";
import ConnectionEdge from "./ConnectionEdge.vue";
import ConnectDialog from "./ConnectDialog.vue";
import { nodeRect, portAnchor, previewPath, type NodeLayout, type Point } from "./geometry";
import { clearSelection, connectPorts, deleteSelection, nudgeSelection, selectAll, selectConnection, selectNode, showStep } from "./useConnect";
import { runOverlay } from "../runs/runOverlay";
import { STEP_DRAG_TYPE } from "../toolbox/filter";
import { t } from "../i18n";

// The pipeline canvas: the steps and their connections on the shared canvas surface (GraphCanvas), with the rules of
// the ports, the run being shown, and the actions of a pipeline.
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

const state = props.store.state;
const canvas = ref<GraphCanvasApi<NodeLayout> | null>(null);
const connectDialog = ref<{ sourceId: string; port?: string } | null>(null);

const nodeById = computed(() => new Map(state.nodes.map((node) => [node.id, node])));
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

// While a connection is drawn, which inputs of a step can receive it.
const compatibility = (node: DesignerNode, connecting: Connecting | null) => {
    if (!connecting || node.id === connecting.sourceId) {
        return null;
    }

    return Object.fromEntries(
        node.inputs.map((input) => [
            input.name,
            rejectConnection(state.nodes, state.connections, { sourceStepId: connecting.sourceId, sourcePort: connecting.port, targetStepId: node.id, targetPort: input.name }) ===
                null,
        ]),
    );
};

const edges = computed(() =>
    state.connections
        .map((connection) => ({
            key: connectionKey(connection),
            edge: connection,
            source: nodeById.value.get(connection.sourceStepId),
            target: nodeById.value.get(connection.targetStepId),
        }))
        .filter((edge): edge is CanvasEdge<DesignerNode, DesignerConnection> => !!edge.source && !!edge.target),
);

const anchorOf = (node: DesignerNode, layout: NodeLayout | undefined, port: string) => {
    const index = node.outputs.findIndex((item) => item.name === port);

    return portAnchor(node, layout, "output", port, index, node.outputs.length);
};

/**
 * The input a connection drawn from an output reaches on a step: the input port under the pointer, or the first input
 * of the right kind of the step.
 */
const resolveTarget = (sourceId: string, port: string, target: DesignerNode, targetPort: string | null): ConnectTarget => {
    const kind = findOutput(nodeById.value.get(sourceId), port)?.kind;
    const portName = targetPort ?? (kind ? firstCompatibleInput(target, kind, state.connections)?.name : undefined) ?? null;
    const valid =
        !!portName && rejectConnection(state.nodes, state.connections, { sourceStepId: sourceId, sourcePort: port, targetStepId: target.id, targetPort: portName }) === null;

    return { port: portName, valid };
};

const connectDialogSource = computed(() => (connectDialog.value ? nodeById.value.get(connectDialog.value.sourceId) : undefined));

const focusNode = (stepId: string) => canvas.value?.focusNode(stepId);

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
        canvas.value?.focus();
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

const onConnect = (sourceId: string, port: string, targetId: string, targetPort: string | null) => {
    if (targetPort) {
        connect({ sourceStepId: sourceId, sourcePort: port, targetStepId: targetId, targetPort });
    } else {
        showToast({ message: t("NoCompatibleInput", "This step has no input for this output."), variant: "warning" });
    }
};

const onSelectNode = (id: string, additive: boolean, pointer: boolean) => {
    selectNode(props.store, id, additive);

    // A click only selects a step, to move it around: its panel opens on a double-click or its Edit button.
    if (pointer && !additive) {
        state.stepPanelRequested = false;
    }
};

const onSelectNodes = (ids: string[]) => {
    state.selectedNodeIds = ids;
    state.selectedConnectionKey = null;
};

const onMoveNodes = (moves: NodeMove[], key: string) => {
    props.store.execute(moveNodesCommand(props.store.graph, moves, key));
};

// Moving steps around closes their panel (unless it's pinned), so it doesn't cover them.
const onDragStart = () => {
    state.stepPanelRequested = false;
};

// A click on an output that leads nowhere yet adds the next step after it.
const onPortClick = (stepId: string, port: string, clientX: number, clientY: number) => {
    if (!state.connections.some((item) => item.sourceStepId === stepId && item.sourcePort === port)) {
        emit("add-after", stepId, port, clientX, clientY);
    }
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
    const anchor = canvas.value?.canvasToClient(anchorOf(node, canvas.value?.layoutOf(node.id), port)) ?? { x: 0, y: 0 };

    emit("add-after", node.id, port, anchor.x, anchor.y);
};

const openNodeMenu = (node: DesignerNode, event: MouseEvent | null) => {
    if (!state.selectedNodeIds.includes(node.id)) {
        selectNode(props.store, node.id);
    }

    const items: MenuItem[] = [];

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

    canvas.value?.openMenu(items, t("StepActions", "Actions of {0}", node.displayText), event ?? { nodeId: node.id });
};

const openEdgeMenu = (key: string, event: MouseEvent) => {
    if (props.readOnly) {
        return;
    }

    selectConnection(props.store, key);
    canvas.value?.openMenu(
        [{ label: t("DeleteConnection", "Delete the connection"), icon: "fa-solid fa-trash", danger: true, dataCy: "menu-delete-edge", run: deleteSelected }],
        t("ConnectionActions", "Actions of the connection"),
        event,
    );
};

defineExpose({
    fit: () => canvas.value?.fit(),
    centerOn: (stepId: string) => canvas.value?.centerOn(stepId),
    focusNode,
    keepVisible: (stepId: string, inset = 0) => canvas.value?.keepVisible(stepId, inset),
    deleteStep,
    focus: () => canvas.value?.focus(),
    zoomIn: () => canvas.value?.zoomIn(),
    zoomOut: () => canvas.value?.zoomOut(),
    resetZoom: () => canvas.value?.resetZoom(),
    visibleCenter: () => canvas.value!.visibleCenter(),
    clientToCanvas: (clientX: number, clientY: number) => canvas.value!.clientToCanvas(clientX, clientY),
    layoutOf: (stepId: string) => canvas.value?.layoutOf(stepId),
});
</script>

<template>
    <GraphCanvas
        ref="canvas"
        :nodes="state.nodes"
        :edges="edges"
        :selected-ids="state.selectedNodeIds"
        :viewport="state.viewport"
        :node-rect="nodeRect"
        :port-anchor="anchorOf"
        :preview-path="previewPath"
        :resolve-target="resolveTarget"
        :drag-type="STEP_DRAG_TYPE"
        keyboard-ignore=".dpd-connect-dialog"
        :label="
            t(
                'CanvasInstructions',
                'Pipeline canvas. Tab to a step; Enter opens it, the arrow keys move it, Delete removes it, Shift+F10 shows its actions. Ctrl+Z undoes.',
            )
        "
        :read-only="readOnly"
        @select-node="onSelectNode"
        @select-nodes="onSelectNodes"
        @select-all="selectAll(store)"
        @clear-selection="clearSelection(store)"
        @show-node="showStep(store, $event)"
        @drag-start="onDragStart"
        @move-nodes="onMoveNodes"
        @nudge="(dx, dy) => nudgeSelection(store, dx, dy)"
        @delete-selection="deleteSelected"
        @connect="onConnect"
        @port-click="onPortClick"
        @drop="(type, point) => emit('drop-step', type, point)"
    >
        <template #edge="{ edge, lifted, sourceLayout, targetLayout }">
            <ConnectionEdge
                v-if="lifted"
                :edge-key="edge.key"
                :connection="edge.edge"
                :source="edge.source"
                :target="edge.target"
                :source-layout="sourceLayout"
                :target-layout="targetLayout"
                :read-only="readOnly"
            />
            <ConnectionEdge
                v-else
                :edge-key="edge.key"
                :connection="edge.edge"
                :source="edge.source"
                :target="edge.target"
                :source-layout="sourceLayout"
                :target-layout="targetLayout"
                :selected="state.selectedConnectionKey === edge.key"
                :read-only="readOnly"
                @select="selectConnection(store, edge.key)"
                @delete="deleteSelected"
                @open-menu="openEdgeMenu(edge.key, $event)"
            />
        </template>

        <template #node="{ node, selected, lifted, dropTarget, connecting, onNodePointerdown, onPortPointerdown, onMeasure, onFocus }">
            <StepNode
                :node="node"
                :selected="selected"
                :lifted="lifted"
                :drop-target="dropTarget"
                :read-only="readOnly"
                :issues="issuesByStep.get(node.id) ?? NO_ISSUES"
                :connected-inputs="connectedPorts.inputs.get(node.id) ?? NO_PORTS"
                :connected-outputs="connectedPorts.outputs.get(node.id) ?? NO_PORTS"
                :compatibility="compatibility(node, connecting)"
                :hovered-input="connecting?.targetId === node.id ? connecting.targetPort : null"
                :overlay="overlays[node.id] ?? null"
                @node-pointerdown="onNodePointerdown"
                @port-pointerdown="(port, event) => findOutput(node, port) && onPortPointerdown(port, event)"
                @port-activate="(port) => openConnectDialog(node, port)"
                @measure="onMeasure"
                @open-menu="(event) => openNodeMenu(node, event)"
                @edit="emit('edit', node.id)"
                @focus="onFocus"
            />
        </template>

        <div v-if="state.loaded && state.nodes.length === 0" class="ocd-empty" data-cy="canvas-empty">
            {{ readOnly ? t("EmptyPipelineReadOnly", "This pipeline has no steps.") : t("EmptyPipeline", "Drag a source from the toolbox to start the pipeline.") }}
        </div>

        <ConnectDialog
            v-if="connectDialogSource"
            :source="connectDialogSource"
            :nodes="state.nodes"
            :connections="state.connections"
            :port="connectDialog?.port"
            @connect="onConnectDialogConfirm"
            @cancel="onConnectDialogCancel"
        />
    </GraphCanvas>
</template>
