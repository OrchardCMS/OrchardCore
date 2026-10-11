import { describe, expect, it, vi } from "vitest";
import { History, HISTORY_LIMIT, type Command } from "../history";
import { moveNodesCommand, type Graph } from "../commands";
import { createNode } from "../../__tests__/fixtures";

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

const node = (id: string, x = 0, y = 0) => createNode(id, { x, y });

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
        const graph: Graph = { nodes: [node("a", 10, 10)], connections: [] };
        const history = new History();

        // Each drag step moves the node live and records the move; the steps share the drag's key.
        for (const [x, y] of [[20, 20], [30, 25], [40, 30]]) {
            const current = graph.nodes[0];
            const command = moveNodesCommand(graph, [{ id: "a", fromX: current.x, fromY: current.y, toX: x, toY: y }], "drag-1");
            command.apply();
            history.record(command);
        }

        expect(history.size).toBe(1);
        expect(graph.nodes[0]).toMatchObject({ x: 40, y: 30 });

        history.undo();
        expect(graph.nodes[0]).toMatchObject({ x: 10, y: 10 });

        history.redo();
        expect(graph.nodes[0]).toMatchObject({ x: 40, y: 30 });
    });

    it("record_DifferentDrags_StaySeparate", () => {
        const graph: Graph = { nodes: [node("a")], connections: [] };
        const history = new History();

        history.execute(moveNodesCommand(graph, [{ id: "a", fromX: 0, fromY: 0, toX: 10, toY: 0 }], "drag-1"));
        history.execute(moveNodesCommand(graph, [{ id: "a", fromX: 10, fromY: 0, toX: 20, toY: 0 }], "drag-2"));

        expect(history.size).toBe(2);
        history.undo();
        expect(graph.nodes[0].x).toBe(10);
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
