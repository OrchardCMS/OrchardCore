import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import ActivityNode from "../ActivityNode.vue";
import { createNode } from "./fixtures";

describe("ActivityNode", () => {
    it("render_ExecutedOrFaulted_ShowsTheCountOrTheFault", () => {
        const executedOnce = mount(ActivityNode, { props: { node: createNode("n1"), executedCount: 1 } });
        const executedTwice = mount(ActivityNode, { props: { node: createNode("n2"), executedCount: 2 } });
        const faulted = mount(ActivityNode, { props: { node: createNode("n3"), executedCount: 1, faulted: true } });

        expect(executedOnce.get("[data-cy=activity-n1]").classes()).toContain("is-executed");
        expect(executedOnce.find("[data-cy=executed-badge]").exists()).toBe(false);
        expect(executedTwice.get("[data-cy=executed-badge]").text()).toBe("×2");
        expect(executedTwice.get("[data-cy=activity-n2]").attributes("aria-label")).toContain("ExecutedTimes");
        expect(faulted.get("[data-cy=activity-n3]").classes()).toContain("is-faulted");
        expect(faulted.find("[data-cy=faulted-badge]").exists()).toBe(true);
        expect(faulted.find("[data-cy=executed-badge]").exists()).toBe(false);
    });

    it("render_TaskWithRetries_ShowsHowManyTimesItsRetried", () => {
        const retried = mount(ActivityNode, { props: { node: createNode("n1", { retries: 3 }) } });
        const notRetried = mount(ActivityNode, { props: { node: createNode("n2") } });

        expect(retried.get("[data-cy=retry-badge]").text()).toBe("3");
        expect(retried.get("[data-cy=retry-badge]").attributes("title")).toBe("RetriedTimes");
        expect(retried.get("[data-cy=activity-n1]").attributes("aria-label")).toContain("RetriedTimes");
        expect(notRetried.find("[data-cy=retry-badge]").exists()).toBe(false);
    });

    it("render_ScriptErrors_ShowsAWarningUnlessTheActivityFaulted", () => {
        const withErrors = mount(ActivityNode, { props: { node: createNode("n1"), executedCount: 1, scriptErrors: ["x is not defined", "y is not defined"] } });
        const faulted = mount(ActivityNode, { props: { node: createNode("n2"), executedCount: 1, faulted: true, scriptErrors: ["x is not defined"] } });

        const root = withErrors.get("[data-cy=activity-n1]");
        expect(root.classes()).toContain("has-script-errors");
        expect(root.attributes("aria-label")).toContain("ScriptErrorActivity");
        expect(withErrors.get("[data-cy=script-error-badge]").attributes("title")).toBe("ScriptErrorActivity\nx is not defined\ny is not defined");

        expect(faulted.get("[data-cy=activity-n2]").classes()).not.toContain("has-script-errors");
        expect(faulted.find("[data-cy=script-error-badge]").exists()).toBe(false);
        expect(mount(ActivityNode, { props: { node: createNode("n3") } }).find("[data-cy=script-error-badge]").exists()).toBe(false);
    });

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

    it("editButton_Clicked_EmitsEditWithoutAPointerDownOnTheNode", async () => {
        const wrapper = mount(ActivityNode, { props: { node: createNode("n1") } });

        await wrapper.get("[data-cy=node-edit]").trigger("pointerdown");
        await wrapper.get("[data-cy=node-edit]").trigger("click");

        // The button doesn't start a drag of the node.
        expect(wrapper.emitted("node-pointerdown")).toBeUndefined();
        expect(wrapper.emitted("edit")).toHaveLength(1);
        expect(mount(ActivityNode, { props: { node: createNode("n2"), readOnly: true } }).find("[data-cy=node-edit]").exists()).toBe(false);
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
