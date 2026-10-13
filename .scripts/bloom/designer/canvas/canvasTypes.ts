import type { Point } from "./geometry";
import type { MenuItem } from "./menu";

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

/**
 * What GraphCanvas exposes to the designer, through a template ref.
 */
export interface GraphCanvasApi<TLayout extends NodeSize = NodeSize> {
    fit(): void;
    centerOn(id: string): void;
    focusNode(id: string): Promise<void>;
    // Pans so a node is in view above the bottom `inset` pixels of the canvas.
    keepVisible(id: string, inset?: number): void;
    // Opens the context menu at the pointer, or, from the keyboard, at the top end of a node.
    openMenu(items: MenuItem[], label: string, at: MouseEvent | { nodeId: string } | null): void;
    closeMenu(): void;
    focus(): void;
    zoomIn(): void;
    zoomOut(): void;
    resetZoom(): void;
    visibleCenter(): Point;
    clientToCanvas(clientX: number, clientY: number): Point;
    canvasToClient(point: Point): Point;
    layoutOf(id: string): TLayout | undefined;
}
