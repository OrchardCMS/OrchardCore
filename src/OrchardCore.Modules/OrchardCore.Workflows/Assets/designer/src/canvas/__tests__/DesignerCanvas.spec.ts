import { afterEach, describe, expect, it } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import DesignerCanvas from "../DesignerCanvas.vue";
import { createDesignerStore } from "../../state/designerStore";
import { transitionKey } from "../../state/commands";
import { clearToasts, useToasts } from "../../ui/toasts";
import { createDefinition } from "./fixtures";

const setup = (readOnly = false) => {
    const store = createDesignerStore();
    store.loadDefinition(createDefinition());
    const wrapper = mount(DesignerCanvas, { props: { store, readOnly }, attachTo: document.body });

    return { store, wrapper, surface: wrapper.get("[data-cy=canvas-surface]") };
};

describe("DesignerCanvas", () => {
    afterEach(() => {
        clearToasts();
        document.body.innerHTML = "";
    });

    it("render_Definition_RendersEveryNodeAndEdge", () => {
        const { wrapper } = setup();

        expect(wrapper.findAll(".wfd-node")).toHaveLength(4);
        expect(wrapper.findAll(".wfd-edge")).toHaveLength(3);
        expect(wrapper.get("[data-cy=activity-fork]").attributes("data-x")).toBe("300");
        expect(wrapper.get("[data-cy='edge-fork:A:a']").exists()).toBe(true);
    });

    it("keyboard_DeleteSelectedNode_RemovesNodeAndEdgesAndOffersUndo", async () => {
        const { store, wrapper, surface } = setup();
        store.state.selectedNodeIds = ["fork"];

        await surface.trigger("keydown", { key: "Delete" });

        expect(wrapper.find("[data-cy=activity-fork]").exists()).toBe(false);
        expect(wrapper.findAll(".wfd-edge")).toHaveLength(0);

        const toasts = useToasts();
        expect(toasts).toHaveLength(1);
        toasts[0].action!.run();
        await flushPromises();

        expect(wrapper.find("[data-cy=activity-fork]").exists()).toBe(true);
        expect(store.state.transitions.map(transitionKey)).toEqual(["start:Done:fork", "fork:A:a", "fork:B:b"]);
    });

    it("keyboard_CtrlAAndArrows_SelectsAllAndNudges", async () => {
        const { store, surface } = setup();

        await surface.trigger("keydown", { key: "a", ctrlKey: true });
        await surface.trigger("keydown", { key: "ArrowRight" });
        await surface.trigger("keydown", { key: "ArrowDown", shiftKey: true });

        expect(store.state.selectedNodeIds).toHaveLength(4);
        expect(store.getNode("fork")).toMatchObject({ x: 310, y: 1 });
        expect(store.getNode("start")).toMatchObject({ x: 10, y: 1 });

        await surface.trigger("keydown", { key: "Escape" });
        expect(store.state.selectedNodeIds).toEqual([]);
    });

    it("keyboard_ReadOnly_DoesNotDeleteOrMove", async () => {
        const { store, surface } = setup(true);
        store.state.selectedNodeIds = ["fork"];

        await surface.trigger("keydown", { key: "Delete" });
        await surface.trigger("keydown", { key: "ArrowRight" });

        expect(store.state.nodes).toHaveLength(4);
        expect(store.getNode("fork")?.x).toBe(300);
    });

    it("contextMenu_ConnectOutcome_ReplacesExistingEdgeWithToast", async () => {
        const { store, wrapper } = setup();

        await wrapper.get("[data-cy=activity-fork]").trigger("keydown", { key: "ContextMenu" });
        expect(wrapper.find("[data-cy=context-menu]").exists()).toBe(true);
        expect(wrapper.find("[data-cy=menu-disconnect-A]").exists()).toBe(true);

        await wrapper.get("[data-cy=menu-connect]").trigger("click");
        await wrapper.get("[data-cy=connect-dialog-outcome]").setValue("A");
        await wrapper.get("[data-cy=connect-dialog-target]").setValue("b");
        await wrapper.get("[data-cy=connect-dialog] form").trigger("submit");

        expect(wrapper.find("[data-cy=connect-dialog]").exists()).toBe(false);
        expect(store.state.transitions.filter((transition) => transition.sourceActivityId === "fork" && transition.sourceOutcomeName === "A")).toEqual([
            { sourceActivityId: "fork", sourceOutcomeName: "A", destinationActivityId: "b" },
        ]);
        expect(useToasts()[0].variant).toBe("warning");
    });

    it("contextMenu_ConnectDialog_ExcludesSourceFromTargets", async () => {
        const { wrapper } = setup();

        await wrapper.get("[data-cy=port-fork-B]").trigger("keydown", { key: "Enter" });

        const options = wrapper.findAll("[data-cy=connect-dialog-target] option").map((option) => option.attributes("value"));
        expect(options).toEqual(["start", "a", "b"]);
        expect((wrapper.get("[data-cy=connect-dialog-outcome]").element as HTMLSelectElement).value).toBe("B");
    });

    it("contextMenu_Event_TogglesStart", async () => {
        const { store, wrapper } = setup();

        await wrapper.get("[data-cy=activity-start]").trigger("keydown", { key: "ContextMenu" });
        await wrapper.get("[data-cy=menu-toggle-start]").trigger("click");

        expect(store.getNode("start")?.isStart).toBe(false);
        expect(wrapper.find("[data-cy=context-menu]").exists()).toBe(false);
    });

    it("zoomControls_Buttons_ZoomWithinLimits", async () => {
        const { store, wrapper } = setup();

        for (let i = 0; i < 10; i++) {
            await wrapper.get("[data-cy=zoom-in]").trigger("click");
        }

        expect(store.state.viewport.zoom).toBe(2);
        expect(wrapper.get("[data-cy=zoom-reset]").text()).toBe("200%");

        for (let i = 0; i < 20; i++) {
            await wrapper.get("[data-cy=zoom-out]").trigger("click");
        }

        expect(store.state.viewport.zoom).toBe(0.25);

        await wrapper.get("[data-cy=zoom-reset]").trigger("click");
        expect(store.state.viewport.zoom).toBe(1);
    });

    it("edgeDelete_SelectedEdge_RemovesOnlyThatEdge", async () => {
        const { store, wrapper } = setup();

        await wrapper.get("[data-cy='edge-fork:B:b'] .wfd-edge-hit").trigger("pointerdown");
        expect(store.state.selectedTransitionKey).toBe("fork:B:b");

        await wrapper.get("[data-cy=edge-delete]").trigger("click");

        expect(store.state.transitions.map(transitionKey)).toEqual(["start:Done:fork", "fork:A:a"]);
        expect(store.state.nodes).toHaveLength(4);
    });
});
