import { describe, expect, it } from "vitest";
import { acceptsKind, connectionKey, createsCycle, danglingConnections, firstCompatibleInput, rejectConnection, replacedBy } from "../ports";
import { createDefinition, createNode, files, library, records } from "../../__tests__/fixtures";
import type { DesignerConnection } from "../../api/types";

const connection = (sourceStepId: string, sourcePort: string, targetStepId: string, targetPort: string): DesignerConnection => ({
    sourceStepId,
    sourcePort,
    targetStepId,
    targetPort,
});

describe("ports", () => {
    it("rejectConnection_SameKind_Accepts", () => {
        const { nodes, connections } = createDefinition();

        expect(rejectConnection(nodes, connections, connection("filter", "Unmatched", "join", "Right"))).toBeNull();
        expect(rejectConnection(nodes, connections, connection("csv", "File", "email", "Attachments"))).toBeNull();
    });

    it("rejectConnection_DifferentKinds_RejectsKind", () => {
        const { nodes, connections } = createDefinition();

        // Rows to a files input, and files to a rows input.
        expect(rejectConnection(nodes, connections, connection("filter", "Unmatched", "email", "Attachments"))).toBe("kind");
        expect(rejectConnection(nodes, connections, connection("csv", "File", "table", "Input"))).toBe("kind");
    });

    it("rejectConnection_SelfMissingOrInputAsSource_Rejects", () => {
        const { nodes, connections } = createDefinition();

        expect(rejectConnection(nodes, connections, connection("filter", "Matched", "filter", "Input"))).toBe("self");
        expect(rejectConnection(nodes, connections, connection("filter", "Nope", "join", "Right"))).toBe("missing");
        expect(rejectConnection(nodes, connections, connection("filter", "Matched", "join", "Nope"))).toBe("missing");
        // An input port is never the source of a connection.
        expect(rejectConnection(nodes, connections, connection("join", "Left", "table", "Input"))).toBe("missing");
    });

    it("rejectConnection_BackToAnEarlierStep_RejectsCycle", () => {
        const { nodes, connections } = createDefinition();

        // source → filter → join: join can't feed the filter.
        expect(rejectConnection(nodes, connections, connection("join", "Output", "filter", "Input"))).toBe("cycle");
        expect(createsCycle(connections, "join", "filter")).toBe(true);
        expect(createsCycle(connections, "filter", "join")).toBe(false);
    });

    it("replacedBy_SingleInput_ReturnsTheExistingConnection", () => {
        const { nodes, connections } = createDefinition();

        expect(replacedBy(nodes, connections, connection("filter", "Unmatched", "join", "Left"))).toEqual([connections[1]]);
        expect(replacedBy(nodes, connections, connection("filter", "Unmatched", "join", "Right"))).toEqual([]);
    });

    it("replacedBy_InputThatAllowsMany_KeepsTheOthers", () => {
        const { nodes } = createDefinition();
        const connections = [connection("filter", "Matched", "table", "Input")];

        expect(replacedBy(nodes, connections, connection("filter", "Unmatched", "table", "Input"))).toEqual([]);
    });

    it("firstCompatibleInput_DropOnAStep_PrefersAFreeInputOfTheKind", () => {
        const { nodes, connections } = createDefinition();
        const join = nodes.find((node) => node.id === "join")!;
        const email = nodes.find((node) => node.id === "email")!;

        // Left is taken, so Right.
        expect(firstCompatibleInput(join, "Records", connections)?.name).toBe("Right");
        expect(firstCompatibleInput(join, "Files", connections)).toBeUndefined();
        expect(firstCompatibleInput(email, "Files", connections)?.name).toBe("Attachments");
    });

    it("acceptsKind_FirstInput_DecidesWhatCanComeAfterAnOutput", () => {
        const steps = library.categories.flatMap((category) => category.steps);
        const byName = (name: string) => steps.find((step) => step.name === name)!;

        expect(acceptsKind(byName("FilterStep"), "Records")).toBe(true);
        expect(acceptsKind(byName("FilterStep"), "Files")).toBe(false);
        expect(acceptsKind(byName("ZipFiles"), "Files")).toBe(true);
        // A source has no input.
        expect(acceptsKind(byName("ContentItemsSource"), "Records")).toBe(false);
    });

    it("danglingConnections_PortRemoved_ReturnsItsConnections", () => {
        const { connections } = createDefinition();
        const filter = createNode("filter", { outputs: [records("Matched")], inputs: [files("Input")] });

        // The input still exists (by name), the Matched output too.
        expect(danglingConnections(filter, connections)).toEqual([]);
        expect(danglingConnections(createNode("filter", { outputs: [records("All")] }), connections).map(connectionKey)).toEqual(["filter:Matched->join:Left"]);
    });
});
