import { afterEach, describe, expect, it } from "vitest";
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
        providedValues: [{ source: "Input", name: "ContentItem", typeName: "contentItem", description: "The content item of the event." }],
    });
    store.loadDefinition({ ...definition, variables: [{ name: "total", typeName: "number" }], variableTypes: [{ name: "number", displayName: "Number", editor: "number" }] });

    return { store, wrapper: mount(AvailableData, { props: { node: store.getNode("a")!, store } }) };
};

describe("AvailableData", () => {
    afterEach(() => {
        localStorage.clear();
    });

    it("render_ActivityAfterAnEvent_ListsItsDataWithTheirExpressions", () => {
        const { wrapper } = setup();

        expect(wrapper.findAll(".wfd-available-group").map((group) => group.text())).toEqual(["VariablesTab", 'AvailableFrom', "AvailableWorkflow"]);

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

    it("toggle_Section_HidesTheListAndRemembersIt", async () => {
        const { wrapper } = setup();

        await wrapper.get("[data-cy=available-data-toggle]").trigger("click");

        expect(wrapper.get("[data-cy=available-data-toggle]").attributes("aria-expanded")).toBe("false");
        expect(setup().wrapper.get("[data-cy=available-data-toggle]").attributes("aria-expanded")).toBe("false");
    });
});
