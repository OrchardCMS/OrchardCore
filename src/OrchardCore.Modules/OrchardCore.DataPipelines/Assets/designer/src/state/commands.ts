import type { DesignerConnection, DesignerNode } from "../api/types";
import type { Command } from "./history";
import { connectionKey, replacedBy } from "./ports";

/**
 * The part of the designer state the commands change. The arrays are the store's reactive arrays.
 */
export interface Graph {
    readonly nodes: DesignerNode[];
    readonly connections: DesignerConnection[];
}

const indexOfNode = (graph: Graph, id: string) => graph.nodes.findIndex((node) => node.id === id);

const indexOfConnection = (graph: Graph, key: string) => graph.connections.findIndex((connection) => connectionKey(connection) === key);

interface Indexed<T> {
    index: number;
    item: T;
}

// Removes the entries from their array (from the last one, so the indexes stay right) and returns them.
const removeWhere = <T>(array: T[], predicate: (item: T) => boolean): Indexed<T>[] => {
    const removed = array.map((item, index) => ({ index, item })).filter((entry) => predicate(entry.item));

    for (const entry of [...removed].reverse()) {
        array.splice(entry.index, 1);
    }

    return removed;
};

// Puts entries removed by removeWhere back at their indexes.
const restore = <T>(array: T[], entries: Indexed<T>[]) => {
    for (const entry of entries) {
        array.splice(entry.index, 0, entry.item);
    }
};

/**
 * Adds a step the server created, and the connection the server made to it (AddStepPayload.connectFrom), as one entry.
 */
export const addNodeCommand = (graph: Graph, node: DesignerNode, connection?: DesignerConnection | null): Command => ({
    label: "add",
    apply() {
        if (indexOfNode(graph, node.id) < 0) {
            graph.nodes.push(node);
        }

        if (connection && indexOfConnection(graph, connectionKey(connection)) < 0) {
            graph.connections.push({ ...connection });
        }
    },
    revert() {
        const index = indexOfNode(graph, node.id);

        if (index >= 0) {
            graph.nodes.splice(index, 1);
        }

        removeWhere(graph.connections, (item) => item.sourceStepId === node.id || item.targetStepId === node.id);
    },
});

/**
 * Removes steps and every connection attached to them. Reverting puts everything back at its index.
 */
export const removeNodesCommand = (graph: Graph, ids: string[]): Command => {
    const removedIds = new Set(ids);
    let nodes: Indexed<DesignerNode>[] = [];
    let connections: Indexed<DesignerConnection>[] = [];

    return {
        label: "remove",
        apply() {
            connections = removeWhere(graph.connections, (item) => removedIds.has(item.sourceStepId) || removedIds.has(item.targetStepId));
            nodes = removeWhere(graph.nodes, (node) => removedIds.has(node.id));
        },
        revert() {
            restore(graph.nodes, nodes);
            restore(graph.connections, connections);
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
 * Moves steps. Moves with the same `coalesceKey` (one drag, or repeated nudges) merge into one entry.
 */
export const moveNodesCommand = (graph: Graph, moves: NodeMove[], coalesceKey?: string): Command => {
    const current = moves.map((move) => ({ ...move }));
    const sameNodes = (other: NodeMove[]) => other.length === current.length && other.every((move) => current.some((entry) => entry.id === move.id));

    const place = (pick: (move: NodeMove) => { x: number; y: number }) => {
        for (const move of current) {
            const node = graph.nodes[indexOfNode(graph, move.id)];

            if (node) {
                const { x, y } = pick(move);
                node.x = x;
                node.y = y;
            }
        }
    };

    const command: Command & { moves: NodeMove[] } = {
        label: "move",
        coalesceKey,
        moves: current,
        apply: () => place((move) => ({ x: move.toX, y: move.toY })),
        revert: () => place((move) => ({ x: move.fromX, y: move.fromY })),
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
 * Adds a connection. An input that accepts one connection (not `allowsMany`) keeps the new one only: the existing one
 * is replaced, and reverting restores it.
 */
export const connectCommand = (graph: Graph, connection: DesignerConnection): Command & { replaced: () => DesignerConnection[] } => {
    let replaced: Indexed<DesignerConnection>[] = [];

    return {
        label: "connect",
        replaced: () => replaced.map((entry) => entry.item),
        apply() {
            const keys = new Set(replacedBy(graph.nodes, graph.connections, connection).map(connectionKey));

            replaced = removeWhere(graph.connections, (item) => keys.has(connectionKey(item)));
            graph.connections.push({ ...connection });
        },
        revert() {
            const index = indexOfConnection(graph, connectionKey(connection));

            if (index >= 0) {
                graph.connections.splice(index, 1);
            }

            restore(graph.connections, replaced);
        },
    };
};

export const disconnectCommand = (graph: Graph, key: string): Command => {
    let removed: Indexed<DesignerConnection>[] = [];

    return {
        label: "disconnect",
        apply() {
            removed = removeWhere(graph.connections, (item) => connectionKey(item) === key);
        },
        revert() {
            restore(graph.connections, removed);
        },
    };
};
