import { History, type Command } from "../core/history";

/**
 * The state the history of a graph keeps up to date: a counter of the changes, which autosave compares with the one it
 * last saved, and whether undo and redo are available.
 */
export interface HistoryState {
    changeVersion: number;
    canUndo: boolean;
    canRedo: boolean;
}

/**
 * The undo history of a designer graph. Every change made through it increments `state.changeVersion`.
 */
export const createGraphHistory = (state: HistoryState, history = new History()) => {
    history.subscribe(() => {
        state.canUndo = history.canUndo;
        state.canRedo = history.canRedo;
    });

    const markChanged = () => {
        state.changeVersion++;
    };

    return {
        history,
        markChanged,

        /**
         * Applies a command and records it in the history.
         */
        execute(command: Command) {
            history.execute(command);
            markChanged();
        },

        /**
         * Records a command whose effect is already applied.
         */
        record(command: Command) {
            history.record(command);
            markChanged();
        },

        undo() {
            if (history.undo()) {
                markChanged();
            }
        },

        redo() {
            if (history.redo()) {
                markChanged();
            }
        },
    };
};

export type GraphHistory = ReturnType<typeof createGraphHistory>;

/**
 * The ids of the nodes the draft on the server has, so a save can tell which nodes were removed, and which removed
 * ones were brought back (by undo).
 */
export const createServerNodeIds = () => {
    let ids = new Set<string>();

    return {
        /**
         * Replaces the ids, after loading or saving the draft.
         */
        reset(nodeIds: Iterable<string>) {
            ids = new Set(nodeIds);
        },

        /**
         * Records a node the server just created.
         */
        add(id: string) {
            ids.add(id);
        },

        /**
         * The nodes of the server draft that the graph no longer has.
         */
        removed(currentIds: Iterable<string>) {
            const current = new Set(currentIds);

            return [...ids].filter((id) => !current.has(id));
        },

        /**
         * The nodes of the graph that the server draft doesn't have.
         */
        restored(currentIds: Iterable<string>) {
            return [...currentIds].filter((id) => !ids.has(id));
        },
    };
};
