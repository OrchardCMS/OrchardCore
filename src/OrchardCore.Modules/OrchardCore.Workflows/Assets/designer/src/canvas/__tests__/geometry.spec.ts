import { describe, expect, it } from "vitest";
import {
    DEFAULT_NODE_HEIGHT,
    NODE_WIDTH,
    bezierControlPoints,
    bezierPoint,
    boundsOf,
    closestSide,
    edgeGeometry,
    findFreeSpot,
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

    // A source card at the origin, its port on its right side.
    const source = { x: 0, y: 0, width: 220, height: 100 };
    const port = { x: 220, y: 50 };

    // Whether a horizontal or vertical segment of a route crosses the inside of a card.
    const crosses = (points: { x: number; y: number }[], rect: typeof source) =>
        points.slice(1).some((point, index) => {
            const previous = points[index];
            const left = Math.min(previous.x, point.x);
            const right = Math.max(previous.x, point.x);
            const top = Math.min(previous.y, point.y);
            const bottom = Math.max(previous.y, point.y);

            return left < rect.x + rect.width && right > rect.x && top < rect.y + rect.height && bottom > rect.y;
        });

    it("edgeGeometry_TargetToTheRight_StepsIntoItsLeftSideLevelWithThePort", () => {
        const geometry = edgeGeometry(port, source, { x: 400, y: 0, width: 220, height: 100 });

        expect(geometry.side).toBe("left");
        expect(geometry.end).toEqual({ x: 400, y: 50 });
        // Level with the port: a straight line.
        expect(geometry.path).toBe("M220,50 L400,50");
    });

    it("edgeGeometry_TargetToTheRightAndLower_TurnsHalfwayWithRoundedCorners", () => {
        const geometry = edgeGeometry(port, source, { x: 400, y: 200, width: 220, height: 100 });

        expect(geometry.points).toEqual([port, { x: 310, y: 50 }, { x: 310, y: 212 }, { x: 400, y: 212 }]);
        expect(geometry.path).toBe("M220,50 L302,50 Q310,50 310,58 L310,204 Q310,212 318,212 L400,212");
    });

    it("edgeGeometry_TargetBelowAndToTheLeft_GoesAroundTheSourceIntoTheTop", () => {
        const target = { x: -200, y: 300, width: 220, height: 100 };
        const geometry = edgeGeometry(port, source, target);

        expect(geometry.side).toBe("top");
        expect(geometry.end).toEqual({ x: -90, y: 300 });
        expect(geometry.points).toEqual([port, { x: 244, y: 50 }, { x: 244, y: 200 }, { x: -90, y: 200 }, { x: -90, y: 300 }]);
        expect(crosses(geometry.points, source)).toBe(false);
    });

    it("edgeGeometry_TargetBelowAndToTheRight_TurnsOnceIntoTheTop", () => {
        const geometry = edgeGeometry(port, source, { x: 200, y: 300, width: 220, height: 100 });

        expect(geometry.points).toEqual([port, { x: 310, y: 50 }, { x: 310, y: 300 }]);
    });

    it("edgeGeometry_TargetAbove_GoesAroundTheSourceIntoTheBottom", () => {
        const target = { x: -100, y: -300, width: 220, height: 100 };
        const geometry = edgeGeometry(port, source, target);

        expect(geometry.side).toBe("bottom");
        expect(geometry.end).toEqual({ x: 10, y: -200 });
        expect(crosses(geometry.points, source)).toBe(false);
    });

    it("edgeGeometry_TargetBehindOnTheSameRow_LoopsAroundBelowBothIntoTheLeftSide", () => {
        const target = { x: -400, y: 20, width: 220, height: 100 };
        const geometry = edgeGeometry(port, source, target);

        expect(geometry.side).toBe("left");
        expect(geometry.end).toEqual({ x: -400, y: 70 });
        expect(crosses(geometry.points, source)).toBe(false);
        expect(crosses(geometry.points, target)).toBe(false);
    });

    it("edgeGeometry_Route_HasItsLabelHalfwayAlongIt", () => {
        const geometry = edgeGeometry(port, source, { x: 420, y: 0, width: 220, height: 100 });

        expect(geometry.mid).toEqual({ x: 320, y: 50 });
    });

    it("findFreeSpot_CardsInTheWay_GoesBelowThem", () => {
        const size = { width: 220, height: 96 };

        expect(findFreeSpot({ x: 403, y: 98 }, size, [])).toEqual({ x: 400, y: 100 });
        // Below a card in the way, and below the next one too.
        expect(findFreeSpot({ x: 400, y: 0 }, size, [{ x: 380, y: 0, width: 220, height: 100 }, { x: 400, y: 120, width: 220, height: 100 }])).toEqual({ x: 400, y: 250 });
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
