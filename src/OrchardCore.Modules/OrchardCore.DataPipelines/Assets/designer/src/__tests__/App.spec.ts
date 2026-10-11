import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import App from "../App.vue";
import { createDesignerStore } from "../state/designerStore";
import type { DesignerApi } from "../api/designerApi";
import type { DesignerConfig } from "../config";
import { createDefinition, createNode, createRun, library, records } from "./fixtures";
import { clearToasts } from "../ui/toasts";

const urls = {
    definition: "/d",
    library: "/l",
    save: "/s",
    addStep: "/a",
    editor: "/e",
    settings: "/st",
    fields: "/f",
    preview: "/p",
    publish: "/pub",
    discard: "/dis",
    versions: "/v",
    restore: "/r",
    run: "/run",
    runs: "/runs",
    cancelRun: "/cancel",
};

const config: DesignerConfig = { pipelineId: "pipeline-1", readOnly: false, urls, listUrl: "/list", translations: {} };

const createApi = (overrides: Partial<Record<keyof DesignerApi, unknown>> = {}) =>
    ({
        getDefinition: vi.fn().mockResolvedValue(createDefinition()),
        getLibrary: vi.fn().mockResolvedValue(library),
        getSettings: vi.fn().mockResolvedValue({ valid: true, content: '<input name="Name" />', scripts: "", styles: "" }),
        getEditor: vi.fn().mockResolvedValue({ valid: true, content: '<input name="Step.Condition" />', scripts: "", styles: "" }),
        getFields: vi.fn().mockResolvedValue({ stepId: "", inputs: [], outputs: [] }),
        getRuns: vi.fn().mockResolvedValue({ runs: [] }),
        save: vi.fn((payload) => Promise.resolve({ revision: payload.revision + 1, issues: [] })),
        ...overrides,
    }) as unknown as DesignerApi & Record<string, ReturnType<typeof vi.fn>>;

const mountApp = (api: DesignerApi, overrides: Partial<DesignerConfig> = {}) => {
    const store = createDesignerStore();
    const wrapper = mount(App, { props: { config: { ...config, ...overrides }, api, store }, attachTo: document.body });

    return { wrapper, store };
};

describe("App", () => {
    afterEach(() => {
        clearToasts();
        document.body.innerHTML = "";
    });

    it("mount_Definition_ShowsTheNameTheRegionsAndTheSteps", async () => {
        const { wrapper } = mountApp(createApi());
        await flushPromises();

        expect(wrapper.get("[data-cy=designer-toolbar]").text()).toContain("Monthly orders");
        expect(wrapper.get("[data-cy=toolbar-list]").attributes("href")).toBe("/list");
        expect(wrapper.findAll("[data-cy^=toolbox-group-]").map((group) => group.attributes("data-cy"))).toEqual([
            "toolbox-group-Source",
            "toolbox-group-Transform",
            "toolbox-group-File",
            "toolbox-group-Destination",
        ]);
        expect(wrapper.findAll(".dpd-node")).toHaveLength(6);
        expect(wrapper.findAll(".dpd-edge")).toHaveLength(2);
        expect(wrapper.find("[data-cy=designer-panel]").exists()).toBe(true);
    });

    it("mount_EmptyPipeline_OffersTheSourcesToStartWith", async () => {
        const addStep = vi.fn().mockResolvedValue({ revision: 1, node: createNode("new", { category: "Source", inputs: [] }), connection: null, issues: [] });
        const api = createApi({ getDefinition: vi.fn().mockResolvedValue({ ...createDefinition(), nodes: [], connections: [] }), addStep });
        const { wrapper, store } = mountApp(api);
        await flushPromises();

        expect(wrapper.findAll("[data-cy^=start-picker-]").map((button) => button.text())).toEqual(["Content items", "Utilisateurs"]);

        await wrapper.get("[data-cy=start-picker-UsersSource]").trigger("click");
        await flushPromises();

        expect(addStep.mock.calls[0][0]).toMatchObject({ revision: 0, type: "UsersSource", connectFrom: null });
        expect(store.state.nodes.map((node) => node.id)).toEqual(["new"]);
        expect(store.state.canUndo).toBe(true);
    });

    it("publish_DraftWithErrors_IsDisabled", async () => {
        const api = createApi({
            getDefinition: vi.fn().mockResolvedValue({ ...createDefinition(), hasDraft: true, issues: [{ severity: "Error", message: "The join has no right input.", stepId: "join" }] }),
        });
        const { wrapper } = mountApp(api);
        await flushPromises();

        expect(wrapper.get("[data-cy=toolbar-publish]").attributes("disabled")).toBeDefined();
        expect(wrapper.get("[data-cy=issues-count]").text()).toBe("1");
    });

    it("quickAdd_UnconnectedOutput_AddsACompatibleStepConnectedByTheServer", async () => {
        const node = createNode("sort", { x: 640, y: 0 });
        const connection = { sourceStepId: "filter", sourcePort: "Unmatched", targetStepId: "sort", targetPort: "Input" };
        const addStep = vi.fn().mockResolvedValue({ revision: 1, node, connection, issues: [] });
        const { wrapper, store } = mountApp(createApi({ addStep }));
        await flushPromises();

        const port = wrapper.get("[data-cy=port-filter-out-Unmatched]");
        port.element.dispatchEvent(new PointerEvent("pointerdown", { button: 0, pointerId: 1, bubbles: true }));
        window.dispatchEvent(new PointerEvent("pointerup", { pointerId: 1, clientX: 5, clientY: 5 }));
        await flushPromises();

        const items = wrapper.findAll(".dpd-quick-add-item");
        expect(items.map((item) => item.attributes("data-cy"))).toEqual(["quick-add-FilterStep", "quick-add-JoinStep", "quick-add-CsvFile", "quick-add-TableDestination"]);

        await wrapper.get("[data-cy=quick-add-FilterStep]").trigger("click");
        await flushPromises();

        expect(addStep.mock.calls[0][0]).toMatchObject({ type: "FilterStep", connectFrom: { stepId: "filter", port: "Unmatched" } });
        expect(store.state.connections).toContainEqual(connection);

        // One undo removes the step and its connection.
        store.undo();
        expect(store.getNode("sort")).toBeUndefined();
        expect(store.state.connections).not.toContainEqual(connection);
    });

    it("run_PublishedPipeline_QueuesARunAndShowsItsStepsOnTheCanvas", async () => {
        const run = vi.fn().mockResolvedValue(createRun({ status: "Succeeded" }));
        const api = createApi({
            getDefinition: vi.fn().mockResolvedValue({ ...createDefinition(), publishedVersion: { versionId: "v3", number: 3, publishedUtc: "2026-10-01T00:00:00Z", isPublished: true } }),
            run,
        });
        const { wrapper, store } = mountApp(api);
        await flushPromises();

        await wrapper.get("[data-cy=toolbar-run]").trigger("click");
        await flushPromises();

        expect(run).toHaveBeenCalledTimes(1);
        expect(store.state.overlayRun?.runId).toBe("run-1");
        expect(wrapper.get("[data-cy=step-source] [data-cy=run-badge]").text()).toBe("1,234 rows");
        expect(wrapper.get("[data-cy=step-source]").classes()).toContain("is-run-succeeded");
        expect(wrapper.find("[data-cy=runs-tab]").exists()).toBe(true);
    });

    it("run_NeverPublished_IsDisabledWithAReason", async () => {
        const { wrapper } = mountApp(createApi());
        await flushPromises();

        const button = wrapper.get("[data-cy=toolbar-run]");
        expect(button.attributes("disabled")).toBeDefined();
        expect(button.attributes("title")).toBe("Publish the pipeline to run it.");
    });

    it("versionPage_ReadOnly_HidesTheToolboxAndTheEditing", async () => {
        const api = createApi({
            getDefinition: vi.fn().mockResolvedValue({
                ...createDefinition(),
                version: { versionId: "v2", number: 2, publishedUtc: "2026-10-01T00:00:00Z", isPublished: false },
            }),
        });
        const { wrapper } = mountApp(api, {
            mode: "version",
            designerUrl: "/designer",
            urls: { definition: "/d", library: "/l", fields: "/f" },
        });
        await flushPromises();

        expect(wrapper.find("[data-cy=designer-toolbox]").exists()).toBe(false);
        expect(wrapper.find("[data-cy=toolbar-publish]").exists()).toBe(false);
        expect(wrapper.get("[data-cy=page-version]").text()).toBe("Version 2");
        expect(wrapper.get("[data-cy=page-version-designer]").attributes("href")).toBe("/designer");
        expect(wrapper.get("[data-cy=panel-settings-summary]").text()).toContain("Monthly orders");
        expect(api.getLibrary).not.toHaveBeenCalled();
    });

    it("editorApplied_PortRemoved_DropsItsConnection", async () => {
        const postEditor = vi.fn().mockResolvedValue({
            valid: true,
            revision: 1,
            node: createNode("filter", { x: 300, outputs: [records("Unmatched")] }),
            removedConnections: [{ sourceStepId: "filter", sourcePort: "Matched", targetStepId: "join", targetPort: "Left" }],
            issues: [],
        });
        const { wrapper, store } = mountApp(createApi({ postEditor }), { initialStepId: "filter" });
        await flushPromises();

        const input = wrapper.get("[data-cy=step-panel] input[name='Step.Condition']");
        await input.trigger("input");
        await (wrapper.vm as unknown as { stepPanel: { settle: () => Promise<boolean> } }).stepPanel.settle();
        await flushPromises();

        expect(postEditor).toHaveBeenCalledWith("filter", 0, expect.any(FormData));
        expect(store.state.connections.map((connection) => connection.targetStepId)).toEqual(["filter"]);
    });
});
