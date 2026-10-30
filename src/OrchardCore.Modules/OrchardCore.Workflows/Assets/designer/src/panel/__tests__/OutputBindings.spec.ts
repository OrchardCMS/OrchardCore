import { describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import OutputBindings from "../OutputBindings.vue";
import { createDesignerStore } from "../../state/designerStore";
import type { DesignerApi } from "../../api/designerApi";
import type { DesignerNode } from "../../api/types";
import { createDefinition, createNode } from "../../canvas/__tests__/fixtures";
import { variableTypes, variables } from "../../variables/__tests__/fixtures";

const scriptNode = (bindings: Record<string, string> = {}): DesignerNode =>
    createNode("a", {
        x: 600,
        name: "HttpRequestTask",
        outputs: [
            { name: "Body", typeName: "string", displayName: "Body", description: "The body of the response." },
            { name: "StatusCode", typeName: "number", displayName: "Status code" },
        ],
        outputBindings: bindings,
    });

const setup = (bindings: Record<string, string> = {}, readOnly = false) => {
    const store = createDesignerStore();
    const definition = createDefinition();
    definition.nodes = definition.nodes.map((node) => (node.id === "a" ? scriptNode(bindings) : node));
    store.loadDefinition({ ...definition, revision: 2, variables, variableTypes });
    const saveOutputBindings = vi.fn((activityId: string, revision: number, next: Record<string, string>) =>
        Promise.resolve({ revision: revision + 1, node: scriptNode(next), issues: [{ severity: "Warning" as const, code: "OutputTypeMismatch", message: "m", activityId }] }),
    );
    const api = { saveOutputBindings } as unknown as DesignerApi;
    const wrapper = mount(OutputBindings, { props: { node: store.getNode("a")!, store, api, readOnly } });

    return { store, saveOutputBindings, wrapper };
};

describe("OutputBindings", () => {
    it("render_Outputs_ShowTheirNameTypeAndDescriptionAndOfferTheVariablesToStoreThemIn", () => {
        const { wrapper } = setup({ Body: "greeting" });

        const body = wrapper.get("[data-cy=output-Body]");
        const select = body.get("[data-cy=output-binding]");

        expect(body.get(".wfd-output-name").text()).toBe("Body");
        expect(body.get("[data-cy=output-type]").text()).toBe("Text");
        expect(body.get(".wfd-output-description").text()).toBe("The body of the response.");
        expect(body.get("label").text()).toBe("StoreIn");
        expect((select.element as HTMLSelectElement).value).toBe("greeting");

        // The variables of a type the value converts to come first; the others are grouped, with their type.
        expect(select.findAll(":scope > option").map((option) => option.text())).toEqual(["DontStore", "greeting"]);
        expect(select.get("optgroup").attributes("label")).toBe("OtherVariableTypes");
        expect(select.findAll("optgroup option").map((option) => option.text())).toEqual(["attempts (Number)"]);

        // A number converts to text, so every variable is offered as is.
        const statusCode = wrapper.get("[data-cy=output-StatusCode]");
        expect(statusCode.find(".wfd-output-description").exists()).toBe(false);
        expect(statusCode.findAll("[data-cy=output-binding] > option").map((option) => option.text())).toEqual(["DontStore", "greeting", "attempts"]);
        expect(statusCode.find("optgroup").exists()).toBe(false);
        expect(wrapper.find("[data-cy=output-problem]").exists()).toBe(false);
    });

    it("bind_Variable_SavesTheBindingsAndReplacesTheNode", async () => {
        const { store, saveOutputBindings, wrapper } = setup({ Body: "greeting" });

        await wrapper.get("[data-cy=output-StatusCode] [data-cy=output-binding]").setValue("attempts");
        await flushPromises();

        expect(saveOutputBindings).toHaveBeenCalledWith("a", 2, { Body: "greeting", StatusCode: "attempts" });
        expect(store.state.revision).toBe(3);
        expect(store.getNode("a")!.outputBindings).toEqual({ Body: "greeting", StatusCode: "attempts" });
        expect(store.state.issues.map((issue) => issue.code)).toEqual(["OutputTypeMismatch"]);
    });

    it("unbind_NotStored_RemovesTheBinding", async () => {
        const { saveOutputBindings, wrapper } = setup({ Body: "greeting" });

        await wrapper.get("[data-cy=output-Body] [data-cy=output-binding]").setValue("");
        await flushPromises();

        expect(saveOutputBindings).toHaveBeenCalledWith("a", 2, {});
    });

    it("problems_MismatchedOrUndeclaredVariable_AreFlagged", () => {
        const { wrapper } = setup({ Body: "attempts", StatusCode: "missing" });

        expect(wrapper.get("[data-cy=output-Body] [data-cy=output-problem]").text()).toContain("OutputTypeMismatch");
        expect(wrapper.get("[data-cy=output-StatusCode] [data-cy=output-problem]").text()).toContain("BoundVariableMissing");
        // The undeclared name stays selected.
        expect((wrapper.get("[data-cy=output-StatusCode] [data-cy=output-binding]").element as HTMLSelectElement).value).toBe("missing");
    });

    it("readOnly_Bindings_AreShownAsText", () => {
        const { wrapper } = setup({ Body: "greeting" }, true);

        expect(wrapper.find("select").exists()).toBe(false);
        expect(wrapper.get("[data-cy=output-Body] [data-cy=output-binding]").text()).toBe("greeting");
        expect(wrapper.get("[data-cy=output-StatusCode] [data-cy=output-binding]").text()).toBe("NotBound");
    });

    it("render_ActivityWithoutOutputs_RendersNothing", () => {
        const store = createDesignerStore();
        store.loadDefinition(createDefinition());

        const wrapper = mount(OutputBindings, { props: { node: store.getNode("b")!, store, api: {} as DesignerApi } });

        expect(wrapper.find("[data-cy=activity-outputs]").exists()).toBe(false);
    });
});
