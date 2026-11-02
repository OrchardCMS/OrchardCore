import { describe, expect, it } from "vitest";
import { EDGE_GAP, edgeGeometry, findFreeSpot, portAnchor, roundedPath, routeConnection, routeMidpoint, simplify, snap, type NodeLayout, type Point } from "../geometry";

const rect = (x: number, y: number, width = 240, height = 80) => ({ x, y, width, height });

// Every segment of a route is horizontal or vertical.
const isOrthogonal = (points: Point[]) => points.slice(1).every((point, index) => point.x === points[index].x || point.y === points[index].y);

describe("geometry", () => {
    it("portAnchor_Measured_UsesTheMeasuredDot", () => {
        const layout: NodeLayout = { width: 240, height: 100, inputs: { Left: { x: -1, y: 40 } }, outputs: { Output: { x: 241, y: 50 } } };

        expect(portAnchor({ x: 100, y: 200 }, layout, "input", "Left")).toEqual({ x: 99, y: 240 });
        expect(portAnchor({ x: 100, y: 200 }, layout, "output", "Output")).toEqual({ x: 341, y: 250 });
    });

    it("portAnchor_NotMeasured_SpreadsPortsAlongTheirSide", () => {
        // Two inputs on the left edge of an unmeasured step (240 x 88): at a third and two thirds of its height.
        expect(portAnchor({ x: 0, y: 0 }, undefined, "input", "Left", 0, 2)).toEqual({ x: 0, y: 88 / 3 });
        expect(portAnchor({ x: 0, y: 0 }, undefined, "input", "Right", 1, 2)).toEqual({ x: 0, y: (88 * 2) / 3 });
        expect(portAnchor({ x: 0, y: 0 }, undefined, "output", "Output")).toEqual({ x: 240, y: 44 });
    });

    it("routeConnection_TargetToTheRight_TurnsHalfwayBetweenThePorts", () => {
        const points = routeConnection({ x: 240, y: 40 }, { x: 400, y: 140 }, rect(0, 0), rect(400, 100));

        expect(points).toEqual([
            { x: 240, y: 40 },
            { x: 320, y: 40 },
            { x: 320, y: 140 },
            { x: 400, y: 140 },
        ]);
    });

    it("routeConnection_TargetBehindAndBelow_PassesBetweenTheSteps", () => {
        const start = { x: 540, y: 40 };
        const end = { x: 0, y: 340 };
        const points = routeConnection(start, end, rect(300, 0), rect(0, 300));

        expect(isOrthogonal(points)).toBe(true);
        // Out to the right, between the steps (y 80 to 300), and into the input from its left.
        expect(points[1]).toEqual({ x: start.x + EDGE_GAP, y: 40 });
        expect(points[2].y).toBe(190);
        expect(points[4]).toEqual({ x: -EDGE_GAP, y: 340 });
        expect(points[points.length - 1]).toEqual(end);
    });

    it("routeConnection_TargetBehindAtTheSameHeight_GoesAroundBelowBoth", () => {
        const points = routeConnection({ x: 540, y: 40 }, { x: 0, y: 60 }, rect(300, 0), rect(0, 20, 240, 100));

        expect(isOrthogonal(points)).toBe(true);
        // Below the lower of the two steps (bottom 120).
        expect(points[2].y).toBe(120 + EDGE_GAP);
    });

    it("roundedPath_Corners_AreRoundedAndStraightPointsDropped", () => {
        const points = [
            { x: 0, y: 0 },
            { x: 50, y: 0 },
            { x: 100, y: 0 },
            { x: 100, y: 100 },
        ];

        expect(simplify(points)).toEqual([points[0], points[2], points[3]]);
        expect(roundedPath(points)).toBe("M0,0 L92,0 Q100,0 100,8 L100,100");
    });

    it("routeMidpoint_Route_IsHalfwayAlongIt", () => {
        expect(
            routeMidpoint([
                { x: 0, y: 0 },
                { x: 100, y: 0 },
                { x: 100, y: 100 },
            ]),
        ).toEqual({ x: 100, y: 0 });
        expect(edgeGeometry({ x: 240, y: 40 }, { x: 400, y: 40 }, rect(0, 0), rect(400, 0)).path).toBe("M240,40 L400,40");
    });

    it("findFreeSpot_CardsInTheWay_GoesBelowThem", () => {
        expect(findFreeSpot({ x: 403, y: 0 }, { width: 240, height: 88 }, [rect(400, 0, 240, 80)])).toEqual({ x: 400, y: 110 });
        expect(snap(14)).toBe(10);
    });
});
