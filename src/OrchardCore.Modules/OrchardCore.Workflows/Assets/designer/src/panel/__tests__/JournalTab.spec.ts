import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import JournalTab from "../JournalTab.vue";
import { createDesignerStore } from "../../state/designerStore";
import { createDefinition } from "../../canvas/__tests__/fixtures";
import type { JournalRecord } from "../../api/types";

const record = (sequence: number, activityId: string, status: string, overrides: Partial<JournalRecord> = {}): JournalRecord => ({
    sequence,
    activityId,
    activityName: "NotifyTask",
    isResume: false,
    status,
    outcomes: status === "Completed" ? ["Done"] : [],
    startedUtc: "2026-10-07T12:00:00Z",
    completedUtc: "2026-10-07T12:00:01Z",
    durationMilliseconds: 12.4,
    ...overrides,
});

const setup = (journal: JournalRecord[]) => {
    const store = createDesignerStore();
    store.loadDefinition({ ...createDefinition(), instance: { id: 3, workflowId: "w", status: "Faulted", blockingActivityIds: [], journal } });

    return mount(JournalTab, { props: { store } });
};

describe("JournalTab", () => {
    it("render_Records_ListsThemWithTheirStatusOutcomesAndErrors", () => {
        const wrapper = setup([record(1, "start", "Completed"), record(2, "a", "Faulted", { error: "Boom", activityTitle: "Send the email" })]);

        const first = wrapper.get("[data-cy=journal-record-1]");
        const second = wrapper.get("[data-cy=journal-record-2]");

        expect(first.text()).toContain("Title start");
        expect(first.get("[data-cy=journal-outcomes]").text()).toBe("Done");
        expect(first.get("[data-cy=journal-status]").classes()).toContain("text-bg-success");
        expect(second.text()).toContain("Send the email");
        expect(second.get("[data-cy=journal-status]").classes()).toContain("text-bg-danger");
        expect(second.get("[data-cy=journal-error]").text()).toBe("Boom");
    });

    it("select_Record_EmitsItsActivity", async () => {
        const wrapper = setup([record(1, "fork", "Completed")]);

        await wrapper.get("[data-cy=journal-record-1] button").trigger("click");

        expect(wrapper.emitted("select")).toEqual([["fork"]]);
    });

    it("render_NoRecord_SaysSo", () => {
        expect(setup([]).find("[data-cy=journal-empty]").exists()).toBe(true);
    });
});
