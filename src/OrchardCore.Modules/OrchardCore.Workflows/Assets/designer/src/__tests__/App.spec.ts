import { describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import App from "../App.vue";
import { createDesignerStore } from "../state/designerStore";
import { loadTranslations } from "../i18n";
import type { DesignerApi } from "../api/designerApi";
import type { DesignerConfig } from "../config";

const config: DesignerConfig = {
    workflowTypeId: 7,
    readOnly: false,
    urls: { definition: "", library: "", save: "", addActivity: "", editor: "", settings: "", publish: "", discard: "" },
    instancesUrl: "",
    exportUrl: "",
    listUrl: "",
    translations: {},
};

describe("App", () => {
    it("mount_DefinitionLoaded_ShowsWorkflowNameAndRegions", async () => {
        const api = {
            getDefinition: vi.fn().mockResolvedValue({
                id: 7,
                workflowTypeId: "type-7",
                revision: 0,
                hasDraft: false,
                settings: { name: "Order approval", isEnabled: true, isSingleton: false, lockTimeout: 0, lockExpiration: 0, deleteFinishedWorkflows: false },
                nodes: [],
                transitions: [],
                issues: [],
                runningInstanceCount: 0,
            }),
        } as unknown as DesignerApi;

        const wrapper = mount(App, { props: { config, api, store: createDesignerStore() } });
        await flushPromises();

        expect(wrapper.get("[data-cy=designer-toolbar]").text()).toContain("Order approval");
        expect(wrapper.find("[data-cy=designer-toolbox]").exists()).toBe(true);
        expect(wrapper.find("[data-cy=designer-canvas]").exists()).toBe(true);
        expect(wrapper.find("[data-cy=designer-panel]").exists()).toBe(true);
        expect(wrapper.get("[data-cy=toolbar-undo]").attributes("disabled")).toBeDefined();
    });

    it("mount_LoadFails_ShowsLocalizedError", async () => {
        loadTranslations({ LoadFailed: "Could not load the workflow." });
        const api = { getDefinition: vi.fn().mockRejectedValue(new Error("boom")) } as unknown as DesignerApi;

        const wrapper = mount(App, { props: { config: { ...config, readOnly: true }, api, store: createDesignerStore() } });
        await flushPromises();

        expect(wrapper.get("[role=alert]").text()).toBe("Could not load the workflow.");
        expect(wrapper.find("[data-cy=designer-toolbox]").exists()).toBe(false);
    });
});
