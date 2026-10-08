import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import RunsTab from "../RunsTab.vue";
import ActivityPanel from "../ActivityPanel.vue";
import WorkflowPanel from "../WorkflowPanel.vue";
import { createDesignerStore } from "../../state/designerStore";
import { createDefinition } from "../../canvas/__tests__/fixtures";
import type { DesignerApi } from "../../api/designerApi";
import type { JournalData, JournalRecord } from "../../api/types";

const record = (sequence: number, activityId: string, overrides: Partial<JournalRecord> = {}): JournalRecord => ({
    sequence,
    activityId,
    activityName: "IfElseTask",
    isResume: false,
    status: "Completed",
    outcomes: ["True"],
    startedUtc: "2026-10-08T12:00:00Z",
    completedUtc: "2026-10-08T12:00:00Z",
    durationMilliseconds: 2,
    hasData: true,
    ...overrides,
});

const data: JournalData = {
    evaluations: [{ property: "Condition", syntax: "JavaScript", expression: "variable('total') >= 50", result: "true" }],
    outputs: {},
    variables: { total: '{"amount":90}' },
    properties: {},
    lastResult: null,
    isTruncated: true,
};

const setup = (journal: JournalRecord[], getJournalData = vi.fn(() => Promise.resolve(data))) => {
    const store = createDesignerStore();
    store.loadDefinition({ ...createDefinition(), instance: { id: 3, workflowId: "w", status: "Finished", blockingActivityIds: [], journal } });
    const api = { getJournalData } as unknown as DesignerApi;

    return { store, api, getJournalData };
};

describe("RunsTab", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("render_Runs_ListsTheActivitysRunsAndLoadsTheDataOfARunWhenOpened", async () => {
        const { store, api, getJournalData } = setup([record(1, "start"), record(2, "a"), record(5, "a", { outcomes: ["False"] })]);
        const wrapper = mount(RunsTab, { props: { store, api, activityId: "a" } });

        expect(wrapper.findAll(".wfd-run").map((run) => run.attributes("data-cy"))).toEqual(["run-2", "run-5"]);
        expect(getJournalData).not.toHaveBeenCalled();

        await wrapper.get("[data-cy=run-5] [data-cy=run-toggle]").trigger("click");
        await flushPromises();

        expect(getJournalData).toHaveBeenCalledWith(5);
        const run = wrapper.get("[data-cy=run-5]");
        expect(run.get("[data-cy=run-toggle]").attributes("aria-expanded")).toBe("true");
        expect(run.get("[data-cy=run-evaluations]").text()).toContain("Condition");
        expect(run.get("[data-cy=run-evaluation-result]").text()).toBe("true");
        // An object is shown indented.
        expect(run.get("[data-cy=run-variables] pre").text()).toBe('{\n  "amount": 90\n}');
        expect(run.find("[data-cy=run-truncated]").exists()).toBe(true);

        // Closed and opened again, it isn't loaded again.
        await wrapper.get("[data-cy=run-5] [data-cy=run-toggle]").trigger("click");
        await wrapper.get("[data-cy=run-5] [data-cy=run-toggle]").trigger("click");
        expect(getJournalData).toHaveBeenCalledTimes(1);
    });

    it("render_RunWithoutData_SaysHowToRecordIt", async () => {
        const { store, api, getJournalData } = setup([record(1, "a", { hasData: false })]);
        const wrapper = mount(RunsTab, { props: { store, api, activityId: "a" } });

        await wrapper.get("[data-cy=run-toggle]").trigger("click");

        expect(wrapper.find("[data-cy=run-no-data]").exists()).toBe(true);
        expect(getJournalData).not.toHaveBeenCalled();
    });

    it("journal_RecordSelected_OpensItsActivityOnTheRunsTabOnThatRun", async () => {
        const { store, api, getJournalData } = setup([record(1, "start"), record(2, "a")]);
        const activityPanel = mount(ActivityPanel, { props: { store, api, readOnly: true }, attachTo: document.body });
        const workflowPanel = mount(WorkflowPanel, { props: { store, api, readOnly: true }, attachTo: document.body });

        await workflowPanel.get("[data-cy=panel-tab-journal]").trigger("click");
        await workflowPanel.get("[data-cy=journal-record-2] button").trigger("click");
        await flushPromises();

        expect(store.state.selectedNodeIds).toEqual(["a"]);
        expect(activityPanel.get("[data-cy=activity-tab-runs]").attributes("aria-selected")).toBe("true");
        expect(activityPanel.get("[data-cy=run-2] [data-cy=run-toggle]").attributes("aria-expanded")).toBe("true");
        expect(getJournalData).toHaveBeenCalledWith(2);

        // Another activity, selected on the canvas, opens on its details.
        store.state.selectedNodeIds = ["start"];
        await flushPromises();
        expect(activityPanel.get("[data-cy=activity-tab-details]").attributes("aria-selected")).toBe("true");
    });
});
