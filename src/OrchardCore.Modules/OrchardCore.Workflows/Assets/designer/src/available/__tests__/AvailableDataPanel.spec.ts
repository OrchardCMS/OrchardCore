import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import AvailableData from "../AvailableData.vue";
import { createDesignerStore } from "../../state/designerStore";
import { createDefinition, createNode } from "../../canvas/__tests__/fixtures";

const setup = () => {
    const store = createDesignerStore();
    const definition = createDefinition();
    definition.nodes[0] = createNode("start", {
        isEvent: true,
        isStart: true,
        title: "Content Published",
        providedValues: [
            {
                source: "Input",
                name: "ContentItem",
                typeName: "contentItem",
                description: "The content item of the event.",
                members: [{ name: "ContentType", typeName: "string", description: "The content type." }],
            },
        ],
    });
    store.loadDefinition({
        ...definition,
        variables: [{ name: "total", typeName: "number" }],
        variableTypes: [{ name: "number", displayName: "Number", editor: "number" }],
        globalValues: [
            { kind: "Value", name: "Site", typeName: "object", liquidPath: "Site", members: [{ name: "SiteName", typeName: "string" }] },
            { kind: "Function", name: "uuid()", typeName: "string", javaScript: "uuid()" },
        ],
    });

    return { store, wrapper: mount(AvailableData, { props: { node: store.getNode("a")!, store } }) };
};

describe("AvailableData", () => {
    it("render_ActivityAfterAnEvent_ListsItsDataWithTheirExpressions", () => {
        const { wrapper } = setup();

        expect(wrapper.findAll(".wfd-available-group").map((group) => group.text())).toEqual(["VariablesTab", "AvailableFrom", "AvailableWorkflow", "AvailableGlobal", "AvailableFunctions"]);

        const contentItem = wrapper.get("[data-cy='available-start-Input:ContentItem']");
        expect(contentItem.text()).toContain("ContentItem");
        expect(contentItem.text()).toContain("The content item of the event.");
        expect(contentItem.get("[data-cy=available-javascript]").text()).toContain('input("ContentItem")');
        expect(contentItem.get("[data-cy=available-liquid]").text()).toContain("{{ Workflow.Input.ContentItem }}");

        expect(wrapper.get("[data-cy='available-variables-Variable:total']").text()).toContain("Number");
    });

    it("click_Expression_AsksToInsertIt", async () => {
        const { wrapper } = setup();

        await wrapper.get("[data-cy='available-start-Input:ContentItem'] [data-cy=available-liquid]").trigger("click");

        expect(wrapper.emitted("insert")).toEqual([["{{ Workflow.Input.ContentItem }}"]]);
    });

    it("render_ValueWithFields_ListsTheFieldsWithTheirExpressions", async () => {
        const { wrapper } = setup();

        const fields = wrapper.get("[data-cy='available-start-Input:ContentItem'] [data-cy=available-members]");
        expect(fields.get("summary").text()).toBe("AvailableFields");

        const contentType = fields.get("[data-cy='available-start-Input:ContentItem.ContentType']");
        expect(contentType.text()).toContain("The content type.");
        expect(contentType.get("[data-cy=available-javascript]").text()).toContain('input("ContentItem").ContentType');

        await contentType.get("[data-cy=available-liquid]").trigger("click");

        expect(wrapper.emitted("insert")).toEqual([["{{ Workflow.Input.ContentItem.ContentType }}"]]);
    });

    it("render_GlobalValuesAndFunctions_ShowTheExpressionsTheyHave", () => {
        const { wrapper } = setup();

        const site = wrapper.get("[data-cy='available-global-Value:Site']");
        expect(site.find("[data-cy=available-javascript]").exists()).toBe(false);
        expect(site.get("[data-cy=available-liquid]").text()).toContain("{{ Site }}");
        expect(site.get("[data-cy='available-global-Value:Site.SiteName'] [data-cy=available-liquid]").text()).toContain("{{ Site.SiteName }}");

        const uuid = wrapper.get("[data-cy='available-functions-Function:uuid()']");
        expect(uuid.get("[data-cy=available-javascript]").text()).toContain("uuid()");
        expect(uuid.find("[data-cy=available-liquid]").exists()).toBe(false);
        expect(wrapper.get("[data-cy=available-filters] a").attributes("href")).toContain("/Liquid/");
    });

    it("render_ValueWithoutFields_HasNoFieldsList", () => {
        const { wrapper } = setup();

        expect(wrapper.find("[data-cy='available-variables-Variable:total'] [data-cy=available-members]").exists()).toBe(false);
    });
});
