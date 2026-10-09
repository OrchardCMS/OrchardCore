import type { DesignerTransition } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import {
    canConnect,
    connectCommand,
    disconnectCommand,
    moveNodesCommand,
    removeNodesCommand,
    setStartCommand,
    transitionKey,
} from "../state/commands";

// The graph actions of the canvas, shared by pointer, keyboard and context-menu interactions.

export type ConnectResult =
    | { status: "connected" }
    | { status: "replaced"; replaced: DesignerTransition }
    | { status: "unchanged" }
    | { status: "rejected" };

/**
 * Connects an outcome to an activity. Unless the workflow follows every transition of an outcome (its branching
 * mode), an outcome has one transition at most, so an existing transition of the outcome is replaced. Self-loops
 * and unknown activities or outcomes are rejected.
 */
export const connectOutcome = (store: DesignerStore, transition: DesignerTransition): ConnectResult => {
    if (!canConnect(store.graph, transition)) {
        return { status: "rejected" };
    }

    if (store.state.transitions.some((existing) => transitionKey(existing) === transitionKey(transition))) {
        return { status: "unchanged" };
    }

    const command = connectCommand(store.graph, transition, store.state.settings?.branchingMode === "All");
    store.execute(command);

    const replaced = command.replaced();

    return replaced ? { status: "replaced", replaced } : { status: "connected" };
};

export interface DeleteResult {
    nodes: number;
    transitions: number;
}

/**
 * Deletes the selected activities (with their transitions), or else the selected transition.
 */
export const deleteSelection = (store: DesignerStore): DeleteResult => {
    const ids = [...store.state.selectedNodeIds];

    if (ids.length > 0) {
        const attached = store.state.transitions.filter(
            (transition) => ids.includes(transition.sourceActivityId) || ids.includes(transition.destinationActivityId),
        ).length;

        store.execute(removeNodesCommand(store.graph, ids));
        store.state.selectedNodeIds = [];

        return { nodes: ids.length, transitions: attached };
    }

    const key = store.state.selectedTransitionKey;

    if (key) {
        store.execute(disconnectCommand(store.graph, key));
        store.state.selectedTransitionKey = null;

        return { nodes: 0, transitions: 1 };
    }

    return { nodes: 0, transitions: 0 };
};

/**
 * Moves the selected activities by (dx, dy). Consecutive nudges of the same selection make one undo entry.
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

/**
 * Selects every activity, or only those `include` accepts (the canvas leaves out hidden ones).
 */
export const selectAll = (store: DesignerStore, include: (id: string) => boolean = () => true) => {
    store.state.selectedNodeIds = store.state.nodes.map((node) => node.id).filter(include);
    store.state.selectedTransitionKey = null;
};

export const clearSelection = (store: DesignerStore) => {
    store.state.selectedNodeIds = [];
    store.state.selectedTransitionKey = null;
    store.state.activityPanelRequested = false;
};

/**
 * Selects one activity, or adds/removes it from the selection when `toggle` is set (shift-click).
 */
export const selectNode = (store: DesignerStore, id: string, toggle = false) => {
    const selected = store.state.selectedNodeIds;

    if (toggle) {
        store.state.selectedNodeIds = selected.includes(id) ? selected.filter((existing) => existing !== id) : [...selected, id];
    } else if (!(selected.length === 1 && selected[0] === id)) {
        store.state.selectedNodeIds = [id];
    }

    store.state.selectedTransitionKey = null;
};

/**
 * Selects one activity and shows its panel.
 */
export const showActivity = (store: DesignerStore, id: string) => {
    selectNode(store, id);
    store.state.activityPanelRequested = true;
};

export const selectTransition = (store: DesignerStore, key: string) => {
    store.state.selectedNodeIds = [];
    store.state.selectedTransitionKey = key;
};

export const toggleStart = (store: DesignerStore, id: string) => {
    const node = store.getNode(id);

    if (node?.isEvent) {
        store.execute(setStartCommand(store.graph, id, !node.isStart));
    }
};
