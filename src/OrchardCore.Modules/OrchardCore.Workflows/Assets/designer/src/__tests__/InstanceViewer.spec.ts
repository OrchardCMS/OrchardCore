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
