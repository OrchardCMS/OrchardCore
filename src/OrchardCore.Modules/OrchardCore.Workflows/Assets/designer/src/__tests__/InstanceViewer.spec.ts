import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import App from "../App.vue";
import { createDesignerStore } from "../state/designerStore";
import { selectNode } from "../canvas/useConnect";
import type { DesignerApi } from "../api/designerApi";
import type { DesignerConfig } from "../config";
import { createDefinition } from "../canvas/__tests__/fixtures";

const config: DesignerConfig = {
    workflowTypeId: 1,
    readOnly: true,
    urls: { definition: "/instance", library: "", save: "", addActivity: "", editor: "", settings: "", publish: "", discard: "" },
    instancesUrl: "",
    exportUrl: "",
    listUrl: "",
    translations: {},
};

const setup = async () => {
    const api = {
        getDefinition: vi.fn().mockResolvedValue({
            ...createDefinition(),
            instance: { id: 12, workflowId: "wf-12", status: "Halted", blockingActivityIds: ["a"] },
        }),
        getLibrary: vi.fn(),
        save: vi.fn(),
    } as unknown as DesignerApi & Record<"getLibrary" | "save", ReturnType<typeof vi.fn>>;
    const store = createDesignerStore();
    const wrapper = mount(App, { props: { config, api, store }, attachTo: document.body });
    await flushPromises();

    return { api, store, wrapper };
};

describe("instance viewer", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("mount_ReadOnly_ShowsTheGraphWithoutTheEditingTools", async () => {
        const { api, wrapper } = await setup();

        expect(api.getLibrary).not.toHaveBeenCalled();
        expect(wrapper.find("[data-cy=designer-toolbox]").exists()).toBe(false);
        expect(wrapper.find("[data-cy=save-status]").exists()).toBe(false);
        expect(wrapper.find("[data-cy=toolbar-publish]").exists()).toBe(false);
        expect(wrapper.find("[data-cy=panel-tab-workflow]").exists()).toBe(false);
        expect(wrapper.find("[data-cy=panel-tab-issues]").exists()).toBe(false);
        expect(wrapper.find("[data-cy=viewer-legend]").exists()).toBe(true);
        wrapper.unmount();
    });

    it("mount_BlockingActivities_AreHighlighted", async () => {
        const { wrapper } = await setup();

        expect(wrapper.get("[data-cy=activity-a]").classes()).toContain("is-blocking");
        expect(wrapper.get("[data-cy=activity-b]").classes()).not.toContain("is-blocking");
        expect(wrapper.findAll("[data-cy=blocking-badge]")).toHaveLength(1);
        wrapper.unmount();
    });

    it("mount_BlockingActivityInACollapsedBranch_ExpandsIt", async () => {
        // The blocking activity is behind both collapsed activities (fork is nested in the start's branch).
        window.localStorage.setItem("orchardcore:workflows-designer:collapsed:type-1", '["start","fork"]');
        const { wrapper } = await setup();

        expect(wrapper.find("[data-cy=activity-a]").exists()).toBe(true);
        expect(window.localStorage.getItem("orchardcore:workflows-designer:collapsed:type-1")).toBe("[]");
        wrapper.unmount();
    });

    it("mount_FaultedInstance_ShowsTheExecutedPathTheJournalAndRetries", async () => {
        const instance = {
            id: 12,
            workflowId: "wf-12",
            status: "Faulted",
            blockingActivityIds: [],
            faultMessage: "Boom",
            faultedActivityId: "a",
            executedActivityCounts: { start: 1, fork: 1, a: 1 },
            executedTransitionCounts: { "start:Done:fork": 1, "fork:A:a": 1 },
            journal: [
                { sequence: 1, activityId: "start", activityName: "HttpRequestEvent", isResume: false, status: "Completed", outcomes: ["Done"], startedUtc: "", completedUtc: "", durationMilliseconds: 1 },
                { sequence: 2, activityId: "a", activityName: "NotifyTask", isResume: false, status: "Faulted", outcomes: [], startedUtc: "", completedUtc: "", durationMilliseconds: 3, error: "Boom" },
            ],
        };
        const api = {
            getDefinition: vi
                .fn()
                .mockResolvedValueOnce({ ...createDefinition(), instance })
                .mockResolvedValueOnce({ ...createDefinition(), instance: { id: 12, workflowId: "wf-12", status: "Finished", blockingActivityIds: [] } }),
            getLibrary: vi.fn(),
            save: vi.fn(),
            retry: vi.fn().mockResolvedValue({ status: "Finished" }),
        } as unknown as DesignerApi & Record<"getDefinition" | "retry", ReturnType<typeof vi.fn>>;
        vi.spyOn(window, "confirm").mockReturnValue(true);
        const store = createDesignerStore();
        const wrapper = mount(App, { props: { config: { ...config, urls: { ...config.urls, retry: "/retry" } }, api, store }, attachTo: document.body });
        await flushPromises();

        expect(wrapper.get("[data-cy=activity-start]").classes()).toContain("is-executed");
        expect(wrapper.get("[data-cy=activity-a]").classes()).toContain("is-faulted");
        expect(wrapper.get("[data-cy=activity-b]").classes()).not.toContain("is-executed");
        expect(wrapper.get("[data-cy='edge-start:Done:fork']").classes()).toContain("is-executed");
        expect(wrapper.find("[data-cy=panel-tab-journal]").exists()).toBe(true);

        selectNode(store, "a");
        await flushPromises();

        expect(wrapper.get("[data-cy=fault-message]").text()).toBe("Boom");

        await wrapper.get("[data-cy=retry-button]").trigger("click");
        await flushPromises();

        expect(api.retry).toHaveBeenCalledWith(12, "a");
        expect(api.getDefinition).toHaveBeenCalledTimes(2);
        expect(store.state.instance?.status).toBe("Finished");
        expect(wrapper.find("[data-cy=retry]").exists()).toBe(false);
        wrapper.unmount();
        vi.restoreAllMocks();
    });

    it("mount_FaultedInstanceWithoutRetryPermission_OffersNoRetry", async () => {
        const { store, wrapper } = await setup();
        store.state.instance = { id: 12, workflowId: "wf-12", status: "Faulted", blockingActivityIds: [] };

        selectNode(store, "a");
        await flushPromises();

        expect(wrapper.find("[data-cy=retry]").exists()).toBe(false);
        wrapper.unmount();
    });

    it("select_Activity_ShowsAReadOnlySummary", async () => {
        const { api, store, wrapper } = await setup();

        selectNode(store, "a");
        await flushPromises();

        const summary = wrapper.get("[data-cy=panel-summary]");
        expect(summary.text()).toContain("Title a");
        expect(summary.get("[data-cy=panel-summary-blocking]").text()).toBe("WaitingOnActivity");
        expect(wrapper.find("[data-cy=panel-activity-form]").exists()).toBe(false);

        selectNode(store, "b");
        await flushPromises();

        expect(wrapper.get("[data-cy=panel-summary-blocking]").text()).toBe("NotWaitingOnActivity");
        expect(api.save).not.toHaveBeenCalled();
        wrapper.unmount();
    });
});
