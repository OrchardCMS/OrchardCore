import { describe, expect, it } from "vitest";
import { scriptErrorsByActivity } from "../scriptErrors";
import type { JournalRecord } from "../../api/types";

const record = (sequence: number, activityId: string, status: string, error?: string): JournalRecord => ({
    sequence,
    activityId,
    activityName: "SetPropertyTask",
    isResume: false,
    status,
    outcomes: [],
    startedUtc: "2026-10-07T12:00:00Z",
    completedUtc: "2026-10-07T12:00:01Z",
    durationMilliseconds: 1,
    error,
});

describe("scriptErrorsByActivity", () => {
    it("scriptErrorsByActivity_Journal_ListsTheErrorsOfTheRecordsThatDidNotFault", () => {
        const errors = scriptErrorsByActivity({
            id: 1,
            workflowId: "w",
            status: "Faulted",
            blockingActivityIds: [],
            journal: [
                record(1, "a", "Completed", "x is not defined"),
                record(2, "b", "Completed"),
                record(3, "a", "Completed", "x is not defined"),
                record(4, "a", "Halted", "y is not defined"),
                record(5, "c", "Faulted", "Boom"),
            ],
        });

        // Each message once; the fault is shown as a fault.
        expect([...errors.entries()]).toEqual([["a", ["x is not defined", "y is not defined"]]]);
    });

    it("scriptErrorsByActivity_NoInstanceOrJournal_IsEmpty", () => {
        expect(scriptErrorsByActivity(null).size).toBe(0);
        expect(scriptErrorsByActivity({ id: 1, workflowId: "w", status: "Finished", blockingActivityIds: [] }).size).toBe(0);
    });
});
