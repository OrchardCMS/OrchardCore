// Canvas geometry. Every coordinate is in canvas pixels at 100% zoom, the same unit as ActivityRecord.X/Y,
// so existing workflows lay out exactly as before.

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

export type Side = "left" | "right" | "top" | "bottom";

export const GRID_SIZE = 10;
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

export const snap = (value: number, grid = GRID_SIZE) => Math.round(value / grid) * grid;

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

/**
 * The side of `rect` facing `from`, and the middle point of that side, where an incoming edge ends.
 */
export const closestSide = (rect: Rect, from: Point): { side: Side; point: Point } => {
    const centerX = rect.x + rect.width / 2;
    const centerY = rect.y + rect.height / 2;
    const dx = (from.x - centerX) / (rect.width / 2 || 1);
    const dy = (from.y - centerY) / (rect.height / 2 || 1);

    if (Math.abs(dx) >= Math.abs(dy)) {
        return dx < 0
            ? { side: "left", point: { x: rect.x, y: centerY } }
            : { side: "right", point: { x: rect.x + rect.width, y: centerY } };
    }

    return dy < 0
        ? { side: "top", point: { x: centerX, y: rect.y } }
        : { side: "bottom", point: { x: centerX, y: rect.y + rect.height } };
};

export const sideDirection = (side: Side): Point => {
    switch (side) {
        case "left":
            return { x: -1, y: 0 };
        case "right":
            return { x: 1, y: 0 };
        case "top":
            return { x: 0, y: -1 };
        default:
            return { x: 0, y: 1 };
    }
};

/**
 * The two control points of a cubic Bézier leaving `start` along `startDirection` and entering `end`
 * against `endDirection`. The handle length grows with the distance, within limits.
 */
export const bezierControlPoints = (start: Point, startDirection: Point, end: Point, endDirection: Point): [Point, Point] => {
    const distance = Math.hypot(end.x - start.x, end.y - start.y);
    const handle = Math.min(160, Math.max(40, distance / 2));

    return [
        { x: start.x + startDirection.x * handle, y: start.y + startDirection.y * handle },
        { x: end.x + endDirection.x * handle, y: end.y + endDirection.y * handle },
    ];
};

export const bezierPoint = (t: number, p0: Point, p1: Point, p2: Point, p3: Point): Point => {
    const u = 1 - t;

    return {
        x: u * u * u * p0.x + 3 * u * u * t * p1.x + 3 * u * t * t * p2.x + t * t * t * p3.x,
        y: u * u * u * p0.y + 3 * u * u * t * p1.y + 3 * u * t * t * p2.y + t * t * t * p3.y,
    };
};

const round = (value: number) => Math.round(value * 10) / 10;

export interface EdgeGeometry {
    start: Point;
    end: Point;
    c1: Point;
    c2: Point;
    path: string;
    mid: Point;
    side: Side;
}

/**
 * An edge from an outcome port (which always faces right) to the closest side of the target node.
 */
export const edgeGeometry = (start: Point, target: Rect): EdgeGeometry => {
    const { side, point: end } = closestSide(target, start);
    const [c1, c2] = bezierControlPoints(start, { x: 1, y: 0 }, end, sideDirection(side));

    return {
        start,
        end,
        c1,
        c2,
        side,
        path: `M${round(start.x)},${round(start.y)} C${round(c1.x)},${round(c1.y)} ${round(c2.x)},${round(c2.y)} ${round(end.x)},${round(end.y)}`,
        mid: bezierPoint(0.5, start, c1, c2, end),
    };
};

/**
 * A provisional edge from a port to the pointer while a connection is being dragged.
 */
export const previewPath = (start: Point, pointer: Point) => {
    const [c1, c2] = bezierControlPoints(start, { x: 1, y: 0 }, pointer, { x: -1, y: 0 });

    return `M${round(start.x)},${round(start.y)} C${round(c1.x)},${round(c1.y)} ${round(c2.x)},${round(c2.y)} ${round(pointer.x)},${round(pointer.y)}`;
};

export const rectContains = (rect: Rect, point: Point) =>
    point.x >= rect.x && point.x <= rect.x + rect.width && point.y >= rect.y && point.y <= rect.y + rect.height;

export const rectsIntersect = (a: Rect, b: Rect) =>
    a.x < b.x + b.width && b.x < a.x + a.width && a.y < b.y + b.height && b.y < a.y + a.height;

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
