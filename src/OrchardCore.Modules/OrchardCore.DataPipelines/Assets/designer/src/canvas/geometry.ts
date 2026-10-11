// Canvas geometry. Every coordinate is in canvas pixels at 100% zoom, the same unit as the step positions.

export interface Point {
    x: number;
    y: number;
}

export interface Rect {
    x: number;
    y: number;
    width: number;
    height: number;
}

export const GRID_SIZE = 10;
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

export const snap = (value: number, grid = GRID_SIZE) => Math.round(value / grid) * grid;

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

const round = (value: number) => Math.round(value * 10) / 10;

// How far a connection runs straight out of its port, and around the steps it avoids.
export const EDGE_GAP = 24;

// The radius of the rounded corners of a connection.
export const EDGE_RADIUS = 8;

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

// Drops the corners that don't turn: repeated points, and points in the middle of a straight segment.
export const simplify = (points: Point[]) => {
    const distinct = points.filter((point, index) => index === 0 || point.x !== points[index - 1].x || point.y !== points[index - 1].y);

    return distinct.filter((point, index) => {
        if (index === 0 || index === distinct.length - 1) {
            return true;
        }

        const previous = distinct[index - 1];
        const next = distinct[index + 1];

        return !((previous.x === point.x && point.x === next.x) || (previous.y === point.y && point.y === next.y));
    });
};

const length = (a: Point, b: Point) => Math.hypot(b.x - a.x, b.y - a.y);

/**
 * The SVG path of a route, its corners rounded.
 */
export const roundedPath = (points: Point[], radius = EDGE_RADIUS) => {
    const route = simplify(points);
    let path = `M${round(route[0].x)},${round(route[0].y)}`;

    for (let i = 1; i < route.length - 1; i++) {
        const previous = route[i - 1];
        const corner = route[i];
        const next = route[i + 1];
        const r = Math.min(radius, length(previous, corner) / 2, length(corner, next) / 2);
        const before = { x: corner.x - Math.sign(corner.x - previous.x) * r, y: corner.y - Math.sign(corner.y - previous.y) * r };
        const after = { x: corner.x + Math.sign(next.x - corner.x) * r, y: corner.y + Math.sign(next.y - corner.y) * r };

        path += ` L${round(before.x)},${round(before.y)} Q${round(corner.x)},${round(corner.y)} ${round(after.x)},${round(after.y)}`;
    }

    const last = route[route.length - 1];

    return `${path} L${round(last.x)},${round(last.y)}`;
};

/**
 * The point halfway along a route.
 */
export const routeMidpoint = (points: Point[]): Point => {
    const total = points.slice(1).reduce((sum, point, index) => sum + length(points[index], point), 0);
    let remaining = total / 2;

    for (let i = 1; i < points.length; i++) {
        const segment = length(points[i - 1], points[i]);

        if (segment >= remaining && segment > 0) {
            const ratio = remaining / segment;

            return { x: points[i - 1].x + (points[i].x - points[i - 1].x) * ratio, y: points[i - 1].y + (points[i].y - points[i - 1].y) * ratio };
        }

        remaining -= segment;
    }

    return points[points.length - 1];
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

/**
 * A provisional connection from an output port to the pointer while it is being dragged.
 */
export const previewPath = (start: Point, pointer: Point) => {
    const handle = Math.min(160, Math.max(40, Math.hypot(pointer.x - start.x, pointer.y - start.y) / 2));

    return `M${round(start.x)},${round(start.y)} C${round(start.x + handle)},${round(start.y)} ${round(pointer.x - handle)},${round(pointer.y)} ${round(pointer.x)},${round(pointer.y)}`;
};

export const rectsIntersect = (a: Rect, b: Rect) => a.x < b.x + b.width && b.x < a.x + a.width && a.y < b.y + b.height && b.y < a.y + a.height;

/**
 * The first place for a card of `size` at `start` or below it, at least `margin` away from every rect: where a step
 * added after another one goes.
 */
export const findFreeSpot = (start: Point, size: { width: number; height: number }, rects: Rect[], margin = 24): Point => {
    const spot = { x: snap(start.x), y: snap(start.y) };

    for (let i = 0; i < 50; i++) {
        const blocking = rects.find((rect) =>
            rectsIntersect({ x: rect.x - margin, y: rect.y - margin, width: rect.width + 2 * margin, height: rect.height + 2 * margin }, { ...spot, ...size }),
        );

        if (!blocking) {
            break;
        }

        // Rounded up to the grid, so the spot doesn't fall back within the margin of the card it moves past.
        spot.y = Math.ceil((blocking.y + blocking.height + margin) / GRID_SIZE) * GRID_SIZE;
    }

    return spot;
};

/**
 * The rectangle between two corners, in any order (marquee selection).
 */
export const rectFromPoints = (a: Point, b: Point): Rect => ({
    x: Math.min(a.x, b.x),
    y: Math.min(a.y, b.y),
    width: Math.abs(a.x - b.x),
    height: Math.abs(a.y - b.y),
});

export const boundsOf = (rects: Rect[]): Rect | null => {
    if (rects.length === 0) {
        return null;
    }

    const left = Math.min(...rects.map((rect) => rect.x));
    const top = Math.min(...rects.map((rect) => rect.y));
    const right = Math.max(...rects.map((rect) => rect.x + rect.width));
    const bottom = Math.max(...rects.map((rect) => rect.y + rect.height));

    return { x: left, y: top, width: right - left, height: bottom - top };
};
