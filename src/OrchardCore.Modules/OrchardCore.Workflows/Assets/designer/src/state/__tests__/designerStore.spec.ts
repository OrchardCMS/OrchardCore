import { describe, expect, it } from "vitest";
import { createDesignerStore } from "../designerStore";
import { addNodeCommand, canConnect, connectCommand, disconnectCommand, moveNodesCommand, removeNodesCommand, setStartCommand, transitionKey } from "../commands";
import type { DesignerDefinition, DesignerNode } from "../../api/types";

const node = (id: string, x = 0, y = 0, outcomes = ["Done"]): DesignerNode => ({
    id,
    name: "Task",
    x,
    y,
    isStart: false,
    isEvent: false,
    hasEditor: true,
    isMissing: false,
    title: id,
    displayText: id,
    category: "Test",
    designHtml: "",
    outcomes: outcomes.map((name) => ({ name, displayName: name })),
});

const definition = (): DesignerDefinition => ({
    id: 7,
    workflowTypeId: "type-7",
    revision: 3,
    hasDraft: true,
    settings: { name: "Order", isEnabled: true, isSingleton: false, lockTimeout: 0, lockExpiration: 0, deleteFinishedWorkflows: false },
    nodes: [{ ...node("start", 10.4, 20.6), isStart: true, isEvent: true }, node("a", 300, 20), node("b", 300, 200, ["Done", "Failed"])],
    transitions: [
        { sourceActivityId: "start", sourceOutcomeName: "Done", destinationActivityId: "a" },
        { sourceActivityId: "a", sourceOutcomeName: "Done", destinationActivityId: "b" },
    ],
    issues: [],
    runningInstanceCount: 0,
});

describe("designerStore", () => {
    it("toSavePayload_LoadedDefinition_SerializesRoundedPositionsAndTransitions", () => {
        const store = createDesignerStore();
        store.loadDefinition(definition());

        const payload = store.toSavePayload();

        expect(payload.revision).toBe(3);
        expect(payload.nodes).toEqual([
            { id: "start", x: 10, y: 21, isStart: true },
            { id: "a", x: 300, y: 20, isStart: false },
            { id: "b", x: 300, y: 200, isStart: false },
        ]);
        expect(payload.transitions.map(transitionKey)).toEqual(["start:Done:a", "a:Done:b"]);
        expect(payload.removedActivityIds).toEqual([]);
        expect(payload.restoredActivityIds).toEqual([]);
    });

    it("toSavePayload_RemovedActivity_ListsItAndDropsItsTransitions", () => {
        const store = createDesignerStore();
        store.loadDefinition(definition());

        store.execute(removeNodesCommand(store.graph, ["a"]));
        const payload = store.toSavePayload();

        expect(payload.nodes.map((x) => x.id)).toEqual(["start", "b"]);
        expect(payload.transitions).toEqual([]);
        expect(payload.removedActivityIds).toEqual(["a"]);
    });

    it("toSavePayload_RemovalSavedThenUndone_ListsRestoredActivity", () => {
        const store = createDesignerStore();
        store.loadDefinition(definition());
        store.execute(removeNodesCommand(store.graph, ["a"]));
        const removal = store.toSavePayload();
        store.markSaved(removal, { revision: 4, issues: [] });

        store.undo();
        const payload = store.toSavePayload();

        expect(payload.revision).toBe(4);
        expect(payload.restoredActivityIds).toEqual(["a"]);
        expect(payload.removedActivityIds).toEqual([]);
        expect(payload.nodes.map((x) => x.id)).toEqual(["start", "a", "b"]);
        expect(payload.transitions.map(transitionKey)).toEqual(["start:Done:a", "a:Done:b"]);
    });

    it("toSavePayload_RemovalUndoneBeforeSaving_ListsNothing", () => {
        const store = createDesignerStore();
        store.loadDefinition(definition());

        store.execute(removeNodesCommand(store.graph, ["a"]));
        store.undo();
        const payload = store.toSavePayload();

        expect(payload.removedActivityIds).toEqual([]);
        expect(payload.restoredActivityIds).toEqual([]);
    });

    it("registerServerNode_AddedActivity_IsNotRestoredOnSave", () => {
        const store = createDesignerStore();
        store.loadDefinition(definition());
        const added = node("c", 50, 60);

        store.execute(addNodeCommand(store.graph, added));
        store.registerServerNode(added, 4, []);
        const payload = store.toSavePayload();

        expect(payload.revision).toBe(4);
        expect(payload.restoredActivityIds).toEqual([]);
        expect(payload.nodes.at(-1)).toEqual({ id: "c", x: 50, y: 60, isStart: false });
    });

    it("execute_GraphCommands_IncrementChangeVersionAndUndoState", () => {
        const store = createDesignerStore();
        store.loadDefinition(definition());

        store.execute(moveNodesCommand(store.graph, [{ id: "a", fromX: 300, fromY: 20, toX: 310, toY: 30 }]));
        store.execute(setStartCommand(store.graph, "b", true));

        expect(store.state.changeVersion).toBe(2);
        expect(store.state.canUndo).toBe(true);
        expect(store.getNode("b")?.isStart).toBe(true);

        store.undo();
        store.undo();

        expect(store.state.changeVersion).toBe(4);
        expect(store.state.canUndo).toBe(false);
        expect(store.getNode("a")).toMatchObject({ x: 300, y: 20 });
        expect(store.getNode("b")?.isStart).toBe(false);
    });

    it("connectCommand_OutcomeAlreadyConnected_ReplacesTransitionAndUndoRestoresIt", () => {
        const store = createDesignerStore();
        store.loadDefinition(definition());
        const command = connectCommand(store.graph, { sourceActivityId: "start", sourceOutcomeName: "Done", destinationActivityId: "b" });

        store.execute(command);

        expect(command.replaced()).toMatchObject({ destinationActivityId: "a" });
        expect(store.state.transitions.map(transitionKey)).toEqual(["a:Done:b", "start:Done:b"]);

        store.undo();

        expect(store.state.transitions.map(transitionKey)).toEqual(["start:Done:a", "a:Done:b"]);
    });

    it("canConnect_SelfLoopOrUnknownOutcome_ReturnsFalse", () => {
        const store = createDesignerStore();
        store.loadDefinition(definition());

        expect(canConnect(store.graph, { sourceActivityId: "a", sourceOutcomeName: "Done", destinationActivityId: "a" })).toBe(false);
        expect(canConnect(store.graph, { sourceActivityId: "a", sourceOutcomeName: "Nope", destinationActivityId: "b" })).toBe(false);
        expect(canConnect(store.graph, { sourceActivityId: "b", sourceOutcomeName: "Failed", destinationActivityId: "a" })).toBe(true);
    });

    it("disconnectCommand_Undo_RestoresTransitionAtItsIndex", () => {
        const store = createDesignerStore();
        store.loadDefinition(definition());

        store.execute(disconnectCommand(store.graph, "start:Done:a"));
        expect(store.state.transitions.map(transitionKey)).toEqual(["a:Done:b"]);

        store.undo();
        expect(store.state.transitions.map(transitionKey)).toEqual(["start:Done:a", "a:Done:b"]);
    });

    it("replaceNode_EditorApplied_KeepsPositionAndClearsHistory", () => {
        const store = createDesignerStore();
        store.loadDefinition(definition());
        store.execute(moveNodesCommand(store.graph, [{ id: "a", fromX: 300, fromY: 20, toX: 320, toY: 40 }]));

        store.replaceNode({ ...node("a", 0, 0, ["Yes", "No"]), title: "Renamed" });

        expect(store.getNode("a")).toMatchObject({ x: 320, y: 40, title: "Renamed" });
        expect(store.getNode("a")?.outcomes.map((x) => x.name)).toEqual(["Yes", "No"]);
        expect(store.state.canUndo).toBe(false);
    });
});
