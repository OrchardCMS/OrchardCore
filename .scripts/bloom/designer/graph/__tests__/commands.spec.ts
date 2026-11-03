import { describe, expect, it } from "vitest";
import { addNodeCommand, connectCommand, disconnectCommand, moveNodesCommand, removeNodesCommand, setNodePropertyCommand } from "../commands";
import { createGraphHistory, createServerNodeIds } from "../graphHistory";
import { createPipeline, createWorkflow, ends, step } from "./fixtures";
import type { Graph, GraphNode } from "../model";

const keys = <TNode extends GraphNode, TEdge>(graph: Graph<TNode, TEdge>) => graph.edges.map((edge) => graph.model.key(edge));

const setup = () => {
    const state = { changeVersion: 0, canUndo: false, canRedo: false };

    return { state, history: createGraphHistory(state) };
};

describe("graph commands", () => {
    it("connectCommand_FreeInput_ConnectsAndUndoRemovesIt", () => {
        const graph = createPipeline();
        const { state, history } = setup();

        history.execute(connectCommand(graph, ends("filter", "Unmatched", "join", "Right")));
        expect(keys(graph)).toContain("filter:Unmatched->join:Right");
        expect(state).toMatchObject({ changeVersion: 1, canUndo: true });

        history.undo();
        expect(keys(graph)).not.toContain("filter:Unmatched->join:Right");

        history.redo();
        expect(keys(graph)).toContain("filter:Unmatched->join:Right");
        expect(state.changeVersion).toBe(3);
    });

    it("connectCommand_TakenSingleInput_ReplacesTheEdgeAndUndoRestoresIt", () => {
        const graph = createPipeline();
        const { history } = setup();
        const before = keys(graph);

        const command = connectCommand(graph, ends("filter", "Unmatched", "join", "Left"));
        history.execute(command);

        expect(command.replaced().map((edge) => graph.model.key(edge))).toEqual(["filter:Matched->join:Left"]);
        expect(keys(graph)).toEqual(["source:Output->filter:Input", "filter:Unmatched->join:Left"]);

        history.undo();
        expect(keys(graph)).toEqual(before);
    });

    it("connectCommand_OutcomeWithATransition_ReplacesItUnlessEveryTransitionIsFollowed", () => {
        const graph = createWorkflow();
        const { history } = setup();

        history.execute(connectCommand(graph, ends("check", "True", "start", "")));
        expect(keys(graph)).toEqual(["start:Done:check", "check:True:start"]);

        const all = createWorkflow(true);
        connectCommand(all, ends("check", "True", "start", "")).apply();
        expect(keys(all)).toEqual(["start:Done:check", "check:True:notify", "check:True:start"]);
    });

    it("disconnectCommand_Key_RemovesTheEdgeAndUndoPutsItBack", () => {
        const graph = createWorkflow();
        const { history } = setup();

        history.execute(disconnectCommand(graph, "start:Done:check"));
        expect(keys(graph)).toEqual(["check:True:notify"]);

        history.undo();
        expect(keys(graph)).toEqual(["start:Done:check", "check:True:notify"]);
    });

    it("removeNodesCommand_Nodes_RemovesTheirEdgesAndRevertRestoresOrder", () => {
        const graph = createPipeline();
        const { history } = setup();
        const order = graph.nodes.map((node) => node.id);
        const edges = keys(graph);

        history.execute(removeNodesCommand(graph, ["filter", "csv"]));
        expect(graph.nodes.map((node) => node.id)).toEqual(["source", "join", "table", "email"]);
        expect(graph.edges).toHaveLength(0);

        history.undo();
        expect(graph.nodes.map((node) => node.id)).toEqual(order);
        expect(keys(graph)).toEqual(edges);
    });

    it("addNodeCommand_WithServerEdge_AddsBothAsOneEntry", () => {
        const graph = createPipeline();
        const { history } = setup();

        history.execute(addNodeCommand(graph, step("sort", [{ name: "Input", kind: "Records", allowsMany: false }], [], 600, 300), {
            sourceStepId: "filter",
            sourcePort: "Unmatched",
            targetStepId: "sort",
            targetPort: "Input",
        }));
        expect(graph.nodes.map((node) => node.id)).toContain("sort");
        expect(keys(graph)).toContain("filter:Unmatched->sort:Input");

        history.undo();
        expect(graph.nodes.map((node) => node.id)).not.toContain("sort");
        expect(keys(graph)).not.toContain("filter:Unmatched->sort:Input");
    });

    it("moveNodesCommand_Repeated_MakesOneUndoEntry", () => {
        const graph = createPipeline();
        const { state, history } = setup();
        const join = () => graph.nodes.find((node) => node.id === "join")!;

        for (const [dx, dy] of [
            [10, 0],
            [10, 0],
            [0, 10],
        ]) {
            history.execute(moveNodesCommand(graph, [{ id: "join", fromX: join().x, fromY: join().y, toX: join().x + dx, toY: join().y + dy }], "nudge"));
        }

        expect(join()).toMatchObject({ x: 620, y: 10 });

        history.undo();
        expect(join()).toMatchObject({ x: 600, y: 0 });
        expect(state.canUndo).toBe(false);
    });

    it("setNodePropertyCommand_Property_SetsItAndUndoRestoresIt", () => {
        const graph = createWorkflow();
        const { history } = setup();

        history.execute(setNodePropertyCommand(graph, "check", "isStart", true));
        expect(graph.nodes[1].isStart).toBe(true);

        history.undo();
        expect(graph.nodes[1].isStart).toBe(false);
    });

    it("serverNodeIds_RemovedAndRestored_ComparesWithTheGraph", () => {
        const ids = createServerNodeIds();

        ids.reset(["a", "b"]);
        ids.add("c");

        expect(ids.removed(["a", "c", "d"])).toEqual(["b"]);
        expect(ids.restored(["a", "c", "d"])).toEqual(["d"]);
    });
});
