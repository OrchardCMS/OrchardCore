import { describe, expect, it } from "vitest";
import { createDesignerStore } from "../designerStore";
import { addNodeCommand, removeNodesCommand } from "../commands";
import { connectionKey } from "../ports";
import { clearSelection, connectPorts, deleteSelection, nudgeSelection, selectConnection, selectNode } from "../../canvas/useConnect";
import { createDefinition, createNode } from "../../__tests__/fixtures";

const setup = () => {
    const store = createDesignerStore();
    store.loadDefinition(createDefinition());

    return store;
};

const keys = (store: ReturnType<typeof setup>) => store.state.connections.map(connectionKey);

describe("graph commands", () => {
    it("connectPorts_FreeInput_ConnectsAndUndoRemovesIt", () => {
        const store = setup();

        expect(connectPorts(store, { sourceStepId: "filter", sourcePort: "Unmatched", targetStepId: "join", targetPort: "Right" })).toEqual({ status: "connected" });
        expect(keys(store)).toContain("filter:Unmatched->join:Right");

        store.undo();
        expect(keys(store)).not.toContain("filter:Unmatched->join:Right");

        store.redo();
        expect(keys(store)).toContain("filter:Unmatched->join:Right");
    });

    it("connectPorts_TakenSingleInput_ReplacesTheConnectionAndUndoRestoresIt", () => {
        const store = setup();
        const before = keys(store);

        const result = connectPorts(store, { sourceStepId: "filter", sourcePort: "Unmatched", targetStepId: "join", targetPort: "Left" });

        expect(result.status).toBe("replaced");
        expect(result.status === "replaced" && result.replaced.map(connectionKey)).toEqual(["filter:Matched->join:Left"]);
        expect(keys(store)).toEqual(["source:Output->filter:Input", "filter:Unmatched->join:Left"]);

        store.undo();
        expect(keys(store)).toEqual(before);
    });

    it("connectPorts_InputThatAllowsMany_KeepsEveryConnection", () => {
        const store = setup();

        connectPorts(store, { sourceStepId: "filter", sourcePort: "Matched", targetStepId: "table", targetPort: "Input" });
        connectPorts(store, { sourceStepId: "filter", sourcePort: "Unmatched", targetStepId: "table", targetPort: "Input" });

        expect(keys(store).filter((key) => key.endsWith("table:Input"))).toHaveLength(2);
    });

    it("connectPorts_IncompatibleOrExisting_IsRejectedOrUnchangedWithoutHistory", () => {
        const store = setup();

        expect(connectPorts(store, { sourceStepId: "filter", sourcePort: "Matched", targetStepId: "email", targetPort: "Attachments" })).toEqual({
            status: "rejected",
            reason: "kind",
        });
        expect(connectPorts(store, { sourceStepId: "source", sourcePort: "Output", targetStepId: "filter", targetPort: "Input" })).toEqual({ status: "unchanged" });
        expect(store.state.canUndo).toBe(false);
    });

    it("deleteSelection_Steps_RemovesThemWithTheirConnectionsAndUndoPutsThemBack", () => {
        const store = setup();
        const before = { nodes: store.state.nodes.map((node) => node.id), connections: keys(store) };

        selectNode(store, "filter");
        expect(deleteSelection(store)).toEqual({ nodes: 1, connections: 2 });
        expect(store.state.nodes.map((node) => node.id)).not.toContain("filter");
        expect(store.state.connections).toHaveLength(0);

        store.undo();
        expect(store.state.nodes.map((node) => node.id)).toEqual(before.nodes);
        expect(keys(store)).toEqual(before.connections);
    });

    it("deleteSelection_Connection_RemovesOnlyIt", () => {
        const store = setup();

        selectConnection(store, "filter:Matched->join:Left");
        expect(deleteSelection(store)).toEqual({ nodes: 0, connections: 1 });
        expect(keys(store)).toEqual(["source:Output->filter:Input"]);

        clearSelection(store);
        expect(deleteSelection(store)).toEqual({ nodes: 0, connections: 0 });
    });

    it("nudgeSelection_Repeated_MakesOneUndoEntry", () => {
        const store = setup();

        selectNode(store, "join");
        nudgeSelection(store, 10, 0);
        nudgeSelection(store, 10, 0);
        nudgeSelection(store, 0, 10);
        expect(store.getNode("join")).toMatchObject({ x: 620, y: 10 });

        store.undo();
        expect(store.getNode("join")).toMatchObject({ x: 600, y: 0 });
        expect(store.state.canUndo).toBe(false);
    });

    it("addNodeCommand_WithServerConnection_AddsBothAsOneEntry", () => {
        const store = setup();
        const node = createNode("sort", { x: 600, y: 300 });

        store.execute(addNodeCommand(store.graph, node, { sourceStepId: "filter", sourcePort: "Unmatched", targetStepId: "sort", targetPort: "Input" }));
        expect(store.getNode("sort")).toBeDefined();
        expect(keys(store)).toContain("filter:Unmatched->sort:Input");

        store.undo();
        expect(store.getNode("sort")).toBeUndefined();
        expect(keys(store)).not.toContain("filter:Unmatched->sort:Input");
    });

    it("removeNodesCommand_Revert_RestoresOrder", () => {
        const store = setup();
        const order = store.state.nodes.map((node) => node.id);

        store.execute(removeNodesCommand(store.graph, ["filter", "csv"]));
        store.undo();

        expect(store.state.nodes.map((node) => node.id)).toEqual(order);
    });
});
