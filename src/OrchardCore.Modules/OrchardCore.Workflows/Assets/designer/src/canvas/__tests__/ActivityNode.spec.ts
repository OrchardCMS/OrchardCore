import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import ActivityNode from "../ActivityNode.vue";
import { createNode } from "./fixtures";

describe("ActivityNode", () => {
    it("render_Node_ShowsHeaderBodyPortsAndPosition", () => {
        const node = createNode("n1", { x: 120, y: 40 }, ["Yes", "No"]);

        const wrapper = mount(ActivityNode, { props: { node } });
        const root = wrapper.get("[data-cy=activity-n1]");

        expect(root.attributes("role")).toBe("group");
        expect(root.attributes("tabindex")).toBe("0");
        expect(root.attributes("style")).toContain("translate(120px, 40px)");
        expect(root.attributes("aria-label")).toContain("Title n1");
        expect(wrapper.get(".wfd-node-type").text()).toBe("Notify Task");
        expect(wrapper.get(".wfd-node-body").html()).toContain("<h4>Title n1</h4>");
        expect(wrapper.get("[data-cy=port-n1-Yes]").element.tagName).toBe("BUTTON");
        expect(wrapper.get("[data-cy=port-n1-No]").text()).toBe("No label");
        expect(wrapper.find("[data-cy=start-badge]").exists()).toBe(false);
    });

    it("render_StartEventWithIssues_ShowsStartAndIssueBadges", () => {
        const node = createNode("n1", { isEvent: true, isStart: true });
        const issues = [
            { severity: "Error" as const, code: "InvalidTransition", message: "Broken", activityId: "n1" },
            { severity: "Warning" as const, code: "UnreachableActivity", message: "Unreachable", activityId: "n1" },
        ];

        const wrapper = mount(ActivityNode, { props: { node, issues } });

        expect(wrapper.get("[data-cy=start-badge]").exists()).toBe(true);
        const badge = wrapper.get("[data-cy=issue-badge]");
        expect(badge.text()).toBe("2");
        expect(badge.classes()).toContain("text-bg-danger");
        expect(badge.attributes("title")).toBe("Broken\nUnreachable");
        expect(wrapper.get("[data-cy=activity-n1]").classes()).toEqual(expect.arrayContaining(["is-event", "is-start", "has-errors"]));
    });

    it("render_BlockingInTheViewer_ShowsTheBlockingBadgeAndState", () => {
        const wrapper = mount(ActivityNode, { props: { node: createNode("n1"), blocking: true, readOnly: true } });
        const root = wrapper.get("[data-cy=activity-n1]");

        expect(root.classes()).toContain("is-blocking");
        expect(wrapper.get("[data-cy=blocking-badge]").text()).toBe("Blocking");
        expect(root.attributes("aria-label")).toContain("BlockingActivity");
    });

    it("render_MissingAndSelected_AppliesStateClasses", () => {
        const wrapper = mount(ActivityNode, { props: { node: createNode("n1", { isMissing: true }), selected: true, dropTarget: true } });

        expect(wrapper.get("[data-cy=activity-n1]").classes()).toEqual(expect.arrayContaining(["is-missing", "is-selected", "is-drop-target"]));
    });

    it("keyboard_EnterAndContextMenu_EmitEditAndOpenMenu", async () => {
        const wrapper = mount(ActivityNode, { props: { node: createNode("n1") } });
        const root = wrapper.get("[data-cy=activity-n1]");

        await root.trigger("keydown", { key: "Enter" });
        await root.trigger("keydown", { key: "ContextMenu" });

        expect(wrapper.emitted("edit")).toHaveLength(1);
        expect(wrapper.emitted("open-menu")).toEqual([[null]]);
    });

    it("keyboard_ReadOnly_DoesNotEmitEdit", async () => {
        const wrapper = mount(ActivityNode, { props: { node: createNode("n1"), readOnly: true } });

        await wrapper.get("[data-cy=activity-n1]").trigger("keydown", { key: "Enter" });
        await wrapper.get("[data-cy=activity-n1]").trigger("dblclick");

        expect(wrapper.emitted("edit")).toBeUndefined();
        expect(wrapper.get("[data-cy=port-n1-Done]").attributes("disabled")).toBeDefined();
    });

    it("port_PointerDownAndEnter_StartConnectionOrOpenDialog", async () => {
        const wrapper = mount(ActivityNode, { props: { node: createNode("n1") } });
        const port = wrapper.get("[data-cy=port-n1-Done]");

        await port.trigger("pointerdown");
        await port.trigger("keydown", { key: "Enter" });

        expect(wrapper.emitted("port-pointerdown")?.[0][0]).toBe("Done");
        expect(wrapper.emitted("port-activate")).toEqual([["Done"]]);
        // A port never starts a node drag.
        expect(wrapper.emitted("node-pointerdown")).toBeUndefined();
    });
});
