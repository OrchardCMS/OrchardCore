import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import IssuesList from "../IssuesList.vue";
import type { DesignIssue } from "../../core/types";

interface StepIssue extends DesignIssue {
    stepId?: string;
}

const titles: Record<string, string> = { a: "Send email" };

const mountList = (issues: StepIssue[]) =>
    mount(IssuesList, {
        props: {
            issues,
            targetOf: (issue: StepIssue) => issue.stepId,
            titleOf: (issue: StepIssue) => (issue.stepId ? titles[issue.stepId] : undefined),
        },
    });

describe("IssuesList", () => {
    it("render_NoIssues_SaysSo", () => {
        const wrapper = mountList([]);

        expect(wrapper.find("[data-cy='issues-empty']").exists()).toBe(true);
    });

    it("render_Issues_ListsErrorsFirstWithTheNodeTitle", () => {
        const wrapper = mountList([
            { severity: "Warning", message: "Unused.", stepId: "a" },
            { severity: "Error", message: "No source.", code: "NoSource" },
        ]);
        const buttons = wrapper.findAll("button");

        expect(buttons[0].attributes("data-cy")).toBe("issue-NoSource");
        expect(buttons[0].attributes("disabled")).toBeDefined();
        expect(buttons[1].attributes("data-cy")).toBe("issue-1");
        expect(buttons[1].text()).toContain("Send email");
    });

    it("click_IssueOfANode_SelectsIt", async () => {
        const issue: StepIssue = { severity: "Error", message: "No input.", stepId: "a" };
        const wrapper = mountList([issue]);

        await wrapper.get("button").trigger("click");

        expect(wrapper.emitted("select")).toEqual([[issue]]);
    });
});
