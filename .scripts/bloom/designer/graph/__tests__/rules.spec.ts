import { describe, expect, it } from "vitest";
import { createsCycle, danglingEdges, firstCompatibleInput, hasEdge, rejectConnection, replacedBy } from "../rules";
import { createPipeline, createWorkflow, ends, step } from "./fixtures";

describe("graph rules", () => {
    it("rejectConnection_SameKind_Accepts", () => {
        const graph = createPipeline();

        expect(rejectConnection(graph, ends("filter", "Unmatched", "join", "Right"))).toBeNull();
        expect(rejectConnection(graph, ends("csv", "File", "email", "Attachments"))).toBeNull();
    });

    it("rejectConnection_DifferentKinds_RejectsKind", () => {
        const graph = createPipeline();

        expect(rejectConnection(graph, ends("filter", "Unmatched", "email", "Attachments"))).toBe("kind");
        expect(rejectConnection(graph, ends("csv", "File", "table", "Input"))).toBe("kind");
    });

    it("rejectConnection_SelfMissingOrInputAsSource_Rejects", () => {
        const graph = createPipeline();

        expect(rejectConnection(graph, ends("filter", "Matched", "filter", "Input"))).toBe("self");
        expect(rejectConnection(graph, ends("filter", "Nope", "join", "Right"))).toBe("missing");
        expect(rejectConnection(graph, ends("filter", "Matched", "join", "Nope"))).toBe("missing");
        // An input port is never the source of an edge.
        expect(rejectConnection(graph, ends("join", "Left", "table", "Input"))).toBe("missing");
    });

    it("rejectConnection_BackToAnEarlierNode_RejectsCycleUnlessAllowed", () => {
        const graph = createPipeline();

        expect(rejectConnection(graph, ends("join", "Output", "filter", "Input"))).toBe("cycle");
        expect(rejectConnection(graph, ends("join", "Output", "filter", "Input"), { allowCycles: true })).toBeNull();
        expect(createsCycle(graph, "join", "filter")).toBe(true);
        expect(createsCycle(graph, "filter", "join")).toBe(false);
    });

    it("rejectConnection_WorkflowLoop_IsAllowedButNotToItself", () => {
        const graph = createWorkflow();

        expect(rejectConnection(graph, ends("notify", "Done", "check", ""), { allowCycles: true })).toBeNull();
        expect(rejectConnection(graph, ends("check", "False", "check", ""), { allowCycles: true })).toBe("self");
        expect(rejectConnection(graph, ends("check", "Maybe", "notify", ""), { allowCycles: true })).toBe("missing");
    });

    it("replacedBy_SingleInput_ReturnsTheExistingEdge", () => {
        const graph = createPipeline();

        expect(replacedBy(graph, ends("filter", "Unmatched", "join", "Left"))).toEqual([graph.edges[1]]);
        expect(replacedBy(graph, ends("filter", "Unmatched", "join", "Right"))).toEqual([]);
    });

    it("replacedBy_InputThatAllowsMany_KeepsTheOthers", () => {
        const graph = createPipeline();
        graph.edges.push({ sourceStepId: "filter", sourcePort: "Matched", targetStepId: "table", targetPort: "Input" });

        expect(replacedBy(graph, ends("filter", "Unmatched", "table", "Input"))).toEqual([]);
    });

    it("replacedBy_OutcomeThatLeadsToOneActivity_ReturnsItsTransition", () => {
        const graph = createWorkflow();

        expect(replacedBy(graph, ends("check", "True", "start", ""))).toEqual([graph.edges[1]]);
        expect(replacedBy(createWorkflow(true), ends("check", "True", "start", ""))).toEqual([]);
    });

    it("hasEdge_ExistingEnds_IsTrue", () => {
        const graph = createWorkflow();

        expect(hasEdge(graph, ends("start", "Done", "check", ""))).toBe(true);
        expect(hasEdge(graph, ends("check", "False", "notify", ""))).toBe(false);
    });

    it("firstCompatibleInput_DropOnANode_PrefersAFreeInputOfTheKind", () => {
        const graph = createPipeline();
        const node = (id: string) => graph.nodes.find((item) => item.id === id)!;

        // Left is taken, so Right.
        expect(firstCompatibleInput(graph, node("join"), "Records")?.name).toBe("Right");
        expect(firstCompatibleInput(graph, node("join"), "Files")).toBeUndefined();
        expect(firstCompatibleInput(graph, node("email"), "Files")?.name).toBe("Attachments");
    });

    it("danglingEdges_PortRemoved_ReturnsItsEdges", () => {
        const graph = createPipeline();
        const records = (name: string) => ({ name, kind: "Records", allowsMany: false });

        expect(danglingEdges(graph, step("filter", [records("Input")], [records("Matched")]))).toEqual([]);
        expect(danglingEdges(graph, step("filter", [records("Input")], [records("All")]))).toEqual([graph.edges[1]]);
    });
});
