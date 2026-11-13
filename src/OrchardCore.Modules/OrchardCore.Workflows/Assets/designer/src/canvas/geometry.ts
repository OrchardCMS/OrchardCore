// Canvas geometry. Every coordinate is in canvas pixels at 100% zoom, the same unit as ActivityRecord.X/Y,
// so existing workflows lay out exactly as before. The geometry of every graph comes from @bloom/designer; this adds
// the activities, their outcome ports and the routes of the transitions.
import { EDGE_GAP, roundedPath, routeMidpoint, type Point, type Rect } from "@bloom/designer/canvas/geometry";

export {
    EDGE_GAP,
    EDGE_RADIUS,
    GRID_SIZE,
    boundsOf,
    findFreeSpot,
    previewPath,
    rectContains,
    rectFromPoints,
    rectsIntersect,
    roundedPath,
    routeMidpoint,
    snap,
    type Point,
    type Rect,
} from "@bloom/designer/canvas/geometry";

export type Side = "left" | "right" | "top" | "bottom";

export const NODE_WIDTH = 220;
export const DEFAULT_NODE_HEIGHT = 96;

/**
 * The measured size of a rendered node and the anchor of each outcome port, relative to the node's
 * top-left corner.
 */
export interface NodeLayout {
    width: number;
    height: number;
    ports: Record<string, Point>;
}

export const nodeRect = (node: Point, layout?: NodeLayout): Rect => ({
    x: node.x,
    y: node.y,
    width: layout?.width || NODE_WIDTH,
    height: layout?.height || DEFAULT_NODE_HEIGHT,
});

/**
 * The anchor of an outcome port in canvas coordinates. Before the node is measured, ports are spread
 * along its right side.
 */
export const portAnchor = (node: Point, layout: NodeLayout | undefined, outcome: string, index = 0, count = 1): Point => {
    const port = layout?.ports[outcome];

    if (port) {
        return { x: node.x + port.x, y: node.y + port.y };
    }

    const rect = nodeRect(node, layout);

    return { x: rect.x + rect.width, y: rect.y + (rect.height * (index + 1)) / (count + 1) };
};

export interface EdgeGeometry {
    start: Point;
    end: Point;
    // The corners of the route, from the port to the target.
    points: Point[];
    path: string;
    mid: Point;
    // The side of the target the edge enters.
    side: Side;
}

// How far from a target's corners an edge entering its side stays.
const SIDE_MARGIN = 12;

const clamp = (value: number, min: number, max: number) => (min > max ? (min + max) / 2 : Math.min(max, Math.max(min, value)));

/**
 * The corners of an edge from an outcome port (which faces right, on the right of `source`) to `target`. The route is
 * made of horizontal and vertical segments, and never runs behind the source:
 * - a target to the right is entered on its left side, level with the port when it faces it;
 * - a target below (or above) is entered on its top (or bottom): the edge goes down (or up) past the source's right side;
 * - a target beside or behind the source (a loop back) is reached around below both, on its left side.
 */
export const routeEdge = (start: Point, source: Rect, target: Rect): { points: Point[]; side: Side } => {
    const exitX = start.x + EDGE_GAP;
    const sourceTop = source.y;
    const sourceBottom = source.y + source.height;
    const targetLeft = target.x;
    const targetTop = target.y;
    const targetBottom = target.y + target.height;
    const targetCenterX = target.x + target.width / 2;
    const targetCenterY = target.y + target.height / 2;

    if (targetLeft >= exitX + EDGE_GAP) {
        const endY = clamp(start.y, targetTop + SIDE_MARGIN, targetBottom - SIDE_MARGIN);
        const midX = (start.x + targetLeft) / 2;

        return { side: "left", points: [start, { x: midX, y: start.y }, { x: midX, y: endY }, { x: targetLeft, y: endY }] };
    }

    const below = targetTop >= sourceBottom + EDGE_GAP;
    const above = targetBottom <= sourceTop - EDGE_GAP;

    if (below || above) {
        const side: Side = below ? "top" : "bottom";
        const endY = below ? targetTop : targetBottom;

        // Further right than the port, the edge turns once; otherwise it turns past the source, between the two.
        if (targetCenterX >= exitX) {
            return { side, points: [start, { x: targetCenterX, y: start.y }, { x: targetCenterX, y: endY }] };
        }

        const midY = below ? (sourceBottom + targetTop) / 2 : (sourceTop + targetBottom) / 2;

        return { side, points: [start, { x: exitX, y: start.y }, { x: exitX, y: midY }, { x: targetCenterX, y: midY }, { x: targetCenterX, y: endY }] };
    }

    const belowY = Math.max(sourceBottom, targetBottom) + EDGE_GAP;
    const entryX = targetLeft - EDGE_GAP;

    return {
        side: "left",
        points: [start, { x: exitX, y: start.y }, { x: exitX, y: belowY }, { x: entryX, y: belowY }, { x: entryX, y: targetCenterY }, { x: targetLeft, y: targetCenterY }],
    };
};

/**
 * An edge from an outcome port of `source` to `target` (see {@link routeEdge}).
 */
export const edgeGeometry = (start: Point, source: Rect, target: Rect): EdgeGeometry => {
    const { points, side } = routeEdge(start, source, target);

    return {
        start,
        end: points[points.length - 1],
        points,
        side,
        path: roundedPath(points),
        mid: routeMidpoint(points),
    };
};
