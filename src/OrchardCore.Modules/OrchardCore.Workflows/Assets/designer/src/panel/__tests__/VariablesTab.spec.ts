import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import VariablesTab from "../VariablesTab.vue";
import { createDesignerStore } from "../../state/designerStore";
import { DesignerApiError, type DesignerApi } from "../../api/designerApi";
import type { VariableDefinition, VariablesResult } from "../../api/types";
import { createDefinition } from "../../canvas/__tests__/fixtures";
import { variableTypes, variables } from "../../variables/__tests__/fixtures";

const setup = (options: { readOnly?: boolean; saveVariables?: (revision: number, variables: VariableDefinition[]) => Promise<VariablesResult>; values?: Record<string, unknown> } = {}) => {
    const store = createDesignerStore();
    store.loadDefinition({
        ...createDefinition(),
        revision: 3,
        variables: structuredClone(variables),
        variableTypes,
        instance: options.values ? { id: 9, workflowId: "w", status: "Halted", blockingActivityIds: [], variableValues: options.values } : null,
    });
    const saveVariables = vi.fn(
        options.saveVariables ?? ((revision: number, sent: VariableDefinition[]) => Promise.resolve({ revision: revision + 1, variables: sent, issues: [] })),
    );
    const api = { saveVariables } as unknown as DesignerApi;
    const wrapper = mount(VariablesTab, { props: { store, api, readOnly: options.readOnly ?? false }, attachTo: document.body });

    return { store, saveVariables, wrapper };
};

const rowsOf = (wrapper: ReturnType<typeof setup>["wrapper"]) => wrapper.findAll("[data-cy=variable]");

describe("VariablesTab", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("load_Variables_ShowsATypedEditorForEach", () => {
        const { wrapper } = setup();

        const rows = rowsOf(wrapper);

        expect(rows).toHaveLength(2);
        expect((rows[0].get("[data-cy=variable-name]").element as HTMLInputElement).value).toBe("greeting");
        expect((rows[0].get("[data-cy=variable-type]").element as HTMLSelectElement).value).toBe("string");
        expect((rows[0].get("[data-cy=variable-default]").element as HTMLInputElement).value).toBe("Hello");
        expect(rows[1].get("[data-cy=variable-default]").attributes("type")).toBe("number");
        expect((rows[1].get("[data-cy=variable-default]").element as HTMLInputElement).value).toBe("0");
    });

    it("edit_DefaultChanged_SavesEveryVariableWithTheParsedValue", async () => {
        const { store, saveVariables, wrapper } = setup();
        const input = rowsOf(wrapper)[1].get("[data-cy=variable-default]");

        await input.setValue("5");
        await flushPromises();

        expect(saveVariables).toHaveBeenCalledWith(3, [
            { name: "greeting", typeName: "string", defaultValue: "Hello", description: "Shown to the user" },
            { name: "attempts", typeName: "number", defaultValue: 5, description: null },
        ]);
        expect(store.state.revision).toBe(4);
        expect(store.state.variables[1].defaultValue).toBe(5);
    });

    it("add_NewRow_IsFocusedAndOnlySavedOnceItHasAName", async () => {
        const { saveVariables, wrapper } = setup();

        await wrapper.get("[data-cy=variable-add]").trigger("click");
        await flushPromises();
        const row = rowsOf(wrapper)[2];

        expect(document.activeElement).toBe(row.get("[data-cy=variable-name]").element);
        expect((row.get("[data-cy=variable-type]").element as HTMLSelectElement).value).toBe("string");

        await rowsOf(wrapper)[0].get("[data-cy=variable-description]").trigger("change");
        await flushPromises();

        expect(saveVariables.mock.calls[0][1]).toHaveLength(2);

        await row.get("[data-cy=variable-name]").setValue(" flag ");
        await row.get("[data-cy=variable-type]").setValue("boolean");
        await flushPromises();

        expect(saveVariables.mock.lastCall![1][2]).toEqual({ name: "flag", typeName: "boolean", defaultValue: null, description: null });
        expect(row.get("[data-cy=variable-default]").element.tagName).toBe("SELECT");
    });

    it("delete_Variable_SavesTheOthers", async () => {
        const { saveVariables, wrapper } = setup();

        await rowsOf(wrapper)[0].get("[data-cy=variable-delete]").trigger("click");
        await flushPromises();

        expect(rowsOf(wrapper)).toHaveLength(1);
        expect(saveVariables.mock.calls[0][1].map((variable: VariableDefinition) => variable.name)).toEqual(["attempts"]);
    });

    it("save_InvalidJsonDefault_ShowsTheErrorWithoutSaving", async () => {
        const { saveVariables, wrapper } = setup();
        const type = rowsOf(wrapper)[1].get("[data-cy=variable-type]");
        await type.setValue("object");
        await flushPromises();
        saveVariables.mockClear();

        const json = rowsOf(wrapper)[1].get("[data-cy=variable-default]");
        await json.setValue("{ not json");
        await flushPromises();

        expect(saveVariables).not.toHaveBeenCalled();
        expect(rowsOf(wrapper)[1].get("[data-cy=variable-error]").text()).toBe("InvalidDefaultValue");
    });

    it("save_ServerRejectsDeclarations_ShowsEachErrorOnItsRow", async () => {
        const { wrapper } = setup({
            saveVariables: () =>
                Promise.reject(new DesignerApiError(400, { status: 400, variableErrors: [{ index: 1, name: "greeting", message: "Another variable is named 'greeting'." }] })),
        });

        const name = rowsOf(wrapper)[1].get("[data-cy=variable-name]");
        await name.setValue("Greeting");
        await flushPromises();

        expect(rowsOf(wrapper)[0].find("[data-cy=variable-error]").exists()).toBe(false);
        expect(rowsOf(wrapper)[1].get("[data-cy=variable-error]").text()).toBe("Another variable is named 'greeting'.");
        expect(rowsOf(wrapper)[1].classes()).toContain("is-invalid");
    });

    it("save_Conflict_EmitsTheError", async () => {
        const conflict = new DesignerApiError(409, { status: 409, currentRevision: 7 });
        const { wrapper } = setup({ saveVariables: () => Promise.reject(conflict) });

        await rowsOf(wrapper)[0].get("[data-cy=variable-name]").trigger("change");
        await flushPromises();

        expect(wrapper.emitted("error")).toEqual([[conflict]]);
    });

    it("edit_InputChecked_SavesTheVariableAsAnInput", async () => {
        const { saveVariables, wrapper } = setup();

        await rowsOf(wrapper)[1].get("[data-cy=variable-is-input]").setValue(true);
        await flushPromises();

        expect(saveVariables).toHaveBeenLastCalledWith(3, [
            { name: "greeting", typeName: "string", defaultValue: "Hello", description: "Shown to the user" },
            { name: "attempts", typeName: "number", defaultValue: 0, description: null, isInput: true },
        ]);
    });

    it("readOnly_InputsAndOutputs_AreMarked", async () => {
        const { store, wrapper } = setup({ readOnly: true });

        store.state.variables = [
            { ...store.state.variables[0], isInput: true },
            { ...store.state.variables[1], isOutput: true },
        ];
        await flushPromises();

        expect(wrapper.get("[data-cy=variable-row-greeting]").find("[data-cy=variable-input]").exists()).toBe(true);
        expect(wrapper.get("[data-cy=variable-row-greeting]").find("[data-cy=variable-output]").exists()).toBe(false);
        expect(wrapper.get("[data-cy=variable-row-attempts]").find("[data-cy=variable-output]").exists()).toBe(true);
    });

    it("readOnly_Instance_ShowsTheStoredValues", () => {
        const { wrapper } = setup({ readOnly: true, values: { greeting: "hi there" } });

        expect(wrapper.find("[data-cy=variable]").exists()).toBe(false);
        expect(wrapper.get("[data-cy=variable-row-greeting] [data-cy=variable-value]").text()).toBe("hi there");
        expect(wrapper.get("[data-cy=variable-row-attempts] [data-cy=variable-value]").text()).toBe("NoValue");
    });
});
