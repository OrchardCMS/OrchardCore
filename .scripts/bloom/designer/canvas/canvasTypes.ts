import type { Point } from "./geometry";

/**
 * The measured size of a node. A designer's layout may say more, such as where its ports are.
 */
export interface NodeSize {
    width: number;
    height: number;
}

/**
 * An edge the canvas shows, with its two nodes.
 */
export interface CanvasEdge<TNode, TEdge> {
    key: string;
    edge: TEdge;
    source: TNode;
    target: TNode;
}

/**
 * The connection being drawn from an output: the node and input under the pointer, and whether it can go there.
 */
export interface Connecting {
    sourceId: string;
    port: string;
    pointer: Point;
    targetId: string | null;
    targetPort: string | null;
    valid: boolean;
}

/**
 * Where a connection dropped on a node goes: the input port (null when the node has none for it), and whether the
 * connection can go there.
 */
export interface ConnectTarget {
    port: string | null;
    valid: boolean;
}
