import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { flushPromises } from "@vue/test-utils";
import { nextTick } from "vue";
import { createAutosave } from "../autosave";
import { createRevisionQueue } from "../revisionQueue";
import { createDesignerStore } from "../../state/designerStore";
import { moveNodesCommand } from "../../state/commands";
import { DesignerApiError } from "../../api/designerApi";
import type { SavePayload, SaveResult } from "../../api/types";
import { createDefinition } from "../../canvas/__tests__/fixtures";

const deferred = <T>() => {
    let resolve!: (value: T) => void;
    let reject!: (error: unknown) => void;
    const promise = new Promise<T>((res, rej) => {
        resolve = res;
        reject = rej;
    });

    return { promise, resolve, reject };
};

const setup = (save: (payload: SavePayload) => Promise<SaveResult>) => {
    const store = createDesignerStore();
    store.loadDefinition({ ...createDefinition(), revision: 4, hasDraft: true });
    const onConflict = vi.fn();
    const onSaved = vi.fn();
    const saveMock = vi.fn(save);
    const autosave = createAutosave({ store, queue: createRevisionQueue(store), save: saveMock, onConflict, onSaved });
    autosave.start();

    return { store, autosave, save: saveMock, onConflict, onSaved };
};

// Moves activity "a", which changes the graph.
const move = async (store: ReturnType<typeof setup>["store"], x: number) => {
    const node = store.getNode("a")!;
    store.execute(moveNodesCommand(store.graph, [{ id: "a", fromX: node.x, fromY: node.y, toX: x, toY: node.y }]));
    await nextTick();
};

const advance = async (ms: number) => {
    vi.advanceTimersByTime(ms);
    await flushPromises();
};

describe("autosave", () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    it("change_SeveralChangesInARow_SavesOnceAfterTheDebounce", async () => {
        const { store, save } = setup((payload) => Promise.resolve({ revision: payload.revision + 1, issues: [] }));

        await move(store, 610);
        await advance(500);
        await move(store, 620);
        await move(store, 630);

        expect(store.state.saveStatus).toBe("unsaved");
        await advance(799);
        expect(save).not.toHaveBeenCalled();

        await advance(1);

        expect(save).toHaveBeenCalledTimes(1);
        expect(save.mock.calls[0][0].revision).toBe(4);
        expect(save.mock.calls[0][0].nodes.find((node) => node.id === "a")?.x).toBe(630);
        expect(store.state.saveStatus).toBe("saved");
        expect(store.state.revision).toBe(5);
    });

    it("change_WhileSaving_KeepsOneRequestInFlightAndSavesTheRestTogether", async () => {
        const first = deferred<SaveResult>();
        const { store, save } = setup((payload) => (payload.revision === 4 ? first.promise : Promise.resolve({ revision: payload.revision + 1, issues: [] })));

        await move(store, 610);
        await advance(800);
        expect(save).toHaveBeenCalledTimes(1);
        expect(store.state.saveStatus).toBe("saving");

        await move(store, 620);
        await move(store, 630);
        await advance(5000);
        expect(save).toHaveBeenCalledTimes(1);

        first.resolve({ revision: 5, issues: [] });
        await flushPromises();
        expect(store.state.saveStatus).toBe("unsaved");

        await advance(800);

        expect(save).toHaveBeenCalledTimes(2);
        expect(save.mock.calls[1][0].revision).toBe(5);
        expect(save.mock.calls[1][0].nodes.find((node) => node.id === "a")?.x).toBe(630);
        expect(store.state.saveStatus).toBe("saved");
    });

    it("save_NetworkErrors_RetriesWithBackoffUntilItSaves", async () => {
        let calls = 0;
        const { store, save } = setup((payload) =>
            ++calls <= 3 ? Promise.reject(new DesignerApiError(0, undefined, "offline")) : Promise.resolve({ revision: payload.revision + 1, issues: [] }),
        );

        await move(store, 610);
        await advance(800);
        expect(save).toHaveBeenCalledTimes(1);
        expect(store.state.saveStatus).toBe("offline");

        // Retries after 1 s, 2 s, then 5 s.
        for (const [wait, expected] of [
            [1000, 2],
            [2000, 3],
            [5000, 4],
        ]) {
            await advance(wait - 1);
            expect(save).toHaveBeenCalledTimes(expected - 1);
            await advance(1);
            expect(save).toHaveBeenCalledTimes(expected);
        }

        expect(store.state.saveStatus).toBe("saved");
        expect(store.state.revision).toBe(5);
    });

    it("save_Conflict_StopsAutosavingUntilOverwritten", async () => {
        let calls = 0;
        const { store, save, autosave, onConflict, onSaved } = setup((payload) =>
            ++calls === 1
                ? Promise.reject(new DesignerApiError(409, { currentRevision: 9, modifiedBy: "bob" }))
                : Promise.resolve({ revision: payload.revision + 1, issues: [] }),
        );

        await move(store, 610);
        await advance(800);

        expect(store.state.saveStatus).toBe("conflict");
        expect(onConflict).toHaveBeenCalledTimes(1);
        expect(autosave.conflict.value?.modifiedBy).toBe("bob");

        await move(store, 620);
        await advance(60_000);
        expect(save).toHaveBeenCalledTimes(1);

        // Overwrite saves the graph with the draft's current revision.
        await autosave.overwrite();

        expect(save).toHaveBeenCalledTimes(2);
        expect(save.mock.calls[1][0].revision).toBe(9);
        expect(store.state.revision).toBe(10);
        expect(store.state.saveStatus).toBe("saved");
        expect(autosave.conflict.value).toBeNull();
        expect(onSaved).toHaveBeenCalledTimes(1);
    });

    it("save_ConflictThenReloaded_AutosavesAgain", async () => {
        let calls = 0;
        const { store, save, autosave } = setup((payload) =>
            ++calls === 1 ? Promise.reject(new DesignerApiError(409, { currentRevision: 9 })) : Promise.resolve({ revision: payload.revision + 1, issues: [] }),
        );

        await move(store, 610);
        await advance(800);
        expect(store.state.saveStatus).toBe("conflict");

        // Reload: the designer loads the definition again, then resets autosave.
        store.loadDefinition({ ...createDefinition(), revision: 9, hasDraft: true });
        autosave.reset();
        expect(store.state.saveStatus).toBe("saved");

        await move(store, 640);
        await advance(800);

        expect(save).toHaveBeenCalledTimes(2);
        expect(save.mock.calls[1][0].revision).toBe(9);
        expect(store.state.saveStatus).toBe("saved");
    });

    it("save_RejectedByTheServer_IsNotRetriedUntilAsked", async () => {
        let calls = 0;
        const { store, save, autosave } = setup((payload) =>
            ++calls === 1 ? Promise.reject(new DesignerApiError(400, { title: "Bad request" })) : Promise.resolve({ revision: payload.revision + 1, issues: [] }),
        );

        await move(store, 610);
        await advance(800);
        expect(store.state.saveStatus).toBe("failed");

        await advance(60_000);
        expect(save).toHaveBeenCalledTimes(1);

        await autosave.save();

        expect(save).toHaveBeenCalledTimes(2);
        expect(store.state.saveStatus).toBe("saved");
    });

    it("flush_PendingChanges_SavesThemWithoutWaitingForTheDebounce", async () => {
        const { store, save, autosave } = setup((payload) => Promise.resolve({ revision: payload.revision + 1, issues: [] }));

        await move(store, 610);

        await expect(autosave.flush()).resolves.toBe(true);
        expect(save).toHaveBeenCalledTimes(1);
        expect(autosave.hasUnsavedChanges()).toBe(false);

        // Nothing is left for the debounce to save.
        await advance(800);
        expect(save).toHaveBeenCalledTimes(1);
    });

    it("flush_Conflict_ResolvesToFalse", async () => {
        const { store, autosave } = setup(() => Promise.reject(new DesignerApiError(409, { currentRevision: 9 })));

        await move(store, 610);

        await expect(autosave.flush()).resolves.toBe(false);
        expect(autosave.hasUnsavedChanges()).toBe(true);
    });

    it("pause_ChangesWhilePaused_AreSavedWhenResumed", async () => {
        const { store, save, autosave } = setup((payload) => Promise.resolve({ revision: payload.revision + 1, issues: [] }));

        autosave.pause();
        await move(store, 610);
        await advance(5000);
        expect(save).not.toHaveBeenCalled();
        expect(autosave.hasUnsavedChanges()).toBe(true);

        autosave.resume();
        await advance(800);

        expect(save).toHaveBeenCalledTimes(1);
        expect(store.state.saveStatus).toBe("saved");
    });
});
