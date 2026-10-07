import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import App from "../App.vue";
import { createDesignerStore } from "../state/designerStore";
import type { DesignerApi } from "../api/designerApi";
import type { DesignerConfig } from "../config";
import { createDefinition } from "../canvas/__tests__/fixtures";
import { createFakeConnection } from "../realtime/__tests__/fakeConnection";

const config: DesignerConfig = {
    workflowTypeId: 1,
    readOnly: false,
    mode: "designer",
    urls: { definition: "", library: "", save: "", addActivity: "", editor: "", settings: "", publish: "", discard: "" },
    instancesUrl: "",
    exportUrl: "",
    listUrl: "",
    currentUserId: "me",
    hubUrl: "/tenant/hubs/workflows",
    translations: {},
};

// The SignalR client the page loads, with a fake connection.
const installSignalR = () => {
    const fake = createFakeConnection();
    const urls: string[] = [];

    window.signalR = {
        HubConnectionBuilder: class {
            withUrl(url: string) {
                urls.push(url);

                return this;
            }

            withAutomaticReconnect() {
                return this;
            }

            build() {
                return fake.connection;
            }
        },
    };

    return { ...fake, urls };
};

describe("live updates", () => {
    afterEach(() => {
        delete window.signalR;
        document.body.innerHTML = "";
    });

    it("designer_OthersChanges_ShowPresenceAndAReloadNotice", async () => {
        const hub = installSignalR();
        const api = {
            getDefinition: vi.fn().mockResolvedValue(createDefinition()),
            getLibrary: vi.fn().mockResolvedValue({ categories: [] }),
        } as unknown as DesignerApi & { getDefinition: ReturnType<typeof vi.fn> };
        const wrapper = mount(App, { props: { config, api, store: createDesignerStore() }, attachTo: document.body });
        await flushPromises();

        expect(hub.urls).toEqual(["/tenant/hubs/workflows"]);
        expect(hub.connection.invoke).toHaveBeenCalledWith("SubscribeWorkflowType", "type-1");

        hub.raise("PresenceJoined", { connectionId: "c2", userId: "bob", userName: "bob.smith" });
        await flushPromises();

        expect(wrapper.get("[data-cy=presence-avatar]").text()).toBe("BS");

        // The user's own changes come back too, and don't show the notice.
        hub.raise("WorkflowTypeChanged", { kind: "DraftChanged", workflowTypeId: "type-1", revision: 4, userId: "me", userName: "me" });
        await flushPromises();

        expect(wrapper.find("[data-cy=remote-change]").exists()).toBe(false);

        hub.raise("WorkflowTypeChanged", { kind: "DraftChanged", workflowTypeId: "type-1", revision: 4, userId: "bob", userName: "bob.smith" });
        await flushPromises();

        expect(wrapper.get("[data-cy=remote-change]").text()).toContain("RemoteDraftChanged");

        await wrapper.get("[data-cy=remote-change-reload]").trigger("click");
        await flushPromises();

        expect(api.getDefinition).toHaveBeenCalledTimes(2);
        expect(wrapper.find("[data-cy=remote-change]").exists()).toBe(false);
        wrapper.unmount();

        expect(hub.connection.stop).toHaveBeenCalled();
    });

    it("viewer_InstanceChanged_ReloadsTheInstance", async () => {
        const hub = installSignalR();
        const instance = { id: 12, workflowId: "wf-12", status: "Halted", blockingActivityIds: ["a"] };
        const api = {
            getDefinition: vi
                .fn()
                .mockResolvedValueOnce({ ...createDefinition(), instance })
                .mockResolvedValueOnce({ ...createDefinition(), instance: { ...instance, status: "Finished", blockingActivityIds: [] } }),
        } as unknown as DesignerApi & { getDefinition: ReturnType<typeof vi.fn> };
        const store = createDesignerStore();
        const wrapper = mount(App, { props: { config: { ...config, mode: "instance", readOnly: true }, api, store }, attachTo: document.body });
        await flushPromises();

        expect(hub.connection.invoke).toHaveBeenCalledWith("SubscribeInstance", "wf-12");

        hub.raise("InstanceChanged", { workflowId: "wf-12", workflowTypeId: "type-1", status: "Finished", isDeleted: false });
        await flushPromises();

        expect(store.state.instance?.status).toBe("Finished");
        wrapper.unmount();
    });

    it("designer_WithoutHub_DoesNotConnect", async () => {
        const hub = installSignalR();
        const api = {
            getDefinition: vi.fn().mockResolvedValue(createDefinition()),
            getLibrary: vi.fn().mockResolvedValue({ categories: [] }),
        } as unknown as DesignerApi;
        const wrapper = mount(App, { props: { config: { ...config, hubUrl: null }, api, store: createDesignerStore() } });
        await flushPromises();

        expect(hub.urls).toEqual([]);
        expect(wrapper.find("[data-cy=presence]").exists()).toBe(false);
        wrapper.unmount();
    });
});
