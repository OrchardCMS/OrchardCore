import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import App from "../App.vue";
import { createDesignerStore } from "../state/designerStore";
import { moveNodesCommand } from "../state/commands";
import { DesignerApiError, type DesignerApi } from "../api/designerApi";
import type { DesignerDefinition } from "../api/types";
import type { DesignerConfig } from "../config";
import { clearToasts, useToasts } from "../ui/toasts";
import { createDefinition } from "../canvas/__tests__/fixtures";

const config: DesignerConfig = {
    workflowTypeId: 1,
    readOnly: false,
    urls: { definition: "", library: "", save: "", addActivity: "", editor: "", settings: "", publish: "", discard: "" },
    instancesUrl: "",
    exportUrl: "",
    listUrl: "",
    currentUserId: "me",
    translations: {},
};

type ApiMock = Record<keyof DesignerApi, ReturnType<typeof vi.fn>>;

const setup = async (definition: Partial<DesignerDefinition> = {}, api: Partial<ApiMock> = {}) => {
    const loaded: DesignerDefinition = { ...createDefinition(), hasDraft: true, revision: 2, draftModifiedByUserId: "me", ...definition };
    const mocks = {
        // A fresh copy each time, like a response: the store edits the objects it is given.
        getDefinition: vi.fn(() => Promise.resolve(structuredClone(loaded))),
        getLibrary: vi.fn().mockResolvedValue({ categories: [] }),
        save: vi.fn((payload: { revision: number }) => Promise.resolve({ revision: payload.revision + 1, issues: [] })),
        publish: vi.fn().mockResolvedValue({ issues: [], publishedUtc: "2026-10-02T10:00:00Z" }),
        discard: vi.fn().mockResolvedValue({}),
        getEditor: vi.fn().mockResolvedValue({ valid: true, content: "", scripts: "", styles: "" }),
        ...api,
    } as unknown as ApiMock;
    const store = createDesignerStore();
    // The dialogs are teleported to <body>; the stub renders them in place.
    const wrapper = mount(App, { props: { config, api: mocks as unknown as DesignerApi, store }, attachTo: document.body, global: { stubs: { teleport: true } } });
    await flushPromises();

    return { api: mocks, store, wrapper, vm: wrapper.vm as unknown as { autosave: { save: () => Promise<void> } } };
};

const move = async (store: ReturnType<typeof createDesignerStore>) => {
    store.execute(moveNodesCommand(store.graph, [{ id: "a", fromX: 600, fromY: 0, toX: 640, toY: 0 }]));
    await flushPromises();
};

describe("draft flow", () => {
    afterEach(() => {
        clearToasts();
        vi.restoreAllMocks();
        document.body.innerHTML = "";
    });

    it("publish_WarningsAndRunningInstances_AsksThenPublishesAnyway", async () => {
        const { api, store, wrapper } = await setup({
            issues: [{ severity: "Warning", code: "UnreachableActivity", message: "Unreachable", activityId: "b" }],
            runningInstanceCount: 2,
        });

        await wrapper.get("[data-cy=toolbar-publish]").trigger("click");
        await flushPromises();

        expect(api.publish).not.toHaveBeenCalled();
        const dialog = wrapper.get("[data-cy=publish-confirm-dialog]");
        expect(dialog.findAll(".wfd-issue")).toHaveLength(1);
        expect(dialog.get("[data-cy=publish-running-instances]").text()).toBe("RunningInstancesContinue");
        expect(dialog.get("[data-cy=publish-dialog-confirm]").text()).toBe("PublishAnyway");

        await dialog.get("[data-cy=publish-dialog-confirm]").trigger("click");
        await flushPromises();

        expect(api.publish).toHaveBeenCalledWith(2);
        expect(wrapper.find("[data-cy=publish-confirm-dialog]").exists()).toBe(false);
        expect(store.state.hasDraft).toBe(false);
        expect(store.state.revision).toBe(0);
        expect(useToasts().map((toast) => toast.variant)).toEqual(["success"]);
        wrapper.unmount();
    });

    it("publish_Errors_IsBlockedWithoutCallingTheServer", async () => {
        const { api, wrapper } = await setup({ issues: [{ severity: "Error", code: "InvalidTransition", message: "Broken", activityId: "a" }] });

        await wrapper.get("[data-cy=toolbar-publish]").trigger("click");
        await flushPromises();

        expect(wrapper.find("[data-cy=publish-blocked-dialog]").exists()).toBe(true);
        expect(api.publish).not.toHaveBeenCalled();

        await wrapper.get("[data-cy=publish-dialog-close]").trigger("click");
        expect(wrapper.find("[data-cy=publish-blocked-dialog]").exists()).toBe(false);
        wrapper.unmount();
    });

    it("publish_PendingChanges_SavesThemFirst", async () => {
        const { api, store, wrapper } = await setup();

        await move(store);
        await wrapper.get("[data-cy=toolbar-publish]").trigger("click");
        await flushPromises();

        // The change was saved (revision 2 → 3) before publishing, without waiting for the debounce.
        expect(api.save).toHaveBeenCalledTimes(1);
        expect(api.publish).toHaveBeenCalledWith(3);
        expect(api.save.mock.invocationCallOrder[0]).toBeLessThan(api.publish.mock.invocationCallOrder[0]);
        wrapper.unmount();
    });

    it("conflict_Reload_LoadsTheDefinitionAgain", async () => {
        const { api, store, wrapper, vm } = await setup({}, { save: vi.fn().mockRejectedValue(new DesignerApiError(409, { currentRevision: 6, modifiedBy: "Bob" })) });

        await move(store);
        await vm.autosave.save();
        await flushPromises();

        expect(store.state.saveStatus).toBe("conflict");
        expect(wrapper.get("[data-cy=save-status]").attributes("data-status")).toBe("conflict");

        await wrapper.get("[data-cy=conflict-reload]").trigger("click");
        await flushPromises();

        expect(api.getDefinition).toHaveBeenCalledTimes(2);
        expect(store.state.saveStatus).toBe("saved");
        expect(store.getNode("a")?.x).toBe(600);
        expect(wrapper.find("[data-cy=conflict-dialog]").exists()).toBe(false);
        wrapper.unmount();
    });

    it("conflict_Overwrite_SavesWithTheServerRevisionThenReloads", async () => {
        const save = vi
            .fn()
            .mockRejectedValueOnce(new DesignerApiError(409, { currentRevision: 6, modifiedBy: "Bob" }))
            .mockResolvedValueOnce({ revision: 7, issues: [] });
        const { api, store, wrapper, vm } = await setup({}, { save });

        await move(store);
        await vm.autosave.save();
        await flushPromises();

        await wrapper.get("[data-cy=conflict-overwrite]").trigger("click");
        await flushPromises();

        expect(save).toHaveBeenCalledTimes(2);
        expect(save.mock.calls[1][0]).toMatchObject({ revision: 6 });
        // The result is loaded, since the draft keeps what the other person added.
        expect(api.getDefinition).toHaveBeenCalledTimes(2);
        expect(store.state.saveStatus).toBe("saved");
        wrapper.unmount();
    });

    it("conflict_DialogClosed_ReopensFromTheStatus", async () => {
        const { store, wrapper, vm } = await setup({}, { save: vi.fn().mockRejectedValue(new DesignerApiError(409, { currentRevision: 6 })) });

        await move(store);
        await vm.autosave.save();
        await flushPromises();
        await wrapper.get("[data-cy=dialog-close]").trigger("click");

        expect(wrapper.find("[data-cy=conflict-dialog]").exists()).toBe(false);
        expect(store.state.saveStatus).toBe("conflict");

        await wrapper.get("[data-cy=save-resolve]").trigger("click");
        expect(wrapper.find("[data-cy=conflict-dialog]").exists()).toBe(true);
        wrapper.unmount();
    });

    it("discard_Confirmed_DiscardsAndLoadsTheLiveDefinition", async () => {
        vi.spyOn(window, "confirm").mockReturnValue(true);
        const { api, store, wrapper } = await setup();
        api.getDefinition.mockResolvedValue({ ...createDefinition(), hasDraft: false, revision: 0 });

        await move(store);
        await wrapper.get("[data-cy=toolbar-discard]").trigger("click");
        await flushPromises();

        expect(api.discard).toHaveBeenCalledTimes(1);
        // The pending change was dropped, not saved.
        expect(api.save).not.toHaveBeenCalled();
        expect(store.state.hasDraft).toBe(false);
        expect(store.state.saveStatus).toBe("saved");
        expect(store.getNode("a")?.x).toBe(600);
        wrapper.unmount();
    });

    it("discard_Cancelled_KeepsTheDraft", async () => {
        vi.spyOn(window, "confirm").mockReturnValue(false);
        const { api, wrapper } = await setup();

        await wrapper.get("[data-cy=toolbar-discard]").trigger("click");
        await flushPromises();

        expect(api.discard).not.toHaveBeenCalled();
        wrapper.unmount();
    });

    it("banner_DraftLastEditedBySomeoneElse_IsShownUntilDismissed", async () => {
        const { wrapper } = await setup({ draftModifiedBy: "Bob", draftModifiedByUserId: "bob", draftModifiedUtc: "2026-10-01T08:30:00Z" });

        const banner = wrapper.get("[data-cy=draft-banner]");
        expect(banner.text()).toContain("DraftLastEditedBy");

        await banner.get("[data-cy=draft-banner-dismiss]").trigger("click");
        expect(wrapper.find("[data-cy=draft-banner]").exists()).toBe(false);
        wrapper.unmount();
    });

    it("banner_OwnDraft_IsNotShown", async () => {
        const { wrapper } = await setup({ draftModifiedBy: "Me", draftModifiedByUserId: "me" });

        expect(wrapper.find("[data-cy=draft-banner]").exists()).toBe(false);
        wrapper.unmount();
    });

    it("beforeUnload_UnsavedChanges_AsksBeforeLeaving", async () => {
        const { store, wrapper, vm } = await setup();
        const leave = () => {
            const event = new Event("beforeunload", { cancelable: true });
            window.dispatchEvent(event);

            return event.defaultPrevented;
        };

        expect(leave()).toBe(false);

        await move(store);
        expect(leave()).toBe(true);

        await vm.autosave.save();
        await flushPromises();
        expect(leave()).toBe(false);
        wrapper.unmount();
    });
});
