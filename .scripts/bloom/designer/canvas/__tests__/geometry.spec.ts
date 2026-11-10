import { describe, expect, it } from "vitest";
import { boundsOf, findFreeSpot, previewPath, rectContains, rectFromPoints, rectsIntersect, roundedPath, routeMidpoint, simplify, snap } from "../geometry";

describe("geometry", () => {
    it("snap_Values_RoundToGrid", () => {
        expect(snap(14)).toBe(10);
        expect(snap(15)).toBe(20);
        expect(snap(-4)).toBe(-0);
        expect(snap(23, 5)).toBe(25);
    });

    it("simplify_StraightAndRepeatedCorners_DropsThem", () => {
        expect(
            simplify([
                { x: 0, y: 0 },
                { x: 0, y: 0 },
                { x: 10, y: 0 },
                { x: 20, y: 0 },
                { x: 20, y: 30 },
            ]),
        ).toEqual([
            { x: 0, y: 0 },
            { x: 20, y: 0 },
            { x: 20, y: 30 },
        ]);
    });

    it("roundedPath_Corner_RoundsItWithinHalfTheSegments", () => {
        expect(
            roundedPath([
                { x: 0, y: 0 },
                { x: 100, y: 0 },
                { x: 100, y: 10 },
            ]),
        ).toBe("M0,0 L95,0 Q100,0 100,5 L100,10");
    });

    it("routeMidpoint_Route_IsHalfwayAlongIt", () => {
        expect(
            routeMidpoint([
                { x: 0, y: 0 },
                { x: 100, y: 0 },
                { x: 100, y: 100 },
            ]),
        ).toEqual({ x: 100, y: 0 });
    });

    it("previewPath_Pointer_CurvesOutOfThePortTowardsIt", () => {
        expect(previewPath({ x: 0, y: 0 }, { x: 200, y: 0 })).toBe("M0,0 C100,0 100,0 200,0");
        // The handles stay within 40 and 160 pixels.
        expect(previewPath({ x: 0, y: 0 }, { x: 10, y: 0 })).toBe("M0,0 C40,0 -30,0 10,0");
    });

    it("findFreeSpot_CardsInTheWay_GoesBelowThem", () => {
        const size = { width: 220, height: 96 };

        expect(findFreeSpot({ x: 403, y: 98 }, size, [])).toEqual({ x: 400, y: 100 });
        // Below a card in the way, and below the next one too.
        expect(
            findFreeSpot({ x: 400, y: 0 }, size, [
                { x: 380, y: 0, width: 220, height: 100 },
                { x: 400, y: 120, width: 220, height: 100 },
            ]),
        ).toEqual({ x: 400, y: 250 });
    });

    it("hitTesting_PointsAndRects_DetectsContainmentAndOverlap", () => {
        const rect = { x: 10, y: 10, width: 100, height: 50 };

        expect(rectContains(rect, { x: 10, y: 10 })).toBe(true);
        expect(rectContains(rect, { x: 111, y: 30 })).toBe(false);
        expect(rectsIntersect(rect, { x: 100, y: 50, width: 10, height: 10 })).toBe(true);
        expect(rectsIntersect(rect, { x: 120, y: 10, width: 10, height: 10 })).toBe(false);
        expect(rectFromPoints({ x: 50, y: 80 }, { x: 10, y: 20 })).toEqual({ x: 10, y: 20, width: 40, height: 60 });
    });

    it("boundsOf_Rects_ReturnsEnclosingRect", () => {
        expect(boundsOf([])).toBeNull();
        expect(
            boundsOf([
                { x: 10, y: 20, width: 100, height: 50 },
                { x: -30, y: 100, width: 20, height: 20 },
            ]),
        ).toEqual({ x: -30, y: 20, width: 140, height: 100 });
    });
});
