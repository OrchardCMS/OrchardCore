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
    // The corners of the route, from the port to the target.
    points: Point[];
    path: string;
    mid: Point;
    // The side of the target the edge enters.
    side: Side;
}

// How far an edge runs straight out of its port, and around the cards it avoids.
export const EDGE_GAP = 24;

// The radius of the rounded corners of an edge.
export const EDGE_RADIUS = 8;

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

// Drops the corners that don't turn: repeated points, and points in the middle of a straight segment.
const simplify = (points: Point[]) => {
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
        const before = { x: corner.x - (Math.sign(corner.x - previous.x) * r), y: corner.y - (Math.sign(corner.y - previous.y) * r) };
        const after = { x: corner.x + (Math.sign(next.x - corner.x) * r), y: corner.y + (Math.sign(next.y - corner.y) * r) };

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

/**
 * A provisional edge from a port to the pointer while a connection is being dragged.
 */
export const previewPath = (start: Point, pointer: Point) => {
    const [c1, c2] = bezierControlPoints(start, { x: 1, y: 0 }, pointer, { x: -1, y: 0 });

    return `M${round(start.x)},${round(start.y)} C${round(c1.x)},${round(c1.y)} ${round(c2.x)},${round(c2.y)} ${round(pointer.x)},${round(pointer.y)}`;
};

/**
 * The first place for a card of `size` at `start` or below it, at least `margin` away from every rect: where an activity
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
