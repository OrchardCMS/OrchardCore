import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import ActivityPanel from "../ActivityPanel.vue";
import { createDesignerStore } from "../../state/designerStore";
import { transitionKey } from "../../state/commands";
import { clearSelection, selectNode } from "../../canvas/useConnect";
import { clearToasts, useToasts } from "../../ui/toasts";
import type { DesignerApi } from "../../api/designerApi";
import { createDefinition, createNode } from "../../canvas/__tests__/fixtures";

const editor = (id: string) => ({ valid: true, content: `<input name="Task.Value" value="${id}" />`, scripts: "", styles: "" });

const setup = (postEditor: (...args: unknown[]) => Promise<unknown> = () => Promise.resolve({ valid: true })) => {
    const store = createDesignerStore();
    store.loadDefinition(createDefinition());
    const api = {
        getEditor: vi.fn((id: string) => Promise.resolve(editor(id))),
        postEditor: vi.fn(postEditor),
    } as unknown as DesignerApi & Record<"getEditor" | "postEditor", ReturnType<typeof vi.fn>>;
    const wrapper = mount(ActivityPanel, { props: { store, api }, attachTo: document.body });

    return { store, api, wrapper };
};

const edit = async (wrapper: ReturnType<typeof setup>["wrapper"]) => {
    const input = wrapper.get("input[name='Task.Value']");
    (input.element as HTMLInputElement).value = "changed";
    await input.trigger("input");
};

describe("ActivityPanel", () => {
    afterEach(() => {
        clearToasts();
        vi.restoreAllMocks();
        document.body.innerHTML = "";
    });

    it("selection_SingleActivity_OpensThePanelOnItsEditor", async () => {
        const { store, api, wrapper } = setup();
        const panel = wrapper.get("[data-cy=activity-panel]");

        expect(panel.isVisible()).toBe(false);

        selectNode(store, "a");
        await flushPromises();

        expect(panel.isVisible()).toBe(true);
        expect(panel.classes()).not.toContain("is-pinned");
        expect(api.getEditor).toHaveBeenCalledWith("a");
        expect(wrapper.get("[data-cy=panel-activity-title]").text()).toContain("Title a");
        expect((wrapper.get("input[name='Task.Value']").element as HTMLInputElement).value).toBe("a");
    });

    it("selection_Cleared_ClosesThePanel", async () => {
        const { store, wrapper } = setup();
        selectNode(store, "a");
        await flushPromises();

        clearSelection(store);
        await flushPromises();

        expect(wrapper.get("[data-cy=activity-panel]").isVisible()).toBe(false);
    });

    it("selection_SeveralActivities_ClosesThePanel", async () => {
        const { store, wrapper } = setup();
        selectNode(store, "a");
        await flushPromises();

        selectNode(store, "b", true);
        await flushPromises();

        expect(wrapper.get("[data-cy=activity-panel]").isVisible()).toBe(false);
    });

    it("pin_Toggle_KeepsThePanelOpenBelowTheCanvasAndIsRemembered", async () => {
        const { store, api, wrapper } = setup();
        selectNode(store, "a");
        await flushPromises();
        const panel = wrapper.get("[data-cy=activity-panel]");

        await wrapper.get("[data-cy=activity-panel-pin]").trigger("click");

        expect(panel.classes()).toContain("is-pinned");
        expect(wrapper.get("[data-cy=activity-panel-pin]").attributes("aria-pressed")).toBe("true");
        // Pinned, it can't be closed; it is unpinned instead.
        expect(wrapper.find("[data-cy=activity-panel-close]").exists()).toBe(false);

        clearSelection(store);
        await flushPromises();

        expect(panel.isVisible()).toBe(true);
        expect(wrapper.find("[data-cy=panel-empty]").exists()).toBe(true);

        selectNode(store, "a");
        selectNode(store, "b", true);
        await flushPromises();

        expect(wrapper.find("[data-cy=panel-multiple]").exists()).toBe(true);

        // Another designer in this browser starts pinned.
        const other = mount(ActivityPanel, { props: { store, api } });
        expect(other.get("[data-cy=activity-panel]").classes()).toContain("is-pinned");
        other.unmount();
    });

    it("close_Clicked_AppliesTheChangesAndClearsTheSelection", async () => {
        const { store, api, wrapper } = setup((id) =>
            Promise.resolve({ valid: true, revision: 3, node: createNode(id as string, { x: 600 }), removedTransitions: [], issues: [] }),
        );
        selectNode(store, "a");
        await flushPromises();
        await edit(wrapper);

        await wrapper.get("[data-cy=activity-panel-close]").trigger("click");
        await flushPromises();

        expect(api.postEditor).toHaveBeenCalledWith("a", 0, expect.any(FormData));
        expect(store.state.selectedNodeIds).toEqual([]);
        expect(wrapper.get("[data-cy=activity-panel]").isVisible()).toBe(false);
        expect(wrapper.emitted("closed")).toHaveLength(1);
    });

    it("tabs_ActivityWithOutputs_ShowTheSettingsTheOutputsAndTheAvailableData", async () => {
        const { store, wrapper } = setup();
        store.state.variables = [{ name: "greeting", typeName: "string" }];
        store.getNode("a")!.outputs = [{ name: "Result", typeName: "any", displayName: "Result" }];
        selectNode(store, "a");
        await flushPromises();

        expect(wrapper.findAll("[role=tab]").map((tab) => tab.attributes("data-cy"))).toEqual(["activity-tab-settings", "activity-tab-outputs", "activity-tab-data"]);

        const outputs = wrapper.get("[data-cy=activity-outputs]");
        expect(outputs.isVisible()).toBe(false);

        await wrapper.get("[data-cy=activity-tab-outputs]").trigger("click");

        expect(outputs.isVisible()).toBe(true);
        expect(outputs.text()).toContain("Result");

        // An activity without outputs has no Outputs tab.
        selectNode(store, "b");
        await flushPromises();

        expect(wrapper.findAll("[role=tab]").map((tab) => tab.attributes("data-cy"))).toEqual(["activity-tab-settings", "activity-tab-data"]);
    });

    it("tabs_Switch_ShowTheSettingsOrTheAvailableData", async () => {
        const { store, wrapper } = setup();
        selectNode(store, "a");
        await flushPromises();

        const settings = wrapper.get("[data-cy=panel-activity-form]");
        const data = wrapper.get("[data-cy=available-data]");
        expect(wrapper.get("[data-cy=activity-tab-settings]").attributes("aria-selected")).toBe("true");
        expect(settings.isVisible()).toBe(true);
        expect(data.isVisible()).toBe(false);

        await wrapper.get("[data-cy=activity-tab-data]").trigger("click");

        expect(wrapper.get("[data-cy=activity-tab-data]").attributes("aria-selected")).toBe("true");
        expect(settings.isVisible()).toBe(false);
        expect(data.isVisible()).toBe(true);

        // Another activity opens on its settings.
        selectNode(store, "b");
        await flushPromises();

        expect(wrapper.get("[data-cy=activity-tab-settings]").attributes("aria-selected")).toBe("true");
    });

    it("tabs_ArrowKeys_MoveTheSelectionAndTheFocus", async () => {
        const { store, wrapper } = setup();
        selectNode(store, "a");
        await flushPromises();

        await wrapper.get("[data-cy=activity-tab-settings]").trigger("keydown", { key: "ArrowRight" });
        await flushPromises();

        expect(wrapper.get("[data-cy=activity-tab-data]").attributes("aria-selected")).toBe("true");
        expect(document.activeElement).toBe(wrapper.get("[data-cy=activity-tab-data]").element);

        // The arrows wrap around.
        await wrapper.get("[data-cy=activity-tab-data]").trigger("keydown", { key: "ArrowRight" });
        await flushPromises();
        expect(document.activeElement).toBe(wrapper.get("[data-cy=activity-tab-settings]").element);
    });

    it("insert_WithAFieldFocused_InsertsInItAndShowsTheSettings", async () => {
        const { store, wrapper } = setup();
        selectNode(store, "a");
        await flushPromises();

        const input = wrapper.get("input[name='Task.Value']");
        (input.element as HTMLInputElement).setSelectionRange(1, 1);
        await input.trigger("focusin");
        await wrapper.get("[data-cy=activity-tab-data]").trigger("click");

        await wrapper.get("[data-cy='available-workflow-LastResult'] [data-cy=available-javascript]").trigger("click");
        await flushPromises();

        expect((input.element as HTMLInputElement).value).toBe("alastResult()");
        expect(wrapper.get("[data-cy=activity-tab-settings]").attributes("aria-selected")).toBe("true");
    });

    it("selectionChange_InvalidEditorAndUserKeepsEditing_KeepsTheSelection", async () => {
        const { store, api, wrapper } = setup(() => Promise.resolve({ valid: false, content: '<span class="field-validation-error">Required</span>', scripts: "", styles: "" }));
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

    it("selectionCleared_InvalidEditorAndUserKeepsEditing_KeepsThePanelOpen", async () => {
        const { store, wrapper } = setup(() => Promise.resolve({ valid: false, content: '<span class="field-validation-error">Required</span>', scripts: "", styles: "" }));
        vi.spyOn(window, "confirm").mockReturnValue(false);
        selectNode(store, "a");
        await flushPromises();
        await edit(wrapper);

        clearSelection(store);
        await flushPromises();

        expect(store.state.selectedNodeIds).toEqual(["a"]);
        expect(wrapper.get("[data-cy=activity-panel]").isVisible()).toBe(true);
    });

    it("selectionChange_InvalidEditorAndUserDiscards_MovesToTheNewActivity", async () => {
        const { store, api, wrapper } = setup(() => Promise.resolve({ valid: false, content: '<span class="field-validation-error">Required</span>', scripts: "", styles: "" }));
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
            <T>(task: (revision: number) => Promise<T>) =>
                new Promise<T>((resolve) => {
                    release = () => resolve(task(7));
                }),
        );
        const wrapper = mount(ActivityPanel, { props: { store, api, mutate }, attachTo: document.body });
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

    it("open_WithFocus_FocusesTheFirstFieldOnceLoaded", async () => {
        const { wrapper } = setup();

        await (wrapper.vm as unknown as { open: (id: string, options: { focus: boolean }) => Promise<void> }).open("a", { focus: true });
        await flushPromises();

        expect(document.activeElement).toBe(wrapper.get("input[name='Task.Value']").element);
    });

    it("escape_InTheEditor_ReturnsTheFocusToTheActivity", async () => {
        const { store, wrapper } = setup();
        selectNode(store, "a");
        await flushPromises();

        await wrapper.get("input[name='Task.Value']").trigger("keydown", { key: "Escape" });

        expect(wrapper.emitted("return-focus")).toEqual([["a"]]);
    });

    it("resizer_ArrowKeys_ResizeThePanelWithinTheDesignerAndAreRemembered", async () => {
        const { store, api, wrapper } = setup();
        Object.defineProperty(wrapper.element.parentElement!, "clientHeight", { configurable: true, value: 600 });
        selectNode(store, "a");
        await flushPromises();
        const resizer = wrapper.get("[data-cy=activity-panel-resizer]");

        await resizer.trigger("keydown", { key: "ArrowUp", shiftKey: true });
        await resizer.trigger("keydown", { key: "ArrowUp", shiftKey: true });
        await resizer.trigger("keydown", { key: "ArrowUp", shiftKey: true });

        // jsdom has no layout, so the panel grows from its minimum height (128 pixels).
        expect(wrapper.get("[data-cy=activity-panel]").attributes("style")).toContain("height: 256px");

        // The canvas keeps some room.
        for (let index = 0; index < 10; index++) {
            await resizer.trigger("keydown", { key: "ArrowUp", shiftKey: true });
        }

        expect(wrapper.get("[data-cy=activity-panel]").attributes("style")).toContain("height: 504px");

        // Another designer in this browser has that height.
        const other = mount(ActivityPanel, { props: { store, api } });
        await flushPromises();
        expect(other.get("[data-cy=activity-panel]").attributes("style")).toContain("height: 504px");
        other.unmount();
    });
});
