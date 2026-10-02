import type { DesignerNode, DesignerTransition } from "../api/types";
import type { Command } from "./history";

/**
 * The part of the designer state the commands change. The arrays are the store's reactive arrays.
 */
export interface Graph {
    readonly nodes: DesignerNode[];
    readonly transitions: DesignerTransition[];
}

/**
 * The key of a transition, the same format as WorkflowDesignIssue.GetTransitionKey on the server.
 */
export const transitionKey = (transition: DesignerTransition) =>
    `${transition.sourceActivityId}:${transition.sourceOutcomeName}:${transition.destinationActivityId}`;

const indexOfNode = (graph: Graph, id: string) => graph.nodes.findIndex((node) => node.id === id);

const indexOfTransition = (graph: Graph, key: string) => graph.transitions.findIndex((transition) => transitionKey(transition) === key);

/**
 * Returns whether a transition may be drawn: both activities exist, the outcome exists on the source, and
 * it isn't a self-loop (the engine never follows those).
 */
export const canConnect = (graph: Graph, transition: DesignerTransition) => {
    if (transition.sourceActivityId === transition.destinationActivityId) {
        return false;
    }

    const source = graph.nodes.find((node) => node.id === transition.sourceActivityId);

    return (
        source !== undefined &&
        source.outcomes.some((outcome) => outcome.name === transition.sourceOutcomeName) &&
        graph.nodes.some((node) => node.id === transition.destinationActivityId)
    );
};

export const addNodeCommand = (graph: Graph, node: DesignerNode): Command => ({
    label: "add",
    apply() {
        if (indexOfNode(graph, node.id) < 0) {
            graph.nodes.push(node);
        }
    },
    revert() {
        const index = indexOfNode(graph, node.id);

        if (index >= 0) {
            graph.nodes.splice(index, 1);
        }

        for (let i = graph.transitions.length - 1; i >= 0; i--) {
            const transition = graph.transitions[i];

            if (transition.sourceActivityId === node.id || transition.destinationActivityId === node.id) {
                graph.transitions.splice(i, 1);
            }
        }
    },
});

/**
 * Removes activities and every transition attached to them. Reverting puts everything back at its index.
 */
export const removeNodesCommand = (graph: Graph, ids: string[]): Command => {
    const removedIds = new Set(ids);
    let nodes: { index: number; node: DesignerNode }[] = [];
    let transitions: { index: number; transition: DesignerTransition }[] = [];

    return {
        label: "remove",
        apply() {
            nodes = graph.nodes.map((node, index) => ({ index, node })).filter((entry) => removedIds.has(entry.node.id));
            transitions = graph.transitions
                .map((transition, index) => ({ index, transition }))
                .filter((entry) => removedIds.has(entry.transition.sourceActivityId) || removedIds.has(entry.transition.destinationActivityId));

            for (const entry of [...transitions].reverse()) {
                graph.transitions.splice(entry.index, 1);
            }

            for (const entry of [...nodes].reverse()) {
                graph.nodes.splice(entry.index, 1);
            }
        },
        revert() {
            for (const entry of nodes) {
                graph.nodes.splice(entry.index, 0, entry.node);
            }

            for (const entry of transitions) {
                graph.transitions.splice(entry.index, 0, entry.transition);
            }
        },
    };
};

export interface NodeMove {
    id: string;
    fromX: number;
    fromY: number;
    toX: number;
    toY: number;
}

/**
 * Moves activities. Moves with the same `coalesceKey` (one drag, or repeated nudges) merge into one entry.
 */
export const moveNodesCommand = (graph: Graph, moves: NodeMove[], coalesceKey?: string): Command => {
    const current = moves.map((move) => ({ ...move }));
    const sameNodes = (other: NodeMove[]) =>
        other.length === current.length && other.every((move) => current.some((entry) => entry.id === move.id));

    const command: Command & { moves: NodeMove[] } = {
        label: "move",
        coalesceKey,
        moves: current,
        apply() {
            for (const move of current) {
                const node = graph.nodes[indexOfNode(graph, move.id)];

                if (node) {
                    node.x = move.toX;
                    node.y = move.toY;
                }
            }
        },
        revert() {
            for (const move of current) {
                const node = graph.nodes[indexOfNode(graph, move.id)];

                if (node) {
                    node.x = move.fromX;
                    node.y = move.fromY;
                }
            }
        },
        absorb(next) {
            const nextMoves = (next as Partial<typeof command>).moves;

            if (!nextMoves || !sameNodes(nextMoves)) {
                return false;
            }

            for (const move of nextMoves) {
                const entry = current.find((candidate) => candidate.id === move.id)!;
                entry.toX = move.toX;
                entry.toY = move.toY;
            }

            return true;
        },
    };

    return command;
};

/**
 * Adds a transition. An outcome has at most one transition (the engine only follows the first one), so an
 * existing transition of the same outcome is replaced; reverting restores it.
 */
export const connectCommand = (graph: Graph, transition: DesignerTransition): Command & { replaced: () => DesignerTransition | undefined } => {
    let replaced: { index: number; transition: DesignerTransition } | undefined;

    return {
        label: "connect",
        replaced: () => replaced?.transition,
        apply() {
            const index = graph.transitions.findIndex(
                (existing) => existing.sourceActivityId === transition.sourceActivityId && existing.sourceOutcomeName === transition.sourceOutcomeName,
            );

            replaced = index >= 0 ? { index, transition: graph.transitions[index] } : undefined;

            if (replaced) {
                graph.transitions.splice(index, 1);
            }

            graph.transitions.push({ ...transition });
        },
        revert() {
            const index = indexOfTransition(graph, transitionKey(transition));

            if (index >= 0) {
                graph.transitions.splice(index, 1);
            }

            if (replaced) {
                graph.transitions.splice(replaced.index, 0, replaced.transition);
            }
        },
    };
};

export const disconnectCommand = (graph: Graph, key: string): Command => {
    let removed: { index: number; transition: DesignerTransition } | undefined;

    return {
        label: "disconnect",
        apply() {
            const index = indexOfTransition(graph, key);
            removed = index >= 0 ? { index, transition: graph.transitions[index] } : undefined;

            if (removed) {
                graph.transitions.splice(index, 1);
            }
        },
        revert() {
            if (removed) {
                graph.transitions.splice(removed.index, 0, removed.transition);
            }
        },
    };
};

export const setStartCommand = (graph: Graph, id: string, isStart: boolean): Command => {
    let previous = false;

    return {
        label: "setStart",
        apply() {
            const node = graph.nodes[indexOfNode(graph, id)];

            if (node) {
                previous = node.isStart;
                node.isStart = isStart;
            }
        },
        revert() {
            const node = graph.nodes[indexOfNode(graph, id)];

            if (node) {
                node.isStart = previous;
            }
        },
    };
};
