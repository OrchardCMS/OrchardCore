import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import App from "../App.vue";
import { createDesignerStore } from "../state/designerStore";
import type { DesignerApi } from "../api/designerApi";
import type { DesignerDefinition, DesignerVersion, DesignerVersions } from "../api/types";
import type { DesignerConfig } from "../config";
import { loadTranslations } from "../i18n";
import { clearToasts, useToasts } from "../ui/toasts";
import { createDefinition } from "../canvas/__tests__/fixtures";

const version = (number: number, overrides: Partial<DesignerVersion> = {}): DesignerVersion => ({
    versionId: `v${number}`,
    version: number,
    name: "Test",
    createdUtc: "2026-10-06T09:00:00Z",
    createdBy: "admin",
    isPublished: false,
    ...overrides,
});

const designerConfig: DesignerConfig = {
    workflowTypeId: 1,
    mode: "designer",
    readOnly: false,
    urls: { definition: "", library: "", save: "", addActivity: "", editor: "", settings: "", publish: "", discard: "", versions: "/versions", restore: "/restore" },
    designerUrl: "/designer",
    versionPageUrl: "/version",
    comparePageUrl: "/compare",
    instancesUrl: "",
    exportUrl: "",
    listUrl: "",
    currentUserId: "me",
    translations: {},
};

const versions: DesignerVersions = {
    versions: [version(2, { isPublished: true, instanceCount: 0 }), version(1, { instanceCount: 3 })],
    draft: { revision: 2, modifiedUtc: "2026-10-06T10:00:00Z", modifiedBy: "admin" },
};

const setup = async (definition: Partial<DesignerDefinition> = {}, config: Partial<DesignerConfig> = {}) => {
    const loaded: DesignerDefinition = { ...createDefinition(), hasDraft: true, revision: 2, publishedVersion: version(2, { isPublished: true }), ...definition };
    const api = {
        getDefinition: vi.fn(() => Promise.resolve(structuredClone(loaded))),
        getLibrary: vi.fn().mockResolvedValue({ categories: [] }),
        save: vi.fn((payload: { revision: number }) => Promise.resolve({ revision: payload.revision + 1, issues: [] })),
        publish: vi.fn().mockResolvedValue({ issues: [], publishedUtc: "2026-10-06T11:00:00Z", version: version(3, { isPublished: true }) }),
        getVersions: vi.fn().mockResolvedValue(versions),
        restore: vi.fn((_: string, revision: number) => Promise.resolve({ revision: revision + 1, issues: [] })),
    };
    const store = createDesignerStore();
    const wrapper = mount(App, {
        props: { config: { ...designerConfig, ...config }, api: api as unknown as DesignerApi, store },
        attachTo: document.body,
        global: { stubs: { teleport: true } },
    });
    await flushPromises();

    return { api, store, wrapper };
};

describe("versions", () => {
    beforeEach(() => {
        loadTranslations({
            VersionNumber: "Version {0}",
            PublishedVersion: "Published as version {0}",
            VersionRestored: "Version {0} restored",
            RunsOnVersion: "Runs on version {0}",
            InstanceCount: "{0} instance(s)",
        });
    });

    afterEach(() => {
        clearToasts();
        vi.restoreAllMocks();
        loadTranslations({});
        document.body.innerHTML = "";
    });

    it("toolbar_PublishedVersion_IsShown", async () => {
        const { wrapper } = await setup();

        expect(wrapper.get("[data-cy=published-version]").text()).toBe("Version 2");
        expect(wrapper.find("[data-cy=toolbar-versions]").exists()).toBe(true);
        wrapper.unmount();
    });

    it("versionsDialog_Open_ListsTheVersionsWithTheirLinks", async () => {
        const { api, wrapper } = await setup();

        await wrapper.get("[data-cy=toolbar-versions]").trigger("click");
        await flushPromises();

        expect(api.getVersions).toHaveBeenCalledTimes(1);
        const published = wrapper.get("[data-cy=version-2]");
        expect(published.find("[data-cy=version-published]").exists()).toBe(true);
        expect(published.find("[data-cy=version-restore]").exists()).toBe(false);
        expect(published.get("[data-cy=version-compare]").attributes("href")).toBe("/compare?from=v2&to=draft");

        const first = wrapper.get("[data-cy=version-1]");
        expect(first.text()).toContain("3 instance(s)");
        expect(first.get("[data-cy=version-view]").attributes("href")).toBe("/version?versionId=v1");
        expect(first.get("[data-cy=version-compare]").attributes("href")).toBe("/compare?from=v1&to=draft");
        expect(wrapper.get("[data-cy=versions-draft-compare]").attributes("href")).toBe("/compare?from=v2&to=draft");
        wrapper.unmount();
    });

    it("versionsDialog_Restore_CopiesTheVersionIntoTheDraftAndReloads", async () => {
        vi.spyOn(window, "confirm").mockReturnValue(true);
        const { api, wrapper } = await setup();

        await wrapper.get("[data-cy=toolbar-versions]").trigger("click");
        await flushPromises();
        await wrapper.get("[data-cy=version-1] [data-cy=version-restore]").trigger("click");
        await flushPromises();

        expect(window.confirm).toHaveBeenCalledTimes(1);
        expect(api.restore).toHaveBeenCalledWith("v1", 2);
        expect(api.getDefinition).toHaveBeenCalledTimes(2);
        expect(wrapper.find("[data-cy=versions-dialog]").exists()).toBe(false);
        expect(useToasts().at(-1)?.message).toBe("Version 1 restored");
        wrapper.unmount();
    });

    it("versionsDialog_RestoreCancelled_KeepsTheDraft", async () => {
        vi.spyOn(window, "confirm").mockReturnValue(false);
        const { api, wrapper } = await setup();

        await wrapper.get("[data-cy=toolbar-versions]").trigger("click");
        await flushPromises();
        await wrapper.get("[data-cy=version-1] [data-cy=version-restore]").trigger("click");
        await flushPromises();

        expect(api.restore).not.toHaveBeenCalled();
        expect(wrapper.find("[data-cy=versions-dialog]").exists()).toBe(true);
        wrapper.unmount();
    });

    it("publish_NewVersion_IsShownInTheToolbarAndTheToast", async () => {
        const { api, wrapper } = await setup();

        await wrapper.get("[data-cy=toolbar-publish]").trigger("click");
        await flushPromises();

        expect(api.publish).toHaveBeenCalledWith(2);
        expect(wrapper.get("[data-cy=published-version]").text()).toBe("Version 3");
        expect(useToasts().at(-1)?.message).toBe("Published as version 3");
        wrapper.unmount();
    });

    it("versionPage_EarlierVersion_ShowsItAndLinksToTheComparison", async () => {
        const { wrapper } = await setup(
            { hasDraft: false, revision: 0, version: version(1), publishedVersion: version(2, { isPublished: true }) },
            { mode: "version", readOnly: true, urls: { ...designerConfig.urls, versions: null, restore: null } },
        );

        expect(wrapper.get("[data-cy=page-version]").text()).toBe("Version 1");
        expect(wrapper.find("[data-cy=page-version-published]").exists()).toBe(false);
        expect(wrapper.get("[data-cy=page-version-compare]").attributes("href")).toBe("/compare?from=v1&to=v2");
        expect(wrapper.get("[data-cy=page-version-designer]").attributes("href")).toBe("/designer");
        expect(wrapper.find("[data-cy=toolbar-publish]").exists()).toBe(false);
        expect(wrapper.find("[data-cy=published-version]").exists()).toBe(false);
        wrapper.unmount();
    });

    it("instanceViewer_InstanceOnAnEarlierVersion_SaysSo", async () => {
        const { wrapper } = await setup(
            {
                hasDraft: false,
                revision: 0,
                version: version(1),
                instance: { id: 12, workflowId: "wf-12", status: "Halted", blockingActivityIds: ["a"] },
            },
            { mode: "instance", readOnly: true },
        );

        expect(wrapper.get("[data-cy=instance-version]").text()).toContain("Runs on version 1");
        expect(wrapper.find("[data-cy=instance-version-not-published]").exists()).toBe(true);
        wrapper.unmount();
    });
});
