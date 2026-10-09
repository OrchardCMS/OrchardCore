import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import RunDialog from "../RunDialog.vue";
import App from "../../App.vue";
import { createDesignerStore } from "../../state/designerStore";
import { createDefinition } from "../../canvas/__tests__/fixtures";
import type { DesignerApi } from "../../api/designerApi";
import type { DesignerRun } from "../../api/types";
import type { DesignerConfig } from "../../config";

const inputsRun: DesignerRun = {
    mode: "inputs",
    isEnabled: true,
    activityId: "start",
    inputs: [
        { name: "amount", typeName: "number", isInput: true },
        { name: "notify", typeName: "boolean", isInput: true },
        { name: "order", typeName: "object", isInput: true },
        { name: "note", typeName: "string", isInput: true },
    ],
};

const httpRun: DesignerRun = { mode: "http", isEnabled: true, activityId: "request", httpMethod: "POST", inputs: [] };

const setup = async (run: DesignerRun | null, api: Record<string, unknown> = {}, hasChanges = false) => {
    const designerApi = {
        getDefinition: vi.fn().mockResolvedValue({ ...createDefinition(), run }),
        ...api,
    } as unknown as DesignerApi & Record<string, ReturnType<typeof vi.fn>>;
    const wrapper = mount(RunDialog, {
        props: { api: designerApi, workflowTypeId: 7, hasChanges },
        attachTo: document.body,
        global: { stubs: { teleport: true } },
    });
    await flushPromises();

    return { api: designerApi, wrapper };
};

describe("RunDialog", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("run_WorkflowStartedByWorkflow_RunsWithTheTypedInputsAndShowsTheResult", async () => {
        const run = vi.fn().mockResolvedValue({ instanceId: 12, instanceUrl: "/instances/12", status: "Finished", outputs: { doubled: 42 } });
        const { wrapper } = await setup(inputsRun, { run });

        await wrapper.get("[data-cy=run-input-amount] input").setValue("21");
        await wrapper.get("[data-cy=run-input-notify] input").setValue(true);
        await wrapper.get("[data-cy=run-input-order] textarea").setValue('{ "id": 5 }');
        await wrapper.get("form").trigger("submit");
        await flushPromises();

        // The empty field keeps its variable's default value.
        expect(run).toHaveBeenCalledWith({ amount: 21, notify: true, order: { id: 5 } });
        expect(wrapper.get("[data-cy=run-status]").text()).toBe("Finished");
        expect(wrapper.get("[data-cy=run-output-doubled]").text()).toBe("42");
        expect(wrapper.get("[data-cy=run-open-instance]").attributes("href")).toBe("/instances/12");
        wrapper.unmount();
    });

    it("run_InvalidJsonInput_SaysSoAndRunsNothing", async () => {
        const run = vi.fn();
        const { wrapper } = await setup(inputsRun, { run });

        await wrapper.get("[data-cy=run-input-order] textarea").setValue("{ id: ");
        await wrapper.get("form").trigger("submit");
        await flushPromises();

        expect(run).not.toHaveBeenCalled();
        expect(wrapper.get("[data-cy=run-error]").text()).toBe("RunInvalidJson");
        wrapper.unmount();
    });

    it("run_WorkflowStartedByAnHttpRequest_SendsTheRequestAndFindsTheInstanceItStarted", async () => {
        const getLatestInstance = vi
            .fn()
            .mockResolvedValueOnce({ instanceId: 3, status: "Finished" })
            .mockResolvedValueOnce({ instanceId: 4, instanceUrl: "/instances/4", status: "Halted" });
        const generateHttpUrl = vi.fn().mockResolvedValue("/invoke?token=abc");
        const sendHttpRequest = vi.fn().mockResolvedValue({ status: 200, body: "big" });
        const { wrapper } = await setup(httpRun, { getLatestInstance, generateHttpUrl, sendHttpRequest });

        await wrapper.get("[data-cy=run-query]").setValue("?size=3");
        await wrapper.get("[data-cy=run-body]").setValue('{ "a": 1 }');
        await wrapper.get("form").trigger("submit");
        await flushPromises();

        expect(generateHttpUrl).toHaveBeenCalledWith(7, "request");
        expect(sendHttpRequest).toHaveBeenCalledWith("/invoke?token=abc&size=3", "POST", '{ "a": 1 }', "application/json");
        expect(wrapper.get("[data-cy=run-response-status]").text()).toBe("200");
        expect(wrapper.get("[data-cy=run-response-body]").text()).toBe("big");
        expect(wrapper.get("[data-cy=run-status]").text()).toBe("Halted");
        expect(wrapper.get("[data-cy=run-open-instance]").attributes("href")).toBe("/instances/4");
        wrapper.unmount();
    });

    it("run_RequestThatStartedNoInstance_SaysSo", async () => {
        const getLatestInstance = vi.fn().mockResolvedValue({ instanceId: 3, status: "Finished" });
        const { wrapper } = await setup(
            { ...httpRun, httpMethod: "GET" },
            { getLatestInstance, generateHttpUrl: vi.fn().mockResolvedValue("/invoke"), sendHttpRequest: vi.fn().mockResolvedValue({ status: 404, body: "" }) },
        );

        // A GET request has no body.
        expect(wrapper.find("[data-cy=run-body]").exists()).toBe(false);

        await wrapper.get("form").trigger("submit");
        await flushPromises();

        expect(wrapper.get("[data-cy=run-response-status]").classes()).toContain("text-bg-danger");
        expect(wrapper.find("[data-cy=run-no-instance]").exists()).toBe(true);
        wrapper.unmount();
    });

    it("open_WorkflowThatCantRunOrIsDisabled_SaysWhy", async () => {
        const unavailable = await setup({ mode: null, isEnabled: true, inputs: [] }, {}, true);

        expect(unavailable.wrapper.find("[data-cy=run-draft-note]").exists()).toBe(true);
        expect(unavailable.wrapper.find("[data-cy=run-unavailable]").exists()).toBe(true);
        expect(unavailable.wrapper.find("[data-cy=run-start]").exists()).toBe(false);
        unavailable.wrapper.unmount();

        const disabled = await setup({ ...inputsRun, isEnabled: false });

        expect(disabled.wrapper.find("[data-cy=run-disabled]").exists()).toBe(true);
        expect(disabled.wrapper.get("[data-cy=run-start]").attributes("disabled")).toBeDefined();
        disabled.wrapper.unmount();
    });

    it("toolbar_UserCanExecute_OpensTheDialog", async () => {
        const config: DesignerConfig = {
            workflowTypeId: 7,
            readOnly: false,
            urls: { definition: "", library: "", save: "", addActivity: "", editor: "", settings: "", publish: "", discard: "", run: "/run" },
            instancesUrl: "",
            exportUrl: "",
            listUrl: "",
            translations: {},
        };
        const api = {
            getDefinition: vi.fn().mockResolvedValue({ ...createDefinition(), run: inputsRun }),
            getLibrary: vi.fn().mockResolvedValue({ categories: [] }),
            save: vi.fn(),
        } as unknown as DesignerApi;
        const wrapper = mount(App, { props: { config, api, store: createDesignerStore() }, attachTo: document.body, global: { stubs: { teleport: true } } });
        await flushPromises();

        await wrapper.get("[data-cy=toolbar-run]").trigger("click");
        await flushPromises();

        expect(wrapper.find("[data-cy=run-dialog]").exists()).toBe(true);
        expect(wrapper.find("[data-cy=run-input-amount]").exists()).toBe(true);
        wrapper.unmount();

        const withoutRun = mount(App, {
            props: { config: { ...config, urls: { ...config.urls, run: null } }, api, store: createDesignerStore() },
            attachTo: document.body,
        });
        await flushPromises();

        expect(withoutRun.find("[data-cy=toolbar-run]").exists()).toBe(false);
        withoutRun.unmount();
    });
});
