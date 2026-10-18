import { findNode, type EdgeEnds, type Graph, type GraphNode, type GraphPort } from "./model";

// The rules of the edges: from an output port to an input port of the same kind, and, unless the designer allows it,
// never in a loop.

/**
 * Why an edge can't be drawn, or null when it can:
 * - `missing`: a node or a port doesn't exist;
 * - `self`: from a node to itself;
 * - `kind`: the ports carry different kinds;
 * - `cycle`: the target already leads to the source, so the graph would loop.
 */
export type ConnectRejection = "missing" | "self" | "kind" | "cycle";

export interface ConnectRules {
    /**
     * Whether an edge may lead back to an earlier node, such as a workflow's loop.
     */
    allowCycles?: boolean;
}

const sameEnds = (a: EdgeEnds, b: EdgeEnds) => a.sourceId === b.sourceId && a.sourcePort === b.sourcePort && a.targetId === b.targetId && a.targetPort === b.targetPort;

export const findOutput = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, nodeId: string, port: string): GraphPort | undefined => {
    const node = findNode(graph, nodeId);

    return node ? graph.model.outputs(node).find((item) => item.name === port) : undefined;
};

export const findInput = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, nodeId: string, port: string): GraphPort | undefined => {
    const node = findNode(graph, nodeId);

    return node ? graph.model.inputs(node).find((item) => item.name === port) : undefined;
};

/**
 * Whether the edge with these ends is in the graph.
 */
export const hasEdge = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, ends: EdgeEnds) =>
    graph.edges.some((edge) => sameEnds(graph.model.ends(edge), ends));

/**
 * Whether `targetId` leads to `sourceId` through the edges, so an edge from source to target would make a loop.
 */
export const createsCycle = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, sourceId: string, targetId: string) => {
    const next = new Map<string, string[]>();

    for (const edge of graph.edges) {
        const ends = graph.model.ends(edge);
        next.set(ends.sourceId, [...(next.get(ends.sourceId) ?? []), ends.targetId]);
    }

    const visited = new Set<string>();
    const stack = [targetId];

    while (stack.length > 0) {
        const id = stack.pop()!;

        if (id === sourceId) {
            return true;
        }

        if (!visited.has(id)) {
            visited.add(id);
            stack.push(...(next.get(id) ?? []));
        }
    }

    return false;
};

export const rejectConnection = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, ends: EdgeEnds, rules: ConnectRules = {}): ConnectRejection | null => {
    if (ends.sourceId === ends.targetId) {
        return "self";
    }

    const output = findOutput(graph, ends.sourceId, ends.sourcePort);
    const input = findInput(graph, ends.targetId, ends.targetPort);

    if (!output || !input) {
        return "missing";
    }

    if (output.kind && input.kind && output.kind !== input.kind) {
        return "kind";
    }

    return !rules.allowCycles && createsCycle(graph, ends.sourceId, ends.targetId) ? "cycle" : null;
};

/**
 * The edges a new edge replaces: the one going into an input that accepts one edge, and the one leaving an output that
 * leads to one node.
 */
export const replacedBy = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, ends: EdgeEnds): TEdge[] => {
    const input = findInput(graph, ends.targetId, ends.targetPort);
    const output = findOutput(graph, ends.sourceId, ends.sourcePort);
    const singleInput = input !== undefined && !input.allowsMany;
    const singleOutput = output !== undefined && !output.allowsMany;

    if (!singleInput && !singleOutput) {
        return [];
    }

    return graph.edges.filter((edge) => {
        const existing = graph.model.ends(edge);

        if (sameEnds(existing, ends)) {
            return false;
        }

        return (
            (singleInput && existing.targetId === ends.targetId && existing.targetPort === ends.targetPort) ||
            (singleOutput && existing.sourceId === ends.sourceId && existing.sourcePort === ends.sourcePort)
        );
    });
};

/**
 * The first input of `node` that can receive `kind`: a free one first, then one whose edge would be replaced. Used
 * when an edge is dropped on a node rather than on one of its ports.
 */
export const firstCompatibleInput = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, node: TNode, kind?: string | null): GraphPort | undefined => {
    const inputs = graph.model.inputs(node).filter((input) => !kind || !input.kind || input.kind === kind);
    const isTaken = (input: GraphPort) =>
        !input.allowsMany &&
        graph.edges.some((edge) => {
            const ends = graph.model.ends(edge);

            return ends.targetId === node.id && ends.targetPort === input.name;
        });

    return inputs.find((input) => !isTaken(input)) ?? inputs[0];
};

/**
 * The edges that use a port the node no longer has, after its settings changed its ports.
 */
export const danglingEdges = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>, node: TNode): TEdge[] => {
    const outputs = new Set(graph.model.outputs(node).map((port) => port.name));
    const inputs = new Set(graph.model.inputs(node).map((port) => port.name));

    return graph.edges.filter((edge) => {
        const ends = graph.model.ends(edge);

        return (ends.sourceId === node.id && !outputs.has(ends.sourcePort)) || (ends.targetId === node.id && !inputs.has(ends.targetPort));
    });
};
