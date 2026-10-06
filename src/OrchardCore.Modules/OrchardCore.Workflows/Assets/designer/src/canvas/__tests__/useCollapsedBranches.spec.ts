import { describe, expect, it } from "vitest";
import { effectScope } from "vue";
import { createDesignerStore } from "../../state/designerStore";
import { collapsedPreferenceKey, useCollapsedBranches } from "../useCollapsedBranches";
import { createDefinition } from "./fixtures";

const STORAGE_KEY = `orchardcore:workflows-designer:${collapsedPreferenceKey("type-1")}`;

const setup = () => {
    const store = createDesignerStore();
    store.loadDefinition(createDefinition());
    const branches = effectScope().run(() => useCollapsedBranches(store))!;

    return { store, branches };
};

describe("useCollapsedBranches", () => {
    it("collapse_SelectedActivityAfterIt_DeselectsItAndRemembersPerWorkflowType", () => {
        const { store, branches } = setup();
        store.state.selectedNodeIds = ["start", "a"];
        store.state.selectedTransitionKey = "fork:A:a";

        branches.collapse("fork");

        expect([...branches.hidden.value].sort()).toEqual(["a", "b"]);
        expect(branches.hiddenCounts.value.get("fork")).toBe(2);
        expect(store.state.selectedNodeIds).toEqual(["start"]);
        expect(store.state.selectedTransitionKey).toBeNull();
        expect(window.localStorage.getItem(STORAGE_KEY)).toBe('["fork"]');
    });

    it("load_StoredPreference_RestoresAndDropsDeletedActivities", () => {
        window.localStorage.setItem(STORAGE_KEY, '["fork","deleted"]');
        const { branches } = setup();

        expect([...branches.collapsed.value]).toEqual(["fork"]);
        expect(branches.hidden.value.size).toBe(2);

        branches.expand("fork");

        expect(branches.hidden.value.size).toBe(0);
        expect(window.localStorage.getItem(STORAGE_KEY)).toBe("[]");
    });

    it("load_InvalidPreference_StartsExpanded", () => {
        window.localStorage.setItem(STORAGE_KEY, "{not json");
        const { branches } = setup();

        expect(branches.collapsed.value.size).toBe(0);
    });

    it("countHiddenBy_ActivityWithNothingAfterIt_ReturnsZero", () => {
        const { branches } = setup();

        expect(branches.countHiddenBy("fork")).toBe(2);
        expect(branches.countHiddenBy("a")).toBe(0);
        expect(branches.countHiddenBy("start")).toBe(3);
    });

    it("reveal_HiddenActivity_ExpandsTheBranch", () => {
        const { branches } = setup();
        branches.collapse("start");
        branches.collapse("fork");

        expect(branches.reveal(["fork"])).toBe(true);
        expect([...branches.collapsed.value]).toEqual(["fork"]);
        expect(branches.reveal(["fork"])).toBe(false);

        branches.expandAll();
        expect(branches.hidden.value.size).toBe(0);
    });
});
