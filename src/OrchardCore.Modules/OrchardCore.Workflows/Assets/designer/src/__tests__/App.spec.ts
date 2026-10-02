import { describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import { nextTick } from "vue";
import App from "../App.vue";
import { createDesignerStore } from "../state/designerStore";
import { loadTranslations } from "../i18n";
import type { DesignerApi } from "../api/designerApi";
import type { DesignerConfig } from "../config";
import { createDefinition } from "../canvas/__tests__/fixtures";

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
            getLibrary: vi.fn().mockResolvedValue({ categories: [] }),
        } as unknown as DesignerApi;

        const wrapper = mount(App, { props: { config, api, store: createDesignerStore() } });
        await flushPromises();

        expect(wrapper.get("[data-cy=designer-toolbar]").text()).toContain("Order approval");
        expect(wrapper.find("[data-cy=designer-toolbox]").exists()).toBe(true);
        expect(wrapper.find("[data-cy=designer-canvas]").exists()).toBe(true);
        expect(wrapper.find("[data-cy=designer-panel]").exists()).toBe(true);
        expect(wrapper.get("[data-cy=toolbar-undo]").attributes("disabled")).toBeDefined();
    });

    it("mount_InitialActivity_SelectsAndOpensIt", async () => {
        const api = {
            getDefinition: vi.fn().mockResolvedValue(createDefinition()),
            getLibrary: vi.fn().mockResolvedValue({ categories: [] }),
            getEditor: vi.fn().mockResolvedValue({ valid: true, content: '<input name="NotifyTask.Message" />', scripts: "", styles: "" }),
        } as unknown as DesignerApi & { getEditor: ReturnType<typeof vi.fn> };
        const store = createDesignerStore();

        const wrapper = mount(App, {
            props: { config: { ...config, instancesUrl: "/instances", exportUrl: "/export", initialActivityId: "a" }, api, store },
            attachTo: document.body,
        });
        await flushPromises();

        expect(store.state.selectedNodeIds).toEqual(["a"]);
        expect(api.getEditor).toHaveBeenCalledWith("a");
        // The links of the old editor's toolbar.
        expect(wrapper.get("[data-cy=toolbar-instances]").attributes("href")).toBe("/instances");
        expect(wrapper.get("[data-cy=toolbar-export]").attributes()).toMatchObject({ href: "/export", "data-url-af": "UnsafeUrl" });
        wrapper.unmount();
    });

    it("toolbox_Collapsed_HoverOpensItAndADragKeepsItOpen", async () => {
        const api = {
            getDefinition: vi.fn().mockResolvedValue(createDefinition()),
            getLibrary: vi.fn().mockResolvedValue({ categories: [] }),
        } as unknown as DesignerApi;
        const wrapper = mount(App, { props: { config, api, store: createDesignerStore() }, attachTo: document.body });
        await flushPromises();
        const toolbox = wrapper.get("[data-cy=designer-toolbox]");

        await wrapper.get("[data-cy=toolbox-collapse]").trigger("click");

        expect(toolbox.classes()).toContain("is-collapsed");
        expect(wrapper.get("[data-cy=toolbox-sheet]").isVisible()).toBe(false);
        expect(window.localStorage.getItem("orchardcore:workflows-designer:toolbox-collapsed")).toBe("true");

        vi.useFakeTimers();

        try {
            await toolbox.trigger("mouseenter");
            vi.advanceTimersByTime(150);
            await nextTick();
            expect(toolbox.classes()).toContain("is-peeking");

            // Dragging an activity to the canvas leaves the toolbox; it stays open until the drag ends.
            await wrapper.get("[data-cy=toolbox-sheet]").trigger("dragstart");
            await toolbox.trigger("mouseleave");
            vi.advanceTimersByTime(1000);
            await nextTick();
            expect(toolbox.classes()).toContain("is-peeking");

            await wrapper.get("[data-cy=toolbox-sheet]").trigger("dragend");
            vi.advanceTimersByTime(300);
            await nextTick();
            expect(toolbox.classes()).not.toContain("is-peeking");
        } finally {
            vi.useRealTimers();
        }

        await wrapper.get("[data-cy=toolbox-expand]").trigger("click");
        expect(toolbox.classes()).not.toContain("is-collapsed");
        wrapper.unmount();
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
