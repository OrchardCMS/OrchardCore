import { reactive } from "vue";
import type { DesignerRun, RunsResult } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { isActiveRun } from "./runOverlay";

/**
 * How often a queued or running run is fetched again, in milliseconds.
 */
export const RUN_POLL_INTERVAL = 2000;

export interface RunTrackerOptions {
    store: DesignerStore;
    getRuns: () => Promise<RunsResult>;
    getRun: (runId: string) => Promise<DesignerRun>;
    run?: () => Promise<DesignerRun>;
    cancelRun?: (runId: string) => Promise<DesignerRun | unknown>;
    interval?: number;
}

export interface RunTrackerState {
    runs: DesignerRun[];
    loaded: boolean;
    loading: boolean;
    error: boolean;
    selectedRunId: string | null;
    starting: boolean;
    cancelling: boolean;
}

/**
 * The runs of the pipeline: the most recent ones, the one selected (whose steps the canvas shows, through
 * `store.state.overlayRun`), and polling while the selected run is queued or running.
 */
export const createRunTracker = (options: RunTrackerOptions) => {
    const { store } = options;
    const interval = options.interval ?? RUN_POLL_INTERVAL;
    const state = reactive<RunTrackerState>({
        runs: [],
        loaded: false,
        loading: false,
        error: false,
        selectedRunId: null,
        starting: false,
        cancelling: false,
    });

    let timer: ReturnType<typeof setTimeout> | undefined;
    let stopped = false;

    const selectedRun = () => state.runs.find((run) => run.runId === state.selectedRunId) ?? null;

    // Puts the newest data of a run in the list (first when it's new) and on the canvas when it's the selected one.
    const upsert = (run: DesignerRun) => {
        const index = state.runs.findIndex((existing) => existing.runId === run.runId);

        if (index >= 0) {
            state.runs.splice(index, 1, run);
        } else {
            state.runs.unshift(run);
        }

        if (run.runId === state.selectedRunId) {
            store.state.overlayRun = run;
        }

        if (!store.state.lastRun || store.state.lastRun.runId === run.runId || index < 0) {
            store.state.lastRun = run;
        }
    };

    const clearTimer = () => {
        clearTimeout(timer);
        timer = undefined;
    };

    const schedule = () => {
        clearTimer();

        if (stopped || !isActiveRun(selectedRun())) {
            return;
        }

        timer = setTimeout(async () => {
            timer = undefined;
            const runId = state.selectedRunId;

            if (!runId) {
                return;
            }

            try {
                upsert(await options.getRun(runId));
            } catch {
                // Tried again on the next tick.
            }

            schedule();
        }, interval);
    };

    return {
        state,
        selectedRun,

        /**
         * Loads the most recent runs.
         */
        async refresh() {
            state.loading = true;
            state.error = false;

            try {
                const result = await options.getRuns();
                const previous = selectedRun();

                state.runs = result.runs;
                state.loaded = true;

                // The selected run stays, even when the list doesn't have it (yet): a run just queued, or an older one.
                if (previous && !state.runs.some((run) => run.runId === previous.runId)) {
                    state.runs.unshift(previous);
                }

                const selected = selectedRun();

                if (selected) {
                    store.state.overlayRun = selected;
                }

                schedule();
            } catch {
                state.error = true;
            } finally {
                state.loading = false;
            }
        },

        /**
         * Shows a run's steps on the canvas, or none (null).
         */
        select(runId: string | null) {
            state.selectedRunId = runId;
            store.state.overlayRun = selectedRun();
            schedule();
        },

        /**
         * Queues a run of the published version and selects it. Resolves to the run, or throws the request's error.
         */
        async start(): Promise<DesignerRun> {
            state.starting = true;

            try {
                const run = await options.run!();

                upsert(run);
                state.selectedRunId = run.runId;
                store.state.overlayRun = run;
                schedule();

                return run;
            } finally {
                state.starting = false;
            }
        },

        async cancel(runId: string) {
            state.cancelling = true;

            try {
                await options.cancelRun!(runId);
                upsert(await options.getRun(runId));
                schedule();
            } finally {
                state.cancelling = false;
            }
        },

        stop() {
            stopped = true;
            clearTimer();
        },

        /**
         * Whether a poll is waiting (for tests).
         */
        get polling() {
            return timer !== undefined;
        },
    };
};

export type RunTracker = ReturnType<typeof createRunTracker>;
