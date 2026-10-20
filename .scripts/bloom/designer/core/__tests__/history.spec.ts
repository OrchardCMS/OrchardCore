import { describe, expect, it, vi } from "vitest";
import { History, HISTORY_LIMIT, type Command } from "../history";

const counterCommand = (counter: { value: number }, delta: number, coalesceKey?: string): Command => ({
    label: "count",
    coalesceKey,
    apply: () => {
        counter.value += delta;
    },
    revert: () => {
        counter.value -= delta;
    },
});

// Sets a position; the steps of one drag absorb each other, keeping the first position to revert to.
const moveCommand = (target: { x: number }, from: number, to: number, coalesceKey: string): Command & { to: number } => {
    const command = {
        label: "move",
        coalesceKey,
        to,
        apply: () => {
            target.x = command.to;
        },
        revert: () => {
            target.x = from;
        },
        absorb: (next: Command) => {
            command.to = (next as Command & { to: number }).to;

            return true;
        },
    };

    return command;
};

describe("History", () => {
    it("undoRedo_ExecutedCommands_RevertAndReapplyInOrder", () => {
        const counter = { value: 0 };
        const history = new History();

        history.execute(counterCommand(counter, 1));
        history.execute(counterCommand(counter, 10));

        history.undo();
        expect(counter.value).toBe(1);
        history.undo();
        expect(counter.value).toBe(0);
        expect(history.canUndo).toBe(false);

        history.redo();
        history.redo();
        expect(counter.value).toBe(11);
        expect(history.canRedo).toBe(false);
    });

    it("execute_AfterUndo_ClearsRedo", () => {
        const counter = { value: 0 };
        const history = new History();

        history.execute(counterCommand(counter, 1));
        history.undo();
        history.execute(counterCommand(counter, 5));

        expect(history.canRedo).toBe(false);
        expect(counter.value).toBe(5);
    });

    it("record_DragSteps_CoalesceIntoOneEntry", () => {
        const target = { x: 10 };
        const history = new History();

        for (const x of [20, 30, 40]) {
            const command = moveCommand(target, target.x, x, "drag-1");
            command.apply();
            history.record(command);
        }

        expect(history.size).toBe(1);
        expect(target.x).toBe(40);

        history.undo();
        expect(target.x).toBe(10);

        history.redo();
        expect(target.x).toBe(40);
    });

    it("record_DifferentDrags_StaySeparate", () => {
        const target = { x: 0 };
        const history = new History();

        history.execute(moveCommand(target, 0, 10, "drag-1"));
        history.execute(moveCommand(target, 10, 20, "drag-2"));

        expect(history.size).toBe(2);
        history.undo();
        expect(target.x).toBe(10);
    });

    it("execute_MoreThanLimit_DropsOldestEntries", () => {
        const counter = { value: 0 };
        const history = new History();

        for (let i = 0; i < HISTORY_LIMIT + 20; i++) {
            history.execute(counterCommand(counter, 1));
        }

        expect(history.size).toBe(HISTORY_LIMIT);

        while (history.canUndo) {
            history.undo();
        }

        expect(counter.value).toBe(20);
    });

    it("markBoundary_AfterCommands_PreventsUndoingPastIt", () => {
        const counter = { value: 0 };
        const history = new History();

        history.execute(counterCommand(counter, 1));
        history.markBoundary();
        history.execute(counterCommand(counter, 2));

        history.undo();
        expect(counter.value).toBe(1);
        expect(history.canUndo).toBe(false);
    });

    it("subscribe_Changes_NotifiesListeners", () => {
        const history = new History();
        const listener = vi.fn();
        const unsubscribe = history.subscribe(listener);

        history.execute(counterCommand({ value: 0 }, 1));
        history.undo();
        unsubscribe();
        history.redo();

        expect(listener).toHaveBeenCalledTimes(2);
    });
});
