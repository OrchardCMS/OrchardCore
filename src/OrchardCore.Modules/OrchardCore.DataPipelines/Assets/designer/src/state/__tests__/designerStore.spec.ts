import { describe, expect, it } from "vitest";
import { createDesignerStore } from "../designerStore";
import { moveNodesCommand } from "../commands";
import { connectionKey } from "../ports";
import { deleteSelection, selectNode } from "../../canvas/useConnect";
import { createDefinition, createNode, records } from "../../__tests__/fixtures";

const setup = () => {
    const store = createDesignerStore();
    store.loadDefinition({ ...createDefinition(), revision: 4 });

    return store;
};

describe("designerStore", () => {
    it("toSavePayload_Graph_SendsPositionsConnectionsAndRemovedSteps", () => {
        const store = setup();

        store.execute(moveNodesCommand(store.graph, [{ id: "join", fromX: 600, fromY: 0, toX: 610.4, toY: 20.6 }]));
        selectNode(store, "csv");
        deleteSelection(store);

        const payload = store.toSavePayload();

        expect(payload.revision).toBe(4);
        expect(payload.nodes.find((node) => node.id === "join")).toEqual({ id: "join", x: 610, y: 21 });
        expect(payload.connections).toHaveLength(2);
        expect(payload.removedStepIds).toEqual(["csv"]);
        expect(payload.restoredStepIds).toEqual([]);
    });

    it("toSavePayload_DeletionUndoneAfterSave_ReportsTheStepAsRestored", () => {
        const store = setup();

        selectNode(store, "csv");
        deleteSelection(store);
        const payload = store.toSavePayload();
        store.markSaved(payload, { revision: 5, issues: [] });

        store.undo();

        expect(store.toSavePayload()).toMatchObject({ revision: 5, removedStepIds: [], restoredStepIds: ["csv"] });
    });

    it("replaceNode_PortRemoved_DropsItsConnectionsKeepsThePositionAndClearsHistory", () => {
        const store = setup();
        store.execute(moveNodesCommand(store.graph, [{ id: "filter", fromX: 300, fromY: 0, toX: 320, toY: 40 }]));
        const version = store.state.changeVersion;

        // The filter no longer has its Matched output; the server reports the connection it removed.
        const removed = store.replaceNode(createNode("filter", { displayText: "Filter (edited)", outputs: [records("Unmatched")] }), [
            { sourceStepId: "filter", sourcePort: "Matched", targetStepId: "join", targetPort: "Left" },
        ]);

        expect(removed).toBe(1);
        expect(store.getNode("filter")).toMatchObject({ displayText: "Filter (edited)", x: 320, y: 40 });
        expect(store.state.connections.map(connectionKey)).toEqual(["source:Output->filter:Input"]);
        expect(store.state.changeVersion).toBe(version + 1);
        expect(store.state.canUndo).toBe(false);
    });

    it("markPublished_Draft_StartsOverFromRevisionZero", () => {
        const store = setup();
        store.applyServerChange(6, []);

        store.markPublished([{ severity: "Warning", message: "Slow" }], { versionId: "v2", number: 2, publishedUtc: "2026-10-01T00:00:00Z", isPublished: true });

        expect(store.state).toMatchObject({ revision: 0, hasDraft: false, publishedVersion: { number: 2 } });
        expect(store.state.issues).toHaveLength(1);
    });
});
