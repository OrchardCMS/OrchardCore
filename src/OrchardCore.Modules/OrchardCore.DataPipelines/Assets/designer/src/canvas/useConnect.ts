import type { DesignerConnection } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { connectCommand, disconnectCommand, moveNodesCommand, removeNodesCommand } from "../state/commands";
import { connectionKey, rejectConnection, type ConnectRejection } from "../state/ports";

// The graph actions of the canvas, shared by pointer, keyboard and context-menu interactions.

export type ConnectResult =
    | { status: "connected" }
    | { status: "replaced"; replaced: DesignerConnection[] }
    | { status: "unchanged" }
    | { status: "rejected"; reason: ConnectRejection };

/**
 * Connects an output port to an input port of the same kind. An input that accepts one connection keeps the new one,
 * replacing the existing one (undoable). Connections between different kinds, to the same step or making a loop are
 * rejected.
 */
export const connectPorts = (store: DesignerStore, connection: DesignerConnection): ConnectResult => {
    const reason = rejectConnection(store.state.nodes, store.state.connections, connection);

    if (reason) {
        return { status: "rejected", reason };
    }

    if (store.state.connections.some((existing) => connectionKey(existing) === connectionKey(connection))) {
        return { status: "unchanged" };
    }

    const command = connectCommand(store.graph, connection);
    store.execute(command);

    const replaced = command.replaced();

    return replaced.length > 0 ? { status: "replaced", replaced } : { status: "connected" };
};

export interface DeleteResult {
    nodes: number;
    connections: number;
}

/**
 * Deletes the selected steps (with their connections), or else the selected connection.
 */
export const deleteSelection = (store: DesignerStore): DeleteResult => {
    const ids = [...store.state.selectedNodeIds];

    if (ids.length > 0) {
        const attached = store.state.connections.filter((connection) => ids.includes(connection.sourceStepId) || ids.includes(connection.targetStepId)).length;

        store.execute(removeNodesCommand(store.graph, ids));
        store.state.selectedNodeIds = [];

        return { nodes: ids.length, connections: attached };
    }

    const key = store.state.selectedConnectionKey;

    if (key) {
        store.execute(disconnectCommand(store.graph, key));
        store.state.selectedConnectionKey = null;

        return { nodes: 0, connections: 1 };
    }

    return { nodes: 0, connections: 0 };
};

/**
 * Moves the selected steps by (dx, dy). Consecutive nudges of the same selection make one undo entry.
 */
export const nudgeSelection = (store: DesignerStore, dx: number, dy: number) => {
    const ids = store.state.selectedNodeIds;

    if (ids.length === 0) {
        return;
    }

    const moves = ids
        .map((id) => store.getNode(id))
        .filter((node) => node !== undefined)
        .map((node) => ({ id: node.id, fromX: node.x, fromY: node.y, toX: node.x + dx, toY: node.y + dy }));

    store.execute(moveNodesCommand(store.graph, moves, `nudge:${[...ids].sort().join(",")}`));
};

export const selectAll = (store: DesignerStore) => {
    store.state.selectedNodeIds = store.state.nodes.map((node) => node.id);
    store.state.selectedConnectionKey = null;
};

export const clearSelection = (store: DesignerStore) => {
    store.state.selectedNodeIds = [];
    store.state.selectedConnectionKey = null;
    store.state.stepPanelRequested = false;
};

/**
 * Selects one step, or adds/removes it from the selection when `toggle` is set (shift-click).
 */
export const selectNode = (store: DesignerStore, id: string, toggle = false) => {
    const selected = store.state.selectedNodeIds;

    if (toggle) {
        store.state.selectedNodeIds = selected.includes(id) ? selected.filter((existing) => existing !== id) : [...selected, id];
    } else if (!(selected.length === 1 && selected[0] === id)) {
        store.state.selectedNodeIds = [id];
    }

    store.state.selectedConnectionKey = null;
};

/**
 * Selects one step and shows its panel.
 */
export const showStep = (store: DesignerStore, id: string) => {
    selectNode(store, id);
    store.state.stepPanelRequested = true;
};

export const selectConnection = (store: DesignerStore, key: string) => {
    store.state.selectedNodeIds = [];
    store.state.selectedConnectionKey = key;
};
