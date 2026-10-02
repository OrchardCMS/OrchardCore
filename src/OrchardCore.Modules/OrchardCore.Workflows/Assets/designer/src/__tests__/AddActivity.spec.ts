import { describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import App from "../App.vue";
import { createDesignerStore } from "../state/designerStore";
import { ACTIVITY_DRAG_TYPE } from "../toolbox/filter";
import type { DesignerApi } from "../api/designerApi";
import type { DesignerConfig } from "../config";
import { createDefinition, createNode } from "../canvas/__tests__/fixtures";

const config: DesignerConfig = {
    workflowTypeId: 1,
    readOnly: false,
    urls: { definition: "", library: "", save: "", addActivity: "", editor: "", settings: "", publish: "", discard: "" },
    instancesUrl: "",
    exportUrl: "",
    listUrl: "",
    translations: {},
};

const setup = async () => {
    const added = createNode("new", { x: 90, y: 80 });
    const api = {
        getDefinition: vi.fn().mockResolvedValue(createDefinition()),
        getLibrary: vi.fn().mockResolvedValue({ categories: [] }),
        addActivity: vi.fn().mockResolvedValue({ revision: 1, node: added, issues: [] }),
    } as unknown as DesignerApi & { addActivity: ReturnType<typeof vi.fn> };
    const store = createDesignerStore();
    const wrapper = mount(App, { props: { config, api, store }, attachTo: document.body });
    await flushPromises();

    return { api, store, wrapper };
};

const drop = (element: Element, clientX: number, clientY: number, types: string[], activityName: string) => {
    const event = new Event("drop", { bubbles: true, cancelable: true });
    Object.assign(event, {
        clientX,
        clientY,
        dataTransfer: { types, getData: (type: string) => (type === ACTIVITY_DRAG_TYPE ? activityName : "") },
    });
    element.dispatchEvent(event);
};

describe("adding activities", () => {
    it("drop_PannedAndZoomedCanvas_AddsActivityAtCanvasCoordinates", async () => {
        const { api, store, wrapper } = await setup();
        Object.assign(store.state.viewport, { panX: 100, panY: 50, zoom: 2 });

        // The canvas element is at (0, 0) in jsdom, so client coordinates are canvas-element coordinates.
        drop(wrapper.get("[data-cy=canvas-surface]").element, 500, 250, [ACTIVITY_DRAG_TYPE], "NotifyTask");
        await flushPromises();

        // Canvas point ((500 - 100) / 2, (250 - 50) / 2) = (200, 100); the header is centered on it, so the
        // node's top-left is (200 - 110, 100 - 16), snapped to the 10 px grid.
        expect(api.addActivity).toHaveBeenCalledWith(0, "NotifyTask", 90, 80);
        expect(store.getNode("new")).toBeDefined();
        expect(store.state.selectedNodeIds).toEqual(["new"]);
        expect(store.state.revision).toBe(1);
        expect(store.state.canUndo).toBe(true);
        expect(wrapper.emitted("edit")).toEqual([["new"]]);

        // Undoing the add removes the node; the next save reports it as removed.
        store.undo();
        expect(store.toSavePayload().removedActivityIds).toEqual(["new"]);

        wrapper.unmount();
    });

    it("drop_OtherDragData_IsIgnored", async () => {
        const { api, wrapper } = await setup();

        drop(wrapper.get("[data-cy=canvas-surface]").element, 10, 10, ["text/plain"], "");
        await flushPromises();

        expect(api.addActivity).not.toHaveBeenCalled();
        wrapper.unmount();
    });
});
