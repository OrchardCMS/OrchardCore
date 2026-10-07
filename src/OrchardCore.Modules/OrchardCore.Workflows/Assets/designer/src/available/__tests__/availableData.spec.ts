import { describe, expect, it } from "vitest";
import { availableData, expressionsOf, upstreamNodeIds, upstreamValues } from "../availableData";
import { createNode } from "../../canvas/__tests__/fixtures";
import type { DesignerTransition } from "../../api/types";

const labels = { lastResult: "Last result", lastResultDescription: "", correlationId: "Correlation id", correlationIdDescription: "" };

const link = (source: string, destination: string, outcome = "Done"): DesignerTransition => ({
    sourceActivityId: source,
    sourceOutcomeName: outcome,
    destinationActivityId: destination,
});

// published → retrieve → notify, and published → other; loop is on no path to notify.
const nodes = [
    createNode("published", {
        title: "Content Published",
        providedValues: [
            { source: "Input", name: "ContentItem", typeName: "contentItem", description: "The content item of the event." },
            { source: "Input", name: "ContentEvent", typeName: "object" },
        ],
    }),
    createNode("retrieve", {
        title: "Retrieve Owner",
        providedValues: [
            { source: "Properties", name: "Owner", typeName: "any" },
            { source: "Properties", name: "total", typeName: "any" },
        ],
    }),
    createNode("notify", { title: "Notify" }),
    createNode("other", { title: "Other", providedValues: [{ source: "Output", name: "Result", typeName: "any" }] }),
    createNode("loop", { title: "Loop", providedValues: [{ source: "Properties", name: "item", typeName: "any" }] }),
];

const transitions = [link("published", "retrieve"), link("retrieve", "notify"), link("published", "other"), link("loop", "loop", "Next")];

const variables = [{ name: "Total", typeName: "number", description: "The order total." }];

describe("availableData", () => {
    it("upstreamNodeIds_Graph_ListsTheActivitiesOnAPathNearestFirst", () => {
        expect(upstreamNodeIds(transitions, "notify")).toEqual(["retrieve", "published"]);
        expect(upstreamNodeIds(transitions, "published")).toEqual([]);
        expect(upstreamNodeIds(transitions, "loop")).toEqual([]);
    });

    it("availableData_Activity_ListsVariablesUpstreamValuesAndTheWorkflow", () => {
        const data = availableData(nodes, transitions, variables, "notify", labels);

        expect(data.variables.map((value) => [value.name, value.javaScript, value.liquid])).toEqual([
            ["Total", 'variable("Total")', "{{ Workflow.Variables.Total }}"],
        ]);

        // Nearest first; a property with the name of a declared variable is listed with the variables only.
        expect(data.activities.map((group) => [group.title, group.values.map((value) => value.key)])).toEqual([
            ["Retrieve Owner", ["Properties:Owner"]],
            ["Content Published", ["Input:ContentItem", "Input:ContentEvent"]],
        ]);

        expect(data.activities[1].values[0]).toMatchObject({
            javaScript: 'input("ContentItem")',
            liquid: "{{ Workflow.Input.ContentItem }}",
            typeName: "contentItem",
            description: "The content item of the event.",
        });

        expect(data.workflow.map((value) => value.javaScript)).toEqual(["lastResult()", "correlationId()"]);
    });

    it("expressionsOf_Sources_ReadTheRightCollection", () => {
        expect(expressionsOf("Properties", "Owner")).toEqual({ javaScript: 'property("Owner")', liquid: "{{ Workflow.Properties.Owner }}" });
        expect(expressionsOf("Output", "Result")).toEqual({ javaScript: 'workflow().Output["Result"]', liquid: "{{ Workflow.Output.Result }}" });

        // A name that isn't an identifier is read with brackets.
        expect(expressionsOf("Input", "first name")).toEqual({ javaScript: 'input("first name")', liquid: '{{ Workflow.Input["first name"] }}' });
    });

    it("upstreamValues_Activity_ListsEachValueOnceWithItsActivity", () => {
        const values = upstreamValues(nodes, [...transitions, link("published", "notify")], variables, "notify");

        // Both activities lead to it directly: they're in the order of their transitions.
        expect(values.map((value) => [value.key, value.activityTitle])).toEqual([
            ["Properties:Owner", "Retrieve Owner"],
            ["Input:ContentItem", "Content Published"],
            ["Input:ContentEvent", "Content Published"],
        ]);
        expect(upstreamValues(nodes, transitions, variables, null)).toEqual([]);
    });
});
