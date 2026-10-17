import type { Command } from "../core/history";
import type { EdgeEnds, Graph, GraphNode } from "./model";
import { replacedBy } from "./rules";

// The undoable changes of a designer graph. Each one keeps what it needs to put the graph back as it was.

interface Indexed<T> {
    index: number;
    item: T;
}

const indexOfNode = <TNode extends GraphNode>(graph: Graph<TNode, unknown>, id: string) => graph.nodes.findIndex((node) => node.id === id);

const indexOfEdge = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, key: string) => graph.edges.findIndex((edge) => graph.model.key(edge) === key);

const touches = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, edge: TEdge, ids: Set<string>) => {
    const ends = graph.model.ends(edge);

    return ids.has(ends.sourceId) || ids.has(ends.targetId);
};

/**
 * Removes the entries from their array (from the last one, so the indexes stay right) and returns them.
 */
export const removeWhere = <T>(array: T[], predicate: (item: T) => boolean): Indexed<T>[] => {
    const removed = array.map((item, index) => ({ index, item })).filter((entry) => predicate(entry.item));

    for (const entry of [...removed].reverse()) {
        array.splice(entry.index, 1);
    }

    return removed;
};

/**
 * Puts entries removed by {@link removeWhere} back at their indexes.
 */
export const restoreAt = <T>(array: T[], entries: Indexed<T>[]) => {
    for (const entry of entries) {
        array.splice(entry.index, 0, entry.item);
    }
};

/**
 * Adds a node the server created, and the edge the server made to it, if any, as one entry.
 */
export const addNodeCommand = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, node: TNode, edge?: TEdge | null): Command => ({
    label: "add",
    apply() {
        if (indexOfNode(graph, node.id) < 0) {
            graph.nodes.push(node);
        }

        if (edge && indexOfEdge(graph, graph.model.key(edge)) < 0) {
            graph.edges.push({ ...edge });
        }
    },
    revert() {
        const index = indexOfNode(graph, node.id);

        if (index >= 0) {
            graph.nodes.splice(index, 1);
        }

        removeWhere(graph.edges, (item) => touches(graph, item, new Set([node.id])));
    },
});

/**
 * Removes nodes and every edge attached to them. Reverting puts everything back at its index.
 */
export const removeNodesCommand = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, ids: string[]): Command => {
    const removedIds = new Set(ids);
    let nodes: Indexed<TNode>[] = [];
    let edges: Indexed<TEdge>[] = [];

    return {
        label: "remove",
        apply() {
            edges = removeWhere(graph.edges, (item) => touches(graph, item, removedIds));
            nodes = removeWhere(graph.nodes, (node) => removedIds.has(node.id));
        },
        revert() {
            restoreAt(graph.nodes, nodes);
            restoreAt(graph.edges, edges);
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
 * Moves nodes. Moves of the same nodes with the same `coalesceKey` (one drag, or repeated nudges) merge into one entry.
 */
export const moveNodesCommand = <TNode extends GraphNode>(graph: Graph<TNode, unknown>, moves: NodeMove[], coalesceKey?: string): Command => {
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
 * Adds an edge. The edges it replaces (see {@link replacedBy}) are removed, and reverting restores them.
 */
export const connectCommand = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, ends: EdgeEnds): Command & { replaced: () => TEdge[] } => {
    let replaced: Indexed<TEdge>[] = [];
    const edge = graph.model.create(ends);
    const key = graph.model.key(edge);

    return {
        label: "connect",
        replaced: () => replaced.map((entry) => entry.item),
        apply() {
            const keys = new Set(replacedBy(graph, ends).map((item) => graph.model.key(item)));

            replaced = removeWhere(graph.edges, (item) => keys.has(graph.model.key(item)));
            graph.edges.push({ ...edge });
        },
        revert() {
            const index = indexOfEdge(graph, key);

            if (index >= 0) {
                graph.edges.splice(index, 1);
            }

            restoreAt(graph.edges, replaced);
        },
    };
};

/**
 * Removes the edge with the key.
 */
export const disconnectCommand = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, key: string): Command => {
    let removed: Indexed<TEdge>[] = [];

    return {
        label: "disconnect",
        apply() {
            removed = removeWhere(graph.edges, (item) => graph.model.key(item) === key);
        },
        revert() {
            restoreAt(graph.edges, removed);
        },
    };
};

/**
 * Sets a property of a node, such as whether a workflow starts with it.
 */
export const setNodePropertyCommand = <TNode extends GraphNode, TKey extends keyof TNode>(
    graph: Graph<TNode, unknown>,
    id: string,
    property: TKey,
    value: TNode[TKey],
): Command => {
    let previous: TNode[TKey] | undefined;

    return {
        label: `set:${String(property)}`,
        apply() {
            const node = graph.nodes[indexOfNode(graph, id)];

            if (node) {
                previous = node[property];
                node[property] = value;
            }
        },
        revert() {
            const node = graph.nodes[indexOfNode(graph, id)];

            if (node) {
                node[property] = previous as TNode[TKey];
            }
        },
    };
};
