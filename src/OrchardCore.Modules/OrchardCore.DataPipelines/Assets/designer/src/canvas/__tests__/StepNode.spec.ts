import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import StepNode from "../StepNode.vue";
import { createDefinition } from "../../__tests__/fixtures";

const node = (id: string) => createDefinition().nodes.find((item) => item.id === id)!;

describe("StepNode", () => {
    it("render_PortsOnBothSides_LabelsThemOnlyWhenASideHasSeveral", () => {
        const join = mount(StepNode, { props: { node: node("join") } });
        const filter = mount(StepNode, { props: { node: node("filter") } });

        expect(join.findAll(".dpd-port.is-input .dpd-port-label").map((label) => label.text())).toEqual(["Left", "Right"]);
        expect(join.findAll(".dpd-port.is-output .dpd-port-label")).toHaveLength(0);
        expect(filter.findAll(".dpd-port.is-output .dpd-port-label").map((label) => label.text())).toEqual(["Matched", "Unmatched"]);
        expect(filter.findAll(".dpd-port.is-input .dpd-port-label")).toHaveLength(0);
    });

    it("render_FilePort_HasItsOwnLook", () => {
        const wrapper = mount(StepNode, { props: { node: node("csv") } });

        expect(wrapper.get("[data-cy=port-csv-out-File]").classes()).toContain("is-files");
        expect(wrapper.get("[data-cy=port-csv-in-Input]").classes()).toContain("is-records");
    });

    it("render_DesignHtmlIssuesAndRun_ShowsSummaryBadgeAndCount", () => {
        const wrapper = mount(StepNode, {
            props: {
                node: node("filter"),
                issues: [{ severity: "Error", message: "No condition", stepId: "filter" }],
                overlay: { status: "Failed", label: "1,234 rows", title: "Failed" },
            },
        });

        expect(wrapper.get(".dpd-node-summary").html()).toContain("<p>Summary of filter</p>");
        expect(wrapper.get("[data-cy=issue-badge]").text()).toBe("1");
        expect(wrapper.get("[data-cy=run-badge]").text()).toBe("1,234 rows");
        expect(wrapper.classes()).toEqual(expect.arrayContaining(["is-run-failed", "has-errors"]));
        expect(wrapper.attributes("aria-label")).toContain("1 issues");
    });

    it("drag_Compatibility_MarksTheInputsThatCanOrCannotReceiveIt", () => {
        const wrapper = mount(StepNode, { props: { node: node("join"), compatibility: { Left: false, Right: true }, hoveredInput: "Left" } });

        expect(wrapper.get("[data-cy=port-join-in-Left]").classes()).toEqual(expect.arrayContaining(["is-incompatible", "is-hovered"]));
        expect(wrapper.get("[data-cy=port-join-in-Right]").classes()).toContain("is-compatible");
    });

    it("output_PointerDownAndEnter_StartAConnectionOrOpenTheDialog", async () => {
        const wrapper = mount(StepNode, { props: { node: node("filter") } });
        const port = wrapper.get("[data-cy=port-filter-out-Unmatched]");

        port.element.dispatchEvent(new PointerEvent("pointerdown", { button: 0, bubbles: true }));
        await port.trigger("keydown", { key: "Enter" });

        expect(wrapper.emitted("port-pointerdown")?.[0][0]).toBe("Unmatched");
        expect(wrapper.emitted("port-activate")).toEqual([["Unmatched"]]);
        // The port never starts a step drag.
        expect(wrapper.emitted("node-pointerdown")).toBeUndefined();
    });
});
