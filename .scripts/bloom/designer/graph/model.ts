/**
 * A node of a designer graph, placed on the canvas.
 */
export interface GraphNode {
    id: string;
    x: number;
    y: number;
}

/**
 * The two ends of an edge: from an output port of a node to an input port of another one.
 */
export interface EdgeEnds {
    sourceId: string;
    sourcePort: string;
    targetId: string;
    targetPort: string;
}

/**
 * A port of a node.
 */
export interface GraphPort {
    name: string;
    // What the port carries, such as records or files. Only ports of the same kind connect; ports without a kind
    // connect to any port.
    kind?: string | null;
    // For an input, whether it accepts several edges; for an output, whether it leads to several nodes. A port that
    // doesn't keeps the most recent edge only.
    allowsMany: boolean;
}

/**
 * How a designer's nodes and edges map to the graph: each designer keeps its own types (a workflow's transitions go
 * from an outcome to an activity, a pipeline's connections from a port to a port), and describes them here.
 */
export interface GraphModel<TNode extends GraphNode, TEdge> {
    /**
     * The ends of an edge.
     */
    ends(edge: TEdge): EdgeEnds;
    /**
     * The edge between two ends.
     */
    create(ends: EdgeEnds): TEdge;
    /**
     * The key of an edge, unique in the graph. It is usually the format the server uses, to match its issues.
     */
    key(edge: TEdge): string;
    inputs(node: TNode): GraphPort[];
    outputs(node: TNode): GraphPort[];
}

/**
 * The part of the designer state the commands change. The arrays are the store's reactive arrays; a store that
 * replaces them exposes them through getters.
 */
export interface Graph<TNode extends GraphNode, TEdge> {
    readonly nodes: TNode[];
    readonly edges: TEdge[];
    readonly model: GraphModel<TNode, TEdge>;
}

/**
 * The default key of an edge: "source:port->target:port".
 */
export const endsKey = (ends: EdgeEnds) => `${ends.sourceId}:${ends.sourcePort}->${ends.targetId}:${ends.targetPort}`;

export const findNode = <TNode extends GraphNode>(graph: Pick<Graph<TNode, unknown>, "nodes">, id: string) => graph.nodes.find((node) => node.id === id);
