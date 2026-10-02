import type { DesignerStore } from "../state/designerStore";

/**
 * A request that changes the draft. It receives the revision current when it starts.
 */
export type RevisionTask<T> = (revision: number) => Promise<T>;

/**
 * Runs the requests that change the draft one at a time. Every one of them (Save, AddActivity, the Editor
 * and Settings posts, Publish, Discard) takes the current revision and returns the next one, so two of them
 * in flight would make the second one a conflict.
 */
export const createRevisionQueue = (store: DesignerStore) => {
    let tail: Promise<unknown> = Promise.resolve();
    let pending = 0;

    return {
        /**
         * Queues `task` and resolves to its result. When the result carries a `revision`, it becomes the
         * current revision before the next task starts.
         */
        run<T>(task: RevisionTask<T>): Promise<T> {
            pending++;

            const result = tail.then(async () => {
                const value = await task(store.state.revision);
                const revision = (value as { revision?: unknown } | null | undefined)?.revision;

                if (typeof revision === "number") {
                    store.state.revision = revision;
                }

                return value;
            });

            tail = result.then(
                () => pending--,
                () => pending--,
            );

            return result;
        },

        /**
         * Whether a request is queued or in flight.
         */
        get busy() {
            return pending > 0;
        },
    };
};

export type RevisionQueue = ReturnType<typeof createRevisionQueue>;
