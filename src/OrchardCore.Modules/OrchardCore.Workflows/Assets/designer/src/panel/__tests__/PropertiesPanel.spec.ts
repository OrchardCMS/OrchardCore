import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import { nextTick } from "vue";
import PropertiesPanel from "../PropertiesPanel.vue";
import { createDesignerStore } from "../../state/designerStore";
import { transitionKey } from "../../state/commands";
import { selectNode } from "../../canvas/useConnect";
import { clearToasts, useToasts } from "../../ui/toasts";
import type { DesignerApi } from "../../api/designerApi";
import { createDefinition, createNode } from "../../canvas/__tests__/fixtures";

const editor = (id: string) => ({ valid: true, content: `<input name="Task.Value" value="${id}" />`, scripts: "", styles: "" });

const setup = (postEditor: (...args: unknown[]) => Promise<unknown>) => {
    const store = createDesignerStore();
    store.loadDefinition(createDefinition());
    const api = {
        getEditor: vi.fn((id: string) => Promise.resolve(editor(id))),
        postEditor: vi.fn(postEditor),
        getSettings: vi.fn(() => Promise.resolve({ valid: true, content: '<input name="Name" value="Test" />', scripts: "", styles: "" })),
        postSettings: vi.fn(() => Promise.resolve({ valid: true, revision: 5, settings: { ...createDefinition().settings, name: "Renamed" }, issues: [] })),
    } as unknown as DesignerApi & Record<"getEditor" | "postEditor" | "getSettings" | "postSettings", ReturnType<typeof vi.fn>>;
    const wrapper = mount(PropertiesPanel, { props: { store, api }, attachTo: document.body });

    return { store, api, wrapper };
};

const edit = async (wrapper: ReturnType<typeof setup>["wrapper"]) => {
    const input = wrapper.get("input[name='Task.Value']");
    (input.element as HTMLInputElement).value = "changed";
    await input.trigger("input");
};

describe("PropertiesPanel", () => {
    afterEach(() => {
        clearToasts();
        vi.restoreAllMocks();
        document.body.innerHTML = "";
    });

    it("variables_Declared_FillTheDatalistTheVariablesTabAndTheOutputs", async () => {
        const { store, wrapper } = setup(() => Promise.resolve({ valid: true }));
        store.state.variables = [{ name: "greeting", typeName: "string" }];
        store.getNode("a")!.outputs = [{ name: "Result", typeName: "any", displayName: "Result" }];

        selectNode(store, "a");
        await flushPromises();

        expect(wrapper.findAll("#wfd-variables option").map((option) => option.attributes("value"))).toEqual(["greeting"]);
        expect(wrapper.get("[data-cy=activity-outputs]").text()).toContain("Result");
        expect(wrapper.findAll("[role=tab]").map((tab) => tab.attributes("data-cy"))).toEqual([
            "panel-tab-activity",
            "panel-tab-variables",
            "panel-tab-workflow",
            "panel-tab-issues",
        ]);

        await wrapper.get("[data-cy=panel-tab-variables]").trigger("click");
        await flushPromises();

        expect(wrapper.get("[data-cy=variables-tab] [data-cy=variable-name]").element).toHaveProperty("value", "greeting");
    });

    it("selection_SingleActivity_LoadsItsEditor", async () => {
        const { store, api, wrapper } = setup(() => Promise.resolve({ valid: true }));

        expect(wrapper.find("[data-cy=panel-empty]").exists()).toBe(true);

        selectNode(store, "a");
        await flushPromises();

        expect(api.getEditor).toHaveBeenCalledWith("a");
        expect(wrapper.get("[data-cy=panel-activity-title]").text()).toContain("Title a");
        expect((wrapper.get("input[name='Task.Value']").element as HTMLInputElement).value).toBe("a");
    });

    it("selectionChange_InvalidEditorAndUserKeepsEditing_KeepsTheSelection", async () => {
        const { store, api, wrapper } = setup(() =>
            Promise.resolve({ valid: false, content: '<span class="field-validation-error">Required</span>', scripts: "", styles: "" }),
        );
        vi.spyOn(window, "confirm").mockReturnValue(false);
        selectNode(store, "a");
        await flushPromises();
        await edit(wrapper);

        selectNode(store, "b");
        await flushPromises();

        expect(api.postEditor).toHaveBeenCalledTimes(1);
        expect(window.confirm).toHaveBeenCalledTimes(1);
        expect(store.state.selectedNodeIds).toEqual(["a"]);
        expect(api.getEditor).toHaveBeenCalledTimes(1);
        expect(wrapper.get(".field-validation-error").text()).toBe("Required");
    });

    it("selectionChange_InvalidEditorAndUserDiscards_MovesToTheNewActivity", async () => {
        const { store, api, wrapper } = setup(() =>
            Promise.resolve({ valid: false, content: '<span class="field-validation-error">Required</span>', scripts: "", styles: "" }),
        );
        vi.spyOn(window, "confirm").mockReturnValue(true);
        selectNode(store, "a");
        await flushPromises();
        await edit(wrapper);

        selectNode(store, "b");
        await flushPromises();

        expect(store.state.selectedNodeIds).toEqual(["b"]);
        expect(api.getEditor).toHaveBeenLastCalledWith("b");
    });

    it("selectionChange_ValidEdit_AppliesItAndUpdatesTheNode", async () => {
        const { store, api, wrapper } = setup((id) =>
            Promise.resolve({ valid: true, revision: 3, node: { ...createNode(id as string, { x: 999 }), title: "Applied" }, removedTransitions: [], issues: [] }),
        );
        selectNode(store, "a");
        await flushPromises();
        await edit(wrapper);

        selectNode(store, "b");
        await flushPromises();

        expect(api.postEditor).toHaveBeenCalledWith("a", 0, expect.any(FormData));
        expect(store.getNode("a")?.title).toBe("Applied");
        // The position stays the canvas's.
        expect(store.getNode("a")?.x).toBe(600);
        expect(store.state.revision).toBe(3);
        expect(store.state.selectedNodeIds).toEqual(["b"]);
    });

    it("apply_OutcomesRemoved_RemovesTheirTransitionsWithAToast", async () => {
        const { store, wrapper } = setup(() =>
            Promise.resolve({
                valid: true,
                revision: 1,
                node: createNode("fork", { x: 300, name: "ForkTask", displayText: "Fork" }, ["A"]),
                removedTransitions: [{ sourceActivityId: "fork", sourceOutcomeName: "B", destinationActivityId: "b" }],
                issues: [],
            }),
        );
        selectNode(store, "fork");
        await flushPromises();
        await edit(wrapper);

        await wrapper.get("form").trigger("submit");
        await flushPromises();

        expect(store.state.transitions.map(transitionKey)).toEqual(["start:Done:fork", "fork:A:a"]);
        expect(store.getNode("fork")?.outcomes.map((outcome) => outcome.name)).toEqual(["A"]);
        expect(useToasts().map((toast) => toast.variant)).toEqual(["warning"]);
    });

    it("apply_QueuedBehindAnotherRequest_PostsWithTheRevisionCurrentWhenItRuns", async () => {
        const store = createDesignerStore();
        store.loadDefinition(createDefinition());
        const api = {
            getEditor: vi.fn((id: string) => Promise.resolve(editor(id))),
            postEditor: vi.fn(() => Promise.resolve({ valid: true, revision: 8, node: createNode("a", { x: 600 }), removedTransitions: [], issues: [] })),
        } as unknown as DesignerApi & Record<"postEditor", ReturnType<typeof vi.fn>>;
        let release!: () => void;
        const mutate = vi.fn(
            <T,>(task: (revision: number) => Promise<T>) =>
                new Promise<T>((resolve) => {
                    release = () => resolve(task(7));
                }),
        );
        const wrapper = mount(PropertiesPanel, { props: { store, api, mutate }, attachTo: document.body });
        selectNode(store, "a");
        await flushPromises();
        await edit(wrapper);

        await wrapper.get("form").trigger("submit");
        await flushPromises();
        expect(mutate).toHaveBeenCalledTimes(1);
        expect(api.postEditor).not.toHaveBeenCalled();

        release();
        await flushPromises();

        expect(api.postEditor).toHaveBeenCalledWith("a", 7, expect.any(FormData));
        expect(store.state.revision).toBe(8);
    });

    it("workflowTab_SettingsApplied_UpdatesTheSettings", async () => {
        const { store, api, wrapper } = setup(() => Promise.resolve({ valid: true }));

        await wrapper.get("[data-cy=panel-tab-workflow]").trigger("click");
        await flushPromises();
        const input = wrapper.get("input[name='Name']");
        (input.element as HTMLInputElement).value = "Renamed";
        await input.trigger("input");
        await wrapper.get("form").trigger("submit");
        await flushPromises();

        expect(api.postSettings).toHaveBeenCalledWith(0, expect.any(FormData));
        expect(store.state.settings?.name).toBe("Renamed");
        expect(store.state.revision).toBe(5);
    });

    it("issuesTab_IssueClicked_SelectsAndFocusesTheActivity", async () => {
        const { store, wrapper } = setup(() => Promise.resolve({ valid: true }));
        store.state.issues = [
            { severity: "Warning", code: "UnreachableActivity", message: "Unreachable", activityId: "b" },
            { severity: "Error", code: "InvalidTransition", message: "Broken", activityId: "a" },
        ];

        await wrapper.get("[data-cy=panel-tab-issues]").trigger("click");
        await flushPromises();

        expect(wrapper.get("[data-cy=issues-count]").text()).toBe("2");
        // Errors are listed first.
        expect(wrapper.findAll(".wfd-issue").map((issue) => issue.attributes("data-cy"))).toEqual(["issue-InvalidTransition", "issue-UnreachableActivity"]);

        await wrapper.get("[data-cy=issue-UnreachableActivity]").trigger("click");

        expect(store.state.selectedNodeIds).toEqual(["b"]);
        expect(wrapper.emitted("focus-activity")).toEqual([["b"]]);
    });

    it("tabs_ArrowKeys_MoveTheSelectionAndTheFocus", async () => {
        const { wrapper } = setup(() => Promise.resolve({ valid: true }));
        const activityTab = wrapper.get("[data-cy=panel-tab-activity]");

        expect(activityTab.attributes("tabindex")).toBe("0");
        expect(wrapper.get("[data-cy=panel-tab-issues]").attributes("tabindex")).toBe("-1");
        expect(wrapper.get("[role=tabpanel]").attributes("aria-labelledby")).toBe(activityTab.attributes("id"));

        await activityTab.trigger("keydown", { key: "ArrowRight" });
        await flushPromises();

        expect(wrapper.get("[data-cy=panel-tab-variables]").attributes("aria-selected")).toBe("true");
        expect(document.activeElement).toBe(wrapper.get("[data-cy=panel-tab-variables]").element);

        await wrapper.get("[data-cy=panel-tab-variables]").trigger("keydown", { key: "End" });
        await flushPromises();
        expect(document.activeElement).toBe(wrapper.get("[data-cy=panel-tab-issues]").element);

        // The arrows wrap around.
        await wrapper.get("[data-cy=panel-tab-issues]").trigger("keydown", { key: "ArrowRight" });
        await flushPromises();
        expect(document.activeElement).toBe(wrapper.get("[data-cy=panel-tab-activity]").element);
    });

    it("open_WithFocus_FocusesTheFirstFieldOnceLoaded", async () => {
        const { wrapper } = setup(() => Promise.resolve({ valid: true }));

        await (wrapper.vm as unknown as { open: (id: string, options: { focus: boolean }) => Promise<void> }).open("a", { focus: true });
        await flushPromises();

        expect(document.activeElement).toBe(wrapper.get("input[name='Task.Value']").element);
    });

    it("escape_InTheEditor_ReturnsTheFocusToTheActivity", async () => {
        const { store, wrapper } = setup(() => Promise.resolve({ valid: true }));
        selectNode(store, "a");
        await flushPromises();

        await wrapper.get("input[name='Task.Value']").trigger("keydown", { key: "Escape" });

        expect(wrapper.emitted("return-focus")).toEqual([["a"]]);
    });

    it("collapse_Toggle_ShowsTheTabsOnARailAndIsRemembered", async () => {
        const { store, api, wrapper } = setup(() => Promise.resolve({ valid: true }));
        const panel = wrapper.get("[data-cy=designer-panel]");

        await wrapper.get("[data-cy=panel-collapse]").trigger("click");
        await flushPromises();

        expect(panel.classes()).toContain("is-collapsed");
        expect(wrapper.get("[data-cy=panel-sheet]").isVisible()).toBe(false);
        expect(wrapper.findAll("[data-cy^=panel-rail-]").map((button) => button.attributes("data-cy"))).toEqual([
            "panel-rail-activity",
            "panel-rail-variables",
            "panel-rail-workflow",
            "panel-rail-issues",
        ]);

        // Another designer in this browser starts collapsed.
        const other = mount(PropertiesPanel, { props: { store, api } });
        expect(other.get("[data-cy=designer-panel]").classes()).toContain("is-collapsed");
        other.unmount();

        // A tab of the rail expands the panel on that tab.
        await wrapper.get("[data-cy=panel-rail-workflow]").trigger("click");
        await flushPromises();

        expect(panel.classes()).not.toContain("is-collapsed");
        expect(wrapper.get("[data-cy=panel-tab-workflow]").attributes("aria-selected")).toBe("true");
    });

    it("rail_Hover_OpensThePanelOverTheCanvasUntilThePointerLeaves", async () => {
        const { store, wrapper } = setup(() => Promise.resolve({ valid: true }));
        store.state.issues = [{ severity: "Error", code: "InvalidTransition", message: "Broken", activityId: "a" }];
        await wrapper.get("[data-cy=panel-collapse]").trigger("click");
        await flushPromises();
        const panel = wrapper.get("[data-cy=designer-panel]");
        expect(wrapper.get("[data-cy=panel-rail-issues]").text()).toBe("1");

        vi.useFakeTimers();

        try {
            await panel.trigger("mouseenter");
            await wrapper.get("[data-cy=panel-rail-issues]").trigger("mouseenter");
            vi.advanceTimersByTime(150);
            await nextTick();

            expect(panel.classes()).toEqual(expect.arrayContaining(["is-collapsed", "is-peeking"]));
            expect(wrapper.get("[data-cy=panel-sheet]").isVisible()).toBe(true);
            expect(wrapper.get("[data-cy=panel-tab-issues]").attributes("aria-selected")).toBe("true");

            await panel.trigger("mouseleave");
            vi.advanceTimersByTime(300);
            await nextTick();

            expect(panel.classes()).not.toContain("is-peeking");
            expect(wrapper.get("[data-cy=panel-sheet]").isVisible()).toBe(false);
        } finally {
            vi.useRealTimers();
        }
    });
});
