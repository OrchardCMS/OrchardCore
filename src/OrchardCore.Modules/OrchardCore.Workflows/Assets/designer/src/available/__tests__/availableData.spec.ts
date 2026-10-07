import { describe, expect, it } from "vitest";
import { availableData, expressionsOf, memberExpressionsOf, upstreamNodeIds, upstreamValues } from "../availableData";
import { createNode } from "../../canvas/__tests__/fixtures";
import type { DesignerTransition } from "../../api/types";

const labels = {
    lastResult: "Last result",
    lastResultDescription: "What the activity before returned.",
    lastResultFrom: (activity: string, description: string) => `${activity}: ${description}`,
    correlationId: "Correlation id",
    correlationIdDescription: "",
    inputDescription: "Passed in.",
};

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
            {
                source: "Input",
                name: "ContentEvent",
                typeName: "object",
                members: [
                    { name: "ContentType", typeName: "string", description: "The content type." },
                    { name: "ContentItemId", typeName: "string" },
                ],
            },
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

        // The fields of a value, with the expressions that read them.
        expect(data.activities[1].values[0].members).toEqual([]);
        expect(data.activities[1].values[1].members).toEqual([
            {
                key: "Input:ContentEvent.ContentType",
                name: "ContentType",
                source: "Input",
                typeName: "string",
                description: "The content type.",
                javaScript: 'input("ContentEvent").ContentType',
                liquid: "{{ Workflow.Input.ContentEvent.ContentType }}",
            },
            {
                key: "Input:ContentEvent.ContentItemId",
                name: "ContentItemId",
                source: "Input",
                typeName: "string",
                description: undefined,
                javaScript: 'input("ContentEvent").ContentItemId',
                liquid: "{{ Workflow.Input.ContentEvent.ContentItemId }}",
            },
        ]);

        expect(data.workflow.map((value) => value.javaScript)).toEqual(["lastResult()", "correlationId()"]);
    });

    it("availableData_ActivitiesBeforeDeclareTheLastResult_GiveItsTypeAndFields", () => {
        const retrieve = (id: string, typeName: string, members: { name: string; typeName: string }[] = []) =>
            createNode(id, { title: `Retrieve ${id}`, providedValues: [{ source: "LastResult", name: "LastResult", typeName, description: "The content item.", members }] });
        const field = [{ name: "ContentType", typeName: "string" }];
        const lastResult = (sources: ReturnType<typeof createNode>[], target = createNode("target")) =>
            availableData([...sources, target], sources.map((source) => link(source.id, "target")), [], "target", labels).workflow[0];

        // One activity leads to it: its last result, with its fields; the last result isn't listed with its values.
        const one = lastResult([retrieve("a", "contentItem", field)]);
        expect(one).toMatchObject({ typeName: "contentItem", description: "Retrieve a: The content item.", javaScript: "lastResult()" });
        expect(one.members).toMatchObject([{ name: "ContentType", javaScript: "lastResult().ContentType", liquid: "{{ Workflow.LastResult.ContentType }}" }]);
        expect(availableData([retrieve("a", "contentItem"), createNode("target")], [link("a", "target")], [], "target", labels).activities).toEqual([]);

        // Several agree on the type; they don't on the fields, or on the type.
        expect(lastResult([retrieve("a", "contentItem", field), retrieve("b", "contentItem")])).toMatchObject({ typeName: "contentItem", members: [] });
        expect(lastResult([retrieve("a", "contentItem"), retrieve("b", "string")])).toMatchObject({
            typeName: "any",
            description: "Retrieve a: The content item. Retrieve b: The content item.",
        });

        // An activity that doesn't declare it, or none: any value.
        expect(lastResult([retrieve("a", "contentItem"), createNode("c")])).toMatchObject({ typeName: "any", description: "What the activity before returned." });
        expect(lastResult([])).toMatchObject({ typeName: "any", description: "What the activity before returned." });
    });

    it("availableData_ActivitySetsValuesBeforeItsExpressions_ListsThemForItself", () => {
        const register = createNode("register", {
            title: "Register",
            providedValues: [
                { source: "Properties", name: "EmailConfirmationUrl", typeName: "string", availableToItself: true },
                { source: "Properties", name: "UserName", typeName: "string" },
            ],
        });

        expect(availableData([register], [], [], "register", labels).self.map((value) => [value.key, value.liquid])).toEqual([
            ["Properties:EmailConfirmationUrl", "{{ Workflow.Properties.EmailConfirmationUrl }}"],
        ]);
        expect(upstreamValues([register], [], [], "register").map((value) => [value.key, value.activityTitle])).toEqual([["Properties:EmailConfirmationUrl", "Register"]]);

        // The activities after it see them all.
        expect(availableData([register, createNode("next")], [link("register", "next")], [], "next", labels).self).toEqual([]);
    });

    it("availableData_InputVariables_AreListedAsTheWorkflowsInputs", () => {
        const data = availableData([createNode("a")], [], [{ name: "name", typeName: "string", isInput: true }, { name: "greeting", typeName: "string", isOutput: true }, ...variables], "a", labels);

        expect(data.inputs).toEqual([
            { key: "Input:name", name: "name", source: "Input", typeName: "string", description: "Passed in.", javaScript: 'input("name")', liquid: "{{ Workflow.Input.name }}" },
        ]);
    });

    it("availableData_GlobalValuesAndFunctions_AreListedWithTheirExpressions", () => {
        const data = availableData([createNode("a")], [], [], "a", labels, [
            {
                kind: "Value",
                name: "Site",
                typeName: "object",
                liquidPath: "Site",
                members: [{ name: "SiteName", typeName: "string" }],
            },
            { kind: "Value", name: "User", typeName: "object", liquidPath: "User", members: [{ name: "Identity.Name", typeName: "string" }] },
            { kind: "Function", name: "uuid()", typeName: "string", javaScript: "uuid()", description: "A new id." },
        ]);

        // A Liquid value has no JavaScript, and its fields can be paths.
        expect(data.global.map((value) => [value.key, value.javaScript, value.liquid, value.members?.map((member) => member.liquid)])).toEqual([
            ["Value:Site", null, "{{ Site }}", ["{{ Site.SiteName }}"]],
            ["Value:User", null, "{{ User }}", ["{{ User.Identity.Name }}"]],
        ]);

        // A function has no Liquid.
        expect(data.functions).toMatchObject([{ key: "Function:uuid()", name: "uuid()", javaScript: "uuid()", liquid: null, typeName: "string", description: "A new id." }]);
        expect(availableData([createNode("a")], [], [], "a", labels).global).toEqual([]);
    });

    it("memberExpressionsOf_Field_ReadsItFromTheValue", () => {
        const owner = expressionsOf("Input", "Owner");

        expect(memberExpressionsOf(owner, "Email")).toEqual({ javaScript: 'input("Owner").Email', liquid: "{{ Workflow.Input.Owner.Email }}" });
        expect(memberExpressionsOf(expressionsOf("Output", "first name"), "e-mail")).toEqual({
            javaScript: 'workflow().Output["first name"]["e-mail"]',
            liquid: '{{ Workflow.Output["first name"]["e-mail"] }}',
        });
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
            // The fields are completed too.
            ["Input:ContentEvent.ContentType", "Content Published"],
            ["Input:ContentEvent.ContentItemId", "Content Published"],
        ]);
        expect(upstreamValues(nodes, transitions, variables, null)).toEqual([]);
    });
});
