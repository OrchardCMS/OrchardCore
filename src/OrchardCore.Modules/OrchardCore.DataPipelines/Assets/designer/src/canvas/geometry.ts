// Canvas geometry. Every coordinate is in canvas pixels at 100% zoom, the same unit as the step positions. The geometry
// of every graph comes from @bloom/designer; this adds the steps, their ports and the routes of the connections.
import { EDGE_GAP, roundedPath, routeMidpoint, type Point, type Rect } from "@bloom/designer/canvas/geometry";

export {
    EDGE_GAP,
    EDGE_RADIUS,
    GRID_SIZE,
    boundsOf,
    findFreeSpot,
    previewPath,
    rectFromPoints,
    rectsIntersect,
    roundedPath,
    routeMidpoint,
    simplify,
    snap,
    type Point,
    type Rect,
} from "@bloom/designer/canvas/geometry";

export const NODE_WIDTH = 240;
export const DEFAULT_NODE_HEIGHT = 88;

/**
 * The side of a step a port is on: inputs on the left edge, outputs on the right edge.
 */
export type PortSide = "input" | "output";

/**
 * The measured size of a rendered step and the anchor of each of its ports, relative to its top-left corner.
 */
export interface NodeLayout {
    width: number;
    height: number;
    inputs: Record<string, Point>;
    outputs: Record<string, Point>;
}

export const nodeRect = (node: Point, layout?: NodeLayout): Rect => ({
    x: node.x,
    y: node.y,
    width: layout?.width || NODE_WIDTH,
    height: layout?.height || DEFAULT_NODE_HEIGHT,
});

/**
 * The anchor of a port in canvas coordinates. Before the step is measured, the ports are spread along its side.
 */
export const portAnchor = (node: Point, layout: NodeLayout | undefined, side: PortSide, port: string, index = 0, count = 1): Point => {
    const measured = (side === "input" ? layout?.inputs : layout?.outputs)?.[port];

    if (measured) {
        return { x: node.x + measured.x, y: node.y + measured.y };
    }

    const rect = nodeRect(node, layout);

    return { x: side === "input" ? rect.x : rect.x + rect.width, y: rect.y + (rect.height * (index + 1)) / (count + 1) };
};

/**
 * The corners of a connection from an output port (on the right of `source`, facing right) to an input port (on the
 * left of `target`, facing left). The route is made of horizontal and vertical segments:
 * - a target to the right turns halfway between the two ports;
 * - a target behind the source (a backward connection) leaves to the right, passes between the two steps when one is
 *   above the other, or else below both, and comes back to the input from its left.
 */
export const routeConnection = (start: Point, end: Point, source: Rect, target: Rect): Point[] => {
    if (end.x >= start.x + EDGE_GAP * 2) {
        const midX = (start.x + end.x) / 2;

        return [start, { x: midX, y: start.y }, { x: midX, y: end.y }, end];
    }

    const exitX = start.x + EDGE_GAP;
    const entryX = end.x - EDGE_GAP;
    const sourceBottom = source.y + source.height;
    const targetBottom = target.y + target.height;
    let passY: number;

    if (target.y >= sourceBottom + EDGE_GAP) {
        passY = (sourceBottom + target.y) / 2;
    } else if (targetBottom <= source.y - EDGE_GAP) {
        passY = (targetBottom + source.y) / 2;
    } else {
        passY = Math.max(sourceBottom, targetBottom) + EDGE_GAP;
    }

    return [start, { x: exitX, y: start.y }, { x: exitX, y: passY }, { x: entryX, y: passY }, { x: entryX, y: end.y }, end];
};

export interface EdgeGeometry {
    points: Point[];
    path: string;
    mid: Point;
}

/**
 * A connection from an output port at `start` to an input port at `end` (see {@link routeConnection}).
 */
export const edgeGeometry = (start: Point, end: Point, source: Rect, target: Rect): EdgeGeometry => {
    const points = routeConnection(start, end, source, target);

    return { points, path: roundedPath(points), mid: routeMidpoint(points) };
};
