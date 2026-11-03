import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import { defineComponent, h, reactive } from "vue";
import GraphCanvas from "../GraphCanvas.vue";
import type { CanvasEdge, NodeSize } from "../canvasTypes";
import type { Point } from "../geometry";

interface Node {
    id: string;
    x: number;
    y: number;
}

interface Edge {
    from: string;
    to: string;
}

const nodes: Node[] = [
    { id: "a", x: 0, y: 0 },
    { id: "b", x: 300, y: 0 },
];

const edges: CanvasEdge<Node, Edge>[] = [{ key: "a->b", edge: { from: "a", to: "b" }, source: nodes[0], target: nodes[1] }];

// Renders the nodes and edges through the slots, as a designer does.
const mountCanvas = (props: Record<string, unknown> = {}) => {
    const viewport = reactive({ panX: 0, panY: 0, zoom: 1 });

    const wrapper = mount(GraphCanvas, {
        props: {
            nodes,
            edges,
            selectedIds: [],
            viewport,
            nodeRect: (node: Node, layout: NodeSize | undefined) => ({ x: node.x, y: node.y, width: layout?.width ?? 200, height: layout?.height ?? 80 }),
            portAnchor: (node: Node) => ({ x: node.x + 200, y: node.y + 40 }),
            previewPath: (from: Point, to: Point) => `M${from.x},${from.y} L${to.x},${to.y}`,
            dragType: "application/x-test",
            label: "Canvas",
            ...props,
        },
        slots: {
            edge: (slot: { edge: CanvasEdge<Node, Edge>; lifted: boolean }) => h("path", { "data-cy": `edge-${slot.edge.key}`, "data-lifted": String(slot.lifted) }),
            node: (slot: { node: Node; selected: boolean; onNodePointerdown: (event: PointerEvent) => void; onFocus: () => void }) =>
                h(
                    defineComponent({
                        render: () =>
                            h("div", {
                                "data-node-id": slot.node.id,
                                "data-selected": String(slot.selected),
                                tabindex: 0,
                                onPointerdown: slot.onNodePointerdown,
                                onFocus: slot.onFocus,
                            }),
                    }),
                ),
        },
        attachTo: document.body,
    });

    return { wrapper, viewport };
};

describe("GraphCanvas", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("render_NodesAndEdges_UsesTheSlots", () => {
        const { wrapper } = mountCanvas({ selectedIds: ["b"] });

        expect(wrapper.findAll("[data-node-id]")).toHaveLength(2);
        expect(wrapper.get("[data-node-id='b']").attributes("data-selected")).toBe("true");
        expect(wrapper.get("[data-cy='edge-a->b']").attributes("data-lifted")).toBe("false");
    });

    it("render_HiddenNodes_LeavesThemOutButCentersOnThem", () => {
        const { wrapper, viewport } = mountCanvas({ hiddenIds: new Set(["b"]) });

        expect(wrapper.findAll("[data-node-id]").map((node) => node.attributes("data-node-id"))).toEqual(["a"]);

        // A node being shown is found before it is rendered.
        (wrapper.vm as unknown as { centerOn: (id: string) => void }).centerOn("b");
        expect(viewport.panX).toBe(-400);
    });

    it("keydown_Keys_AskTheDesignerToChangeTheGraph", async () => {
        const { wrapper } = mountCanvas({ selectedIds: ["a"] });
        const surface = wrapper.get("[data-cy='canvas-surface']");

        await surface.trigger("keydown", { key: "Delete" });
        await surface.trigger("keydown", { key: "ArrowRight" });
        await surface.trigger("keydown", { key: "ArrowUp", shiftKey: true });
        await surface.trigger("keydown", { key: "a", ctrlKey: true });
        await surface.trigger("keydown", { key: "Escape" });

        expect(wrapper.emitted("delete-selection")).toHaveLength(1);
        expect(wrapper.emitted("nudge")).toEqual([
            [10, 0],
            [0, -1],
        ]);
        expect(wrapper.emitted("select-all")).toHaveLength(1);
        expect(wrapper.emitted("clear-selection")).toHaveLength(1);
    });

    it("keydown_ReadOnly_DoesNotChangeTheGraph", async () => {
        const { wrapper } = mountCanvas({ selectedIds: ["a"], readOnly: true });
        const surface = wrapper.get("[data-cy='canvas-surface']");

        await surface.trigger("keydown", { key: "Delete" });
        await surface.trigger("keydown", { key: "ArrowRight" });

        expect(wrapper.emitted("delete-selection")).toBeUndefined();
        expect(wrapper.emitted("nudge")).toBeUndefined();
    });

    it("zoomControls_Click_ZoomsAroundTheCenter", async () => {
        const { wrapper, viewport } = mountCanvas();

        await wrapper.get("[data-cy='zoom-in']").trigger("click");
        expect(viewport.zoom).toBeCloseTo(1.2);
        expect(wrapper.get("[data-cy='zoom-reset']").text()).toBe("120%");

        await wrapper.get("[data-cy='zoom-reset']").trigger("click");
        expect(viewport.zoom).toBe(1);
    });

    it("focus_UnselectedNode_SelectsIt", async () => {
        const { wrapper } = mountCanvas();

        await wrapper.get("[data-node-id='b']").trigger("focus");

        expect(wrapper.emitted("select-node")).toEqual([["b", false, false]]);
    });

    it("openMenu_Items_ShowsTheContextMenu", async () => {
        const { wrapper } = mountCanvas();
        const run = vi.fn();

        (wrapper.vm as unknown as { openMenu: (items: unknown[], label: string, at: unknown) => void }).openMenu(
            [{ label: "Delete", icon: "fa-solid fa-trash", dataCy: "menu-delete", run }],
            "Actions",
            { nodeId: "a" },
        );
        await flushPromises();

        await wrapper.get("[data-cy='menu-delete']").trigger("click");

        expect(run).toHaveBeenCalledTimes(1);
    });

    it("drop_ToolboxItem_EmitsItAtTheCanvasPoint", async () => {
        const { wrapper } = mountCanvas();
        const dataTransfer = { types: ["application/x-test"], getData: () => "SendEmail", dropEffect: "" };

        await wrapper.get("[data-cy='canvas-surface']").trigger("drop", { dataTransfer, clientX: 50, clientY: 60 });

        expect(wrapper.emitted("drop")).toEqual([["SendEmail", { x: 50, y: 60 }]]);
    });
});
