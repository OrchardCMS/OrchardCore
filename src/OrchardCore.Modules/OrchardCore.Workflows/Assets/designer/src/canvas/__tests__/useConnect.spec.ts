import { describe, expect, it } from "vitest";
import { createDesignerStore } from "../../state/designerStore";
import { transitionKey } from "../../state/commands";
import { connectOutcome, deleteSelection, nudgeSelection, selectAll, selectNode, selectTransition, toggleStart } from "../useConnect";
import { createDefinition } from "./fixtures";

const setup = () => {
    const store = createDesignerStore();
    store.loadDefinition(createDefinition());

    return store;
};

describe("canvas actions", () => {
    it("connectOutcome_OutcomeAlreadyConnected_ReplacesTheEdge", () => {
        const store = setup();

        const result = connectOutcome(store, { sourceActivityId: "fork", sourceOutcomeName: "A", destinationActivityId: "b" });

        expect(result).toEqual({ status: "replaced", replaced: { sourceActivityId: "fork", sourceOutcomeName: "A", destinationActivityId: "a" } });
        expect(store.state.transitions.filter((transition) => transition.sourceActivityId === "fork" && transition.sourceOutcomeName === "A")).toHaveLength(1);
        expect(store.state.transitions.map(transitionKey)).toContain("fork:A:b");
    });

    it("connectOutcome_FollowingEveryTransition_AddsAnotherEdge", () => {
        const store = setup();
        store.state.settings = { ...store.state.settings!, branchingMode: "All" };

        expect(connectOutcome(store, { sourceActivityId: "fork", sourceOutcomeName: "A", destinationActivityId: "b" })).toEqual({ status: "connected" });
        expect(store.state.transitions.filter((transition) => transition.sourceActivityId === "fork" && transition.sourceOutcomeName === "A")).toHaveLength(2);

        // Undoing removes only the new edge.
        store.undo();
        expect(store.state.transitions.map(transitionKey)).not.toContain("fork:A:b");
        expect(store.state.transitions.map(transitionKey)).toContain("fork:A:a");
    });

    it("connectOutcome_NewOutcome_AddsEdge", () => {
        const store = setup();

        expect(connectOutcome(store, { sourceActivityId: "a", sourceOutcomeName: "Done", destinationActivityId: "b" })).toEqual({ status: "connected" });
        expect(store.state.transitions).toHaveLength(4);
    });

    it("connectOutcome_SelfLoop_IsRejected", () => {
        const store = setup();

        expect(connectOutcome(store, { sourceActivityId: "a", sourceOutcomeName: "Done", destinationActivityId: "a" })).toEqual({ status: "rejected" });
        expect(store.state.transitions).toHaveLength(3);
        expect(store.state.canUndo).toBe(false);
    });

    it("connectOutcome_SameEdgeAgain_IsUnchanged", () => {
        const store = setup();

        expect(connectOutcome(store, { sourceActivityId: "start", sourceOutcomeName: "Done", destinationActivityId: "fork" })).toEqual({ status: "unchanged" });
        expect(store.state.canUndo).toBe(false);
    });

    it("deleteSelection_SelectedActivity_RemovesItsEdgesAndUndoRestoresThem", () => {
        const store = setup();
        selectNode(store, "fork");

        const result = deleteSelection(store);

        expect(result).toEqual({ nodes: 1, transitions: 3 });
        expect(store.state.nodes.map((node) => node.id)).toEqual(["start", "a", "b"]);
        expect(store.state.transitions).toEqual([]);
        expect(store.state.selectedNodeIds).toEqual([]);

        store.undo();

        expect(store.state.nodes.map((node) => node.id)).toEqual(["start", "fork", "a", "b"]);
        expect(store.state.transitions.map(transitionKey)).toEqual(["start:Done:fork", "fork:A:a", "fork:B:b"]);
    });

    it("deleteSelection_SelectedEdge_RemovesOnlyTheEdge", () => {
        const store = setup();
        selectTransition(store, "fork:B:b");

        expect(deleteSelection(store)).toEqual({ nodes: 0, transitions: 1 });
        expect(store.state.nodes).toHaveLength(4);
        expect(store.state.transitions.map(transitionKey)).toEqual(["start:Done:fork", "fork:A:a"]);
    });

    it("nudgeSelection_RepeatedArrows_MergeIntoOneUndoEntry", () => {
        const store = setup();
        selectNode(store, "a");
        selectNode(store, "b", true);

        nudgeSelection(store, 10, 0);
        nudgeSelection(store, 10, 0);
        nudgeSelection(store, 0, -10);

        expect(store.getNode("a")).toMatchObject({ x: 620, y: -10 });
        expect(store.getNode("b")).toMatchObject({ x: 620, y: 190 });
        expect(store.history.size).toBe(1);

        store.undo();

        expect(store.getNode("a")).toMatchObject({ x: 600, y: 0 });
        expect(store.getNode("b")).toMatchObject({ x: 600, y: 200 });
    });

    it("selectNode_ShiftClick_TogglesMembership", () => {
        const store = setup();

        selectNode(store, "a");
        selectNode(store, "b", true);
        selectNode(store, "a", true);

        expect(store.state.selectedNodeIds).toEqual(["b"]);

        selectAll(store);
        expect(store.state.selectedNodeIds).toEqual(["start", "fork", "a", "b"]);
    });

    it("toggleStart_EventAndTask_OnlyTogglesEvents", () => {
        const store = setup();

        toggleStart(store, "start");
        toggleStart(store, "a");

        expect(store.getNode("start")?.isStart).toBe(false);
        expect(store.getNode("a")?.isStart).toBe(false);
        expect(store.history.size).toBe(1);
    });
});
