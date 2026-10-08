import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import TransitionEdge from "../TransitionEdge.vue";
import { createNode } from "./fixtures";

const mountEdge = (props: Record<string, unknown> = {}) =>
    mount(TransitionEdge, {
        props: {
            edgeKey: "s:Done:t",
            source: createNode("s", { x: 0, y: 0 }),
            target: createNode("t", { x: 400, y: 0 }),
            outcome: "Done",
            sourceLayout: { width: 220, height: 100, ports: { Done: { x: 220, y: 80 } } },
            targetLayout: { width: 220, height: 100, ports: {} },
            ...props,
        },
    });

describe("TransitionEdge", () => {
    it("render_TakenTransition_IsHighlightedWithItsCount", () => {
        expect(mountEdge({ executedCount: 1 }).get(".wfd-edge").classes()).toContain("is-executed");
        expect(mountEdge({ executedCount: 1 }).get(".wfd-edge-label").text()).toBe("Done label");
        expect(mountEdge({ executedCount: 3 }).get(".wfd-edge-label").text()).toBe("Done label ×3");
        expect(mountEdge().get(".wfd-edge").classes()).not.toContain("is-executed");
    });

    it("render_Edge_DrawsARouteFromPortToTargetSideWithLabel", () => {
        const wrapper = mountEdge();

        expect(wrapper.get("[data-cy='edge-s:Done:t']").exists()).toBe(true);
        // The target faces the port, so the route is a straight line into its left side.
        expect(wrapper.get(".wfd-edge-line").attributes("d")).toBe("M220,80 L400,80");
        expect(wrapper.get(".wfd-edge-line").attributes("marker-end")).toBe("url(#wfd-arrow)");
        expect(wrapper.get(".wfd-edge-label").text()).toBe("Done label");
        expect(wrapper.find("[data-cy=edge-delete]").exists()).toBe(false);
    });

    it("render_SourceMoved_RecomputesPath", async () => {
        const source = createNode("s", { x: 0, y: 0 });
        const wrapper = mountEdge({ source });

        await wrapper.setProps({ source: { ...source, x: 100 } });

        expect(wrapper.get(".wfd-edge-line").attributes("d")).toMatch(/^M320,80 /);
    });

    it("selected_DeleteAffordance_EmitsDelete", async () => {
        const wrapper = mountEdge({ selected: true });

        expect(wrapper.get(".wfd-edge-line").attributes("marker-end")).toBe("url(#wfd-arrow-selected)");
        await wrapper.get("[data-cy=edge-delete]").trigger("click");
        await wrapper.get("[data-cy=edge-delete]").trigger("keydown", { key: "Enter" });

        expect(wrapper.emitted("delete")).toHaveLength(2);
    });

    it("selectedReadOnly_NoDeleteAffordance", () => {
        const wrapper = mountEdge({ selected: true, readOnly: true });

        expect(wrapper.find("[data-cy=edge-delete]").exists()).toBe(false);
    });

    it("pointerDown_PrimaryButton_EmitsSelect", async () => {
        const wrapper = mountEdge();

        await wrapper.get(".wfd-edge-hit").trigger("pointerdown");

        expect(wrapper.emitted("select")).toHaveLength(1);
    });
});
