import { describe, expect, it } from "vitest";
import type { DesignerTransition } from "../../api/types";
import { collapsedHiding, hiddenActivityIds, hiddenAfter } from "../branches";

// "a>b" connects the Done outcome of a to b; an activity with two connections uses one outcome each.
const graph = (...edges: string[]): DesignerTransition[] =>
    edges.map((edge, index) => {
        const [source, destination] = edge.split(">");

        return { sourceActivityId: source, sourceOutcomeName: `O${index}`, destinationActivityId: destination };
    });

const sorted = (ids: Set<string>) => [...ids].sort();

describe("branches", () => {
    it("hiddenActivityIds_Chain_HidesEverythingAfterTheCollapsedActivity", () => {
        const transitions = graph("s>a", "a>b", "b>c");

        expect(sorted(hiddenActivityIds(["s", "a", "b", "c"], transitions, ["a"], ["s"]))).toEqual(["b", "c"]);
    });

    it("hiddenActivityIds_NothingCollapsed_HidesNothing", () => {
        const transitions = graph("s>a");

        expect(hiddenActivityIds(["s", "a"], transitions, [], ["s"]).size).toBe(0);
        expect(hiddenActivityIds(["s", "a"], transitions, ["deleted"], ["s"]).size).toBe(0);
    });

    it("hiddenActivityIds_JoinReachedAnotherWay_StaysVisible", () => {
        const transitions = graph("s>fork", "fork>a", "fork>b", "a>join", "b>join", "join>end");
        const ids = ["s", "fork", "a", "b", "join", "end"];

        expect(sorted(hiddenActivityIds(ids, transitions, ["a"], ["s"]))).toEqual([]);
        expect(sorted(hiddenActivityIds(ids, transitions, ["fork"], ["s"]))).toEqual(["a", "b", "end", "join"]);
        expect(sorted(hiddenActivityIds(ids, transitions, ["a", "b"], ["s"]))).toEqual(["end", "join"]);
    });

    it("hiddenActivityIds_LoopBackToStart_KeepsTheStartVisible", () => {
        const transitions = graph("s>a", "a>b", "b>s");

        expect(sorted(hiddenActivityIds(["s", "a", "b"], transitions, ["a"], ["s"]))).toEqual(["b"]);
    });

    it("hiddenActivityIds_LoopToAnEarlierActivity_HidesOnlyWhatComesAfter", () => {
        const transitions = graph("s>a", "a>b", "b>c", "c>a");

        expect(sorted(hiddenActivityIds(["s", "a", "b", "c"], transitions, ["b"], ["s"]))).toEqual(["c"]);
    });

    it("hiddenActivityIds_NestedCollapsed_HidesTheInnerOneToo", () => {
        const transitions = graph("s>a", "a>b", "b>c");
        const hidden = hiddenActivityIds(["s", "a", "b", "c"], transitions, ["a", "b"], ["s"]);

        expect(sorted(hidden)).toEqual(["b", "c"]);
        expect(sorted(hiddenAfter("a", transitions, hidden))).toEqual(["b", "c"]);
    });

    it("hiddenActivityIds_CollapsedLoopWithNoWayIn_ShowsTheFirstOne", () => {
        const transitions = graph("x>y", "y>x");

        expect(sorted(hiddenActivityIds(["x", "y"], transitions, ["x", "y"]))).toEqual(["y"]);
    });

    it("hiddenAfter_CollapsedActivity_CountsOnlyItsHiddenActivities", () => {
        const transitions = graph("s>fork", "fork>a", "fork>b", "a>a2");
        const hidden = hiddenActivityIds(["s", "fork", "a", "a2", "b"], transitions, ["a"], ["s"]);

        expect(sorted(hiddenAfter("a", transitions, hidden))).toEqual(["a2"]);
        expect(hiddenAfter("fork", transitions, hidden).size).toBe(0);
    });

    it("collapsedHiding_NestedTarget_ExpandsEveryCollapsedActivityOnTheWay", () => {
        const transitions = graph("s>a", "a>b", "b>c", "s>d", "d>e");
        const ids = ["s", "a", "b", "c", "d", "e"];

        expect(collapsedHiding(["c"], ids, transitions, ["a", "b", "d"], ["s"])).toEqual(["a", "b"]);
        expect(collapsedHiding(["a"], ids, transitions, ["a", "b", "d"], ["s"])).toEqual([]);
    });
});
