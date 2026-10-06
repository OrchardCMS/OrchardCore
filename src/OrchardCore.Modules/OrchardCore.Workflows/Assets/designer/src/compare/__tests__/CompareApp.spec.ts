import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import CompareApp from "../CompareApp.vue";
import type { DesignerApi } from "../../api/designerApi";
import type { DesignerComparison, WorkflowTypeChanges } from "../../api/types";
import type { DesignerConfig } from "../../config";
import { createDefinition, createNode } from "../../canvas/__tests__/fixtures";

const config: DesignerConfig = {
    workflowTypeId: 1,
    mode: "compare",
    readOnly: true,
    urls: { definition: "", library: "", save: "", addActivity: "", editor: "", settings: "", publish: "", discard: "", compare: "/compare-api" },
    designerUrl: "/designer",
    instancesUrl: "",
    exportUrl: "",
    listUrl: "",
    translations: {},
};

const noChanges: WorkflowTypeChanges = {
    addedActivityIds: [],
    removedActivityIds: [],
    changedActivityIds: [],
    movedActivityIds: [],
    addedTransitionKeys: [],
    removedTransitionKeys: [],
    changedSettings: [],
    hasChanges: false,
};

// Version 1 is the fixture; the draft drops "b", adds "c" after "a" and changes "a".
const comparison = (): DesignerComparison => {
    const from = { ...createDefinition(), version: { versionId: "v1", version: 1, name: "Test", createdUtc: "2026-10-06T09:00:00Z", isPublished: true } };
    const to = { ...createDefinition(), hasDraft: true };
    to.nodes = [...to.nodes.filter((node) => node.id !== "b"), createNode("c", { x: 900 })];
    to.transitions = [...to.transitions.filter((transition) => transition.destinationActivityId !== "b"), { sourceActivityId: "a", sourceOutcomeName: "Done", destinationActivityId: "c" }];

    return {
        from,
        to,
        changes: {
            ...noChanges,
            addedActivityIds: ["c"],
            removedActivityIds: ["b"],
            changedActivityIds: ["a"],
            addedTransitionKeys: ["a:Done:c"],
            removedTransitionKeys: ["fork:B:b"],
            hasChanges: true,
        },
    };
};

const setup = async (value: DesignerComparison) => {
    const api = { getComparison: vi.fn().mockResolvedValue(value) };
    const wrapper = mount(CompareApp, { props: { config, api: api as unknown as DesignerApi }, attachTo: document.body });
    await flushPromises();

    return { api, wrapper };
};

describe("CompareApp", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("mount_Comparison_HighlightsTheChangesOnBothSides", async () => {
        const { api, wrapper } = await setup(comparison());

        expect(api.getComparison).toHaveBeenCalledTimes(1);

        const from = wrapper.get("[data-cy=compare-from]");
        expect(from.get("[data-cy=activity-b]").classes()).toContain("is-removed");
        expect(from.get("[data-cy=activity-a]").classes()).toContain("is-changed");
        expect(from.get("[data-cy='edge-fork:B:b']").classes()).toContain("is-removed");
        expect(from.get("[data-cy=activity-start]").classes()).not.toContain("is-changed");

        const to = wrapper.get("[data-cy=compare-to]");
        expect(to.get("[data-cy=activity-c]").classes()).toContain("is-added");
        expect(to.get("[data-cy=activity-a] [data-cy=change-badge]").text()).toBe("ChangeChanged");
        expect(to.get("[data-cy='edge-a:Done:c']").classes()).toContain("is-added");
        expect(to.find("[data-cy=activity-b]").exists()).toBe(false);

        // The draft is on the right, version 1 (published) on the left.
        expect(from.get("h3").text()).toBe("VersionPublished");
        expect(to.get("h3").text()).toBe("Draft");
        wrapper.unmount();
    });

    it("mount_Comparison_ListsTheChangesByTitle", async () => {
        const { wrapper } = await setup(comparison());

        const changes = wrapper.get("[data-cy=compare-changes]");
        expect(changes.find("[data-cy=compare-group-added]").exists()).toBe(true);
        expect(changes.text()).toContain("Title c");
        expect(changes.text()).toContain("Title b");
        expect(changes.text()).toContain("Title a (Done) → Title c");
        expect(changes.find("[data-cy=compare-group-moved]").exists()).toBe(false);
        wrapper.unmount();
    });

    it("mount_SameDefinitions_SaysSo", async () => {
        const { wrapper } = await setup({ from: createDefinition(), to: createDefinition(), changes: noChanges });

        expect(wrapper.find("[data-cy=compare-no-changes]").exists()).toBe(true);
        wrapper.unmount();
    });

    it("mount_LoadFailure_ShowsAnError", async () => {
        const api = { getComparison: vi.fn().mockRejectedValue(new Error("offline")) };
        const wrapper = mount(CompareApp, { props: { config, api: api as unknown as DesignerApi }, attachTo: document.body });
        await flushPromises();

        expect(wrapper.get("[role=alert]").text()).toBe("CompareLoadFailed");
        wrapper.unmount();
    });
});
