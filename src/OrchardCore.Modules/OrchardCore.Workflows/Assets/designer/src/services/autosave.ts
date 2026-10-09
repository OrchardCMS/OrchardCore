import { shallowRef, watch, type WatchStopHandle } from "vue";
import { DesignerApiError } from "../api/designerApi";
import type { ProblemDetails, SavePayload, SaveResult } from "../api/types";
import type { DesignerStore, SaveStatus } from "../state/designerStore";
import type { RevisionQueue } from "./revisionQueue";

/**
 * How long autosave waits after the last graph change, in milliseconds.
 */
export const AUTOSAVE_DELAY = 800;

/**
 * How long autosave waits before each retry after a network or server error; the last delay repeats.
 */
export const RETRY_DELAYS = [1000, 2000, 5000, 10000, 30000];

export interface AutosaveOptions {
    store: DesignerStore;
    queue: RevisionQueue;
    save: (payload: SavePayload) => Promise<SaveResult>;
    delay?: number;
    retryDelays?: number[];
    /**
     * Called when a request is rejected because the draft changed in the meantime.
     */
    onConflict?: (error: DesignerApiError) => void;
    /**
     * Called after each successful save.
     */
    onSaved?: () => void;
}

const isRetryable = (error: unknown) => error instanceof DesignerApiError && (error.isNetworkError || error.status >= 500);

/**
 * Saves the graph to the draft: after `delay` without changes, one request at a time, with the changes
 * made in the meantime saved together afterwards. Network and server errors are retried with backoff. A
 * conflict stops autosaving until it is resolved (`overwrite`, or `reset` after reloading).
 */
export const createAutosave = (options: AutosaveOptions) => {
    const { store, queue } = options;
    const state = store.state;
    const delay = options.delay ?? AUTOSAVE_DELAY;
    const retryDelays = options.retryDelays ?? RETRY_DELAYS;

    // The ProblemDetails of the conflict to resolve, if any.
    const conflict = shallowRef<ProblemDetails | null>(null);

    let savedVersion = state.changeVersion;
    let timer: ReturnType<typeof setTimeout> | undefined;
    let retryTimer: ReturnType<typeof setTimeout> | undefined;
    let attempt = 0;
    let forceNext = false;
    let inFlight: Promise<void> | null = null;
    let paused = false;
    let stopWatching: WatchStopHandle | null = null;

    const setStatus = (status: SaveStatus) => {
        state.saveStatus = status;
    };

    const hasChanges = () => state.changeVersion !== savedVersion;

    const clearTimers = () => {
        clearTimeout(timer);
        clearTimeout(retryTimer);
        timer = undefined;
        retryTimer = undefined;
    };

    const schedule = () => {
        clearTimeout(timer);
        timer = setTimeout(() => {
            timer = undefined;
            void save();
        }, delay);
    };

    const reportConflict = (error: DesignerApiError) => {
        clearTimers();
        conflict.value = error.problem;
        setStatus("conflict");
        options.onConflict?.(error);
    };

    const onError = (error: unknown, forced: boolean) => {
        if (error instanceof DesignerApiError && error.isConflict) {
            reportConflict(error);

            return;
        }

        // The next attempt saves again what this one couldn't, even when nothing changed since.
        forceNext = forced;

        if (isRetryable(error)) {
            setStatus("offline");
            retryTimer = setTimeout(() => {
                retryTimer = undefined;
                void save();
            }, retryDelays[Math.min(attempt, retryDelays.length - 1)]);
            attempt++;
        } else {
            setStatus("failed");
        }
    };

    /**
     * Saves now. While a save is in flight it returns that one; the changes made meanwhile are saved after
     * it. `force` saves even when nothing changed, to write the graph over a conflicting draft.
     */
    const save = (force = false): Promise<void> => {
        if (inFlight) {
            return inFlight;
        }

        const forced = force || forceNext;

        if (paused || state.saveStatus === "conflict" || (!forced && !hasChanges())) {
            return Promise.resolve();
        }

        clearTimers();
        forceNext = false;
        setStatus("saving");

        inFlight = queue
            .run(async () => {
                const version = state.changeVersion;
                const payload = store.toSavePayload();
                const result = await options.save(payload);

                store.markSaved(payload, result);
                savedVersion = version;
            })
            .then(
                () => {
                    inFlight = null;
                    attempt = 0;

                    if (state.saveStatus === "conflict") {
                        // Another request reported a conflict while this save was in flight.
                    } else if (hasChanges()) {
                        setStatus("unsaved");
                        schedule();
                    } else {
                        setStatus("saved");
                    }

                    options.onSaved?.();
                },
                (error: unknown) => {
                    inFlight = null;
                    onError(error, forced);
                },
            );

        return inFlight;
    };

    const onChange = () => {
        if (paused || !hasChanges() || state.saveStatus === "conflict") {
            return;
        }

        // A save in flight saves these changes after it; a pending retry includes them.
        if (inFlight || retryTimer) {
            return;
        }

        setStatus("unsaved");
        schedule();
    };

    return {
        conflict,

        /**
         * Starts watching the graph. Call it once the definition is loaded.
         */
        start() {
            savedVersion = state.changeVersion;
            stopWatching ??= watch(() => state.changeVersion, onChange);
        },

        stop() {
            stopWatching?.();
            stopWatching = null;
            clearTimers();
        },

        /**
         * Holds the saves, for example while the draft is being discarded. The changes are kept.
         */
        pause() {
            paused = true;
            clearTimers();
        },

        /**
         * Saves again, starting with the changes made while paused.
         */
        resume() {
            paused = false;

            if (state.saveStatus === "offline" || state.saveStatus === "failed") {
                void save();
            } else {
                onChange();
            }
        },

        save,

        /**
         * Saves the pending changes now and waits for them. Resolves to whether everything is saved.
         */
        async flush(): Promise<boolean> {
            for (;;) {
                if (inFlight) {
                    await inFlight;
                    continue;
                }

                if (state.saveStatus === "conflict") {
                    return false;
                }

                if (!hasChanges() && !forceNext) {
                    clearTimers();

                    return true;
                }

                await save();

                if (state.saveStatus !== "saved" && state.saveStatus !== "unsaved") {
                    return false;
                }
            }
        },

        /**
         * Reports a conflict returned by another request that changes the draft.
         */
        reportConflict,

        /**
         * Resolves the conflict by saving the graph over the draft, with the draft's current revision.
         */
        overwrite(): Promise<void> {
            const revision = conflict.value?.currentRevision;

            if (typeof revision === "number") {
                state.revision = revision;
            }

            conflict.value = null;
            attempt = 0;
            setStatus("saving");

            return save(true);
        },

        /**
         * Treats the current graph as saved, for example after loading the definition again.
         */
        reset() {
            clearTimers();
            conflict.value = null;
            attempt = 0;
            forceNext = false;
            savedVersion = state.changeVersion;
            setStatus("saved");
        },

        /**
         * Whether leaving the page now would lose changes.
         */
        hasUnsavedChanges() {
            return state.saveStatus !== "saved" || hasChanges() || queue.busy;
        },
    };
};

export type Autosave = ReturnType<typeof createAutosave>;
