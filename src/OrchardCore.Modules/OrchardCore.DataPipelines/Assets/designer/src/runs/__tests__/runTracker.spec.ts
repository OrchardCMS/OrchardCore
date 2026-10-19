import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { flushPromises } from "@vue/test-utils";
import { createRunTracker, RUN_POLL_INTERVAL } from "../runTracker";
import { createDesignerStore } from "../../state/designerStore";
import { createDefinition, createRun } from "../../__tests__/fixtures";
import type { DesignerRun } from "../../api/types";

const setup = (runs: DesignerRun[] = [createRun({ runId: "old", status: "Succeeded" })]) => {
    const store = createDesignerStore();
    store.loadDefinition(createDefinition());
    const getRuns = vi.fn().mockResolvedValue({ runs });
    const getRun = vi.fn();
    const run = vi.fn();
    const cancelRun = vi.fn().mockResolvedValue({});
    const tracker = createRunTracker({ store, getRuns, getRun, run, cancelRun });

    return { store, tracker, getRuns, getRun, run, cancelRun };
};

describe("runTracker", () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    it("start_QueuedRun_SelectsItAndPollsUntilItCompletes", async () => {
        const { store, tracker, getRun, run } = setup();
        await tracker.refresh();
        run.mockResolvedValue(createRun({ runId: "new", status: "Queued", steps: [] }));
        getRun.mockResolvedValueOnce(createRun({ runId: "new", status: "Running" })).mockResolvedValueOnce(createRun({ runId: "new", status: "Succeeded" }));

        await tracker.start();

        expect(tracker.state.runs.map((item) => item.runId)).toEqual(["new", "old"]);
        expect(tracker.state.selectedRunId).toBe("new");
        expect(store.state.overlayRun?.status).toBe("Queued");

        vi.advanceTimersByTime(RUN_POLL_INTERVAL);
        await flushPromises();
        expect(getRun).toHaveBeenCalledWith("new");
        expect(store.state.overlayRun?.status).toBe("Running");

        vi.advanceTimersByTime(RUN_POLL_INTERVAL);
        await flushPromises();
        expect(store.state.overlayRun?.status).toBe("Succeeded");
        expect(store.state.lastRun?.status).toBe("Succeeded");

        // A completed run isn't fetched again.
        vi.advanceTimersByTime(RUN_POLL_INTERVAL * 5);
        await flushPromises();
        expect(getRun).toHaveBeenCalledTimes(2);
        expect(tracker.polling).toBe(false);
    });

    it("select_CompletedRunOrNone_ShowsItWithoutPolling", async () => {
        const { store, tracker, getRun } = setup();
        await tracker.refresh();

        tracker.select("old");
        expect(store.state.overlayRun?.runId).toBe("old");
        expect(tracker.polling).toBe(false);

        tracker.select(null);
        expect(store.state.overlayRun).toBeNull();
        expect(getRun).not.toHaveBeenCalled();
    });

    it("poll_RequestFails_TriesAgainOnTheNextTick", async () => {
        const { tracker, getRun } = setup([createRun({ runId: "live", status: "Running" })]);
        await tracker.refresh();
        getRun.mockRejectedValueOnce(new Error("offline")).mockResolvedValueOnce(createRun({ runId: "live", status: "Failed" }));

        tracker.select("live");
        vi.advanceTimersByTime(RUN_POLL_INTERVAL);
        await flushPromises();
        expect(tracker.polling).toBe(true);

        vi.advanceTimersByTime(RUN_POLL_INTERVAL);
        await flushPromises();
        expect(tracker.state.runs[0].status).toBe("Failed");
        expect(tracker.polling).toBe(false);
    });

    it("cancel_Run_PostsItAndFetchesItAgain", async () => {
        const { tracker, getRun, cancelRun } = setup([createRun({ runId: "live", status: "Running" })]);
        await tracker.refresh();
        getRun.mockResolvedValue(createRun({ runId: "live", status: "Cancelled", canCancel: false }));

        await tracker.cancel("live");

        expect(cancelRun).toHaveBeenCalledWith("live");
        expect(tracker.state.runs[0]).toMatchObject({ status: "Cancelled", canCancel: false });
    });

    it("refresh_SelectedRunNotListedYet_KeepsItSelected", async () => {
        const { store, tracker, run } = setup([]);
        run.mockResolvedValue(createRun({ runId: "new", status: "Succeeded" }));

        await tracker.start();
        await tracker.refresh();

        expect(tracker.state.runs.map((item) => item.runId)).toEqual(["new"]);
        expect(store.state.overlayRun?.runId).toBe("new");
    });

    it("stop_Polling_StopsIt", async () => {
        const { tracker, getRun } = setup([createRun({ runId: "live", status: "Running" })]);
        await tracker.refresh();
        tracker.select("live");

        tracker.stop();
        vi.advanceTimersByTime(RUN_POLL_INTERVAL * 3);
        await flushPromises();

        expect(getRun).not.toHaveBeenCalled();
    });
});
