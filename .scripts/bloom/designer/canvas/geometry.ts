// The geometry the designers share: points, rectangles, the grid, and the rounded paths of edges. Every coordinate is
// in canvas pixels at 100% zoom. Where nodes and their ports sit, and how an edge is routed, belong to each designer.

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

export const snap = (value: number, grid = GRID_SIZE) => Math.round(value / grid) * grid;

// How far an edge runs straight out of its port, and around the nodes it avoids.
export const EDGE_GAP = 24;

// The radius of the rounded corners of an edge.
export const EDGE_RADIUS = 8;

const round = (value: number) => Math.round(value * 10) / 10;

const length = (a: Point, b: Point) => Math.hypot(b.x - a.x, b.y - a.y);

/**
 * The corners of a route without its repeated points, and without the points in the middle of a straight segment.
 */
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
 * A provisional edge from an output port, which faces right, to the pointer while a connection is being drawn. The
 * handles of the curve grow with the distance, within limits.
 */
export const previewPath = (start: Point, pointer: Point) => {
    const handle = Math.min(160, Math.max(40, length(start, pointer) / 2));

    return `M${round(start.x)},${round(start.y)} C${round(start.x + handle)},${round(start.y)} ${round(pointer.x - handle)},${round(pointer.y)} ${round(pointer.x)},${round(pointer.y)}`;
};

/**
 * The first place for a node of `size` at `start` or below it, at least `margin` away from every rect: where a node
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

        // Rounded up to the grid, so the spot doesn't fall back within the margin of the node it moves past.
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
