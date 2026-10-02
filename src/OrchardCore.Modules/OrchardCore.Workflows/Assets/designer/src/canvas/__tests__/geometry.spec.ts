import { describe, expect, it } from "vitest";
import {
    DEFAULT_NODE_HEIGHT,
    NODE_WIDTH,
    bezierControlPoints,
    bezierPoint,
    boundsOf,
    closestSide,
    edgeGeometry,
    nodeRect,
    portAnchor,
    rectContains,
    rectFromPoints,
    rectsIntersect,
    snap,
} from "../geometry";

describe("geometry", () => {
    it("snap_Values_RoundToGrid", () => {
        expect(snap(14)).toBe(10);
        expect(snap(15)).toBe(20);
        expect(snap(-4)).toBe(-0);
        expect(snap(23, 5)).toBe(25);
    });

    it("portAnchor_MeasuredLayout_UsesMeasuredOffset", () => {
        const layout = { width: 220, height: 120, ports: { Done: { x: 220, y: 100 } } };

        expect(portAnchor({ x: 50, y: 60 }, layout, "Done")).toEqual({ x: 270, y: 160 });
    });

    it("portAnchor_NotMeasured_SpreadsPortsOnRightSide", () => {
        const node = { x: 0, y: 0 };

        expect(portAnchor(node, undefined, "A", 0, 2)).toEqual({ x: NODE_WIDTH, y: DEFAULT_NODE_HEIGHT / 3 });
        expect(portAnchor(node, undefined, "B", 1, 2)).toEqual({ x: NODE_WIDTH, y: (DEFAULT_NODE_HEIGHT * 2) / 3 });
    });

    it("closestSide_PointAroundRect_PicksFacingSide", () => {
        const rect = { x: 100, y: 100, width: 200, height: 100 };

        expect(closestSide(rect, { x: 0, y: 150 })).toEqual({ side: "left", point: { x: 100, y: 150 } });
        expect(closestSide(rect, { x: 400, y: 160 })).toEqual({ side: "right", point: { x: 300, y: 150 } });
        expect(closestSide(rect, { x: 200, y: 0 })).toEqual({ side: "top", point: { x: 200, y: 100 } });
        expect(closestSide(rect, { x: 210, y: 400 })).toEqual({ side: "bottom", point: { x: 200, y: 200 } });
    });

    it("bezierControlPoints_Distance_ClampsHandleLength", () => {
        const [near1, near2] = bezierControlPoints({ x: 0, y: 0 }, { x: 1, y: 0 }, { x: 20, y: 0 }, { x: -1, y: 0 });
        const [far1] = bezierControlPoints({ x: 0, y: 0 }, { x: 1, y: 0 }, { x: 1000, y: 0 }, { x: -1, y: 0 });
        const [mid1] = bezierControlPoints({ x: 0, y: 0 }, { x: 1, y: 0 }, { x: 200, y: 0 }, { x: -1, y: 0 });

        expect(near1).toEqual({ x: 40, y: 0 });
        expect(near2).toEqual({ x: -20, y: 0 });
        expect(far1).toEqual({ x: 160, y: 0 });
        expect(mid1).toEqual({ x: 100, y: 0 });
    });

    it("bezierPoint_Ends_ReturnsEndpoints", () => {
        const p0 = { x: 0, y: 0 };
        const p3 = { x: 100, y: 50 };

        expect(bezierPoint(0, p0, { x: 10, y: 0 }, { x: 90, y: 50 }, p3)).toEqual(p0);
        expect(bezierPoint(1, p0, { x: 10, y: 0 }, { x: 90, y: 50 }, p3)).toEqual(p3);
    });

    it("edgeGeometry_TargetToTheRight_EntersLeftSideWithArrowDirection", () => {
        const geometry = edgeGeometry({ x: 220, y: 50 }, { x: 400, y: 0, width: 220, height: 100 });

        expect(geometry.side).toBe("left");
        expect(geometry.end).toEqual({ x: 400, y: 50 });
        expect(geometry.c1.x).toBeGreaterThan(220);
        expect(geometry.c2.x).toBeLessThan(400);
        expect(geometry.path).toBe("M220,50 C310,50 310,50 400,50");
    });

    it("edgeGeometry_TargetBelow_EntersTopSide", () => {
        const geometry = edgeGeometry({ x: 220, y: 50 }, { x: 0, y: 400, width: 220, height: 100 });

        expect(geometry.side).toBe("top");
        expect(geometry.end).toEqual({ x: 110, y: 400 });
        expect(geometry.c2.y).toBeLessThan(400);
    });

    it("hitTesting_PointsAndRects_DetectsContainmentAndOverlap", () => {
        const rect = nodeRect({ x: 10, y: 10 }, { width: 100, height: 50, ports: {} });

        expect(rectContains(rect, { x: 10, y: 10 })).toBe(true);
        expect(rectContains(rect, { x: 111, y: 30 })).toBe(false);
        expect(rectsIntersect(rect, { x: 100, y: 50, width: 10, height: 10 })).toBe(true);
        expect(rectsIntersect(rect, { x: 120, y: 10, width: 10, height: 10 })).toBe(false);
        expect(rectFromPoints({ x: 50, y: 80 }, { x: 10, y: 20 })).toEqual({ x: 10, y: 20, width: 40, height: 60 });
    });

    it("boundsOf_Rects_ReturnsEnclosingRect", () => {
        expect(boundsOf([])).toBeNull();
        expect(boundsOf([{ x: 10, y: 20, width: 100, height: 50 }, { x: -30, y: 100, width: 20, height: 20 }])).toEqual({ x: -30, y: 20, width: 140, height: 100 });
    });
});
