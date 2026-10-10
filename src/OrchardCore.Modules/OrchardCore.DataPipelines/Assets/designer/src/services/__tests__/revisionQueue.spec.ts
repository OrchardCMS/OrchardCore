import { describe, expect, it } from "vitest";
import { flushPromises } from "@vue/test-utils";
import { createRevisionQueue } from "../revisionQueue";
import { createDesignerStore } from "../../state/designerStore";

describe("revisionQueue", () => {
    it("run_TwoRequests_RunsThemOneAtATimeWithTheCurrentRevision", async () => {
        const store = createDesignerStore();
        store.state.revision = 3;
        const queue = createRevisionQueue(store);
        const seen: number[] = [];
        let release!: () => void;

        const first = queue.run(async (revision) => {
            seen.push(revision);
            await new Promise<void>((resolve) => (release = resolve));

            return { revision: revision + 1 };
        });
        const second = queue.run(async (revision) => {
            seen.push(revision);

            return { revision: revision + 1 };
        });
        await flushPromises();

        expect(seen).toEqual([3]);
        expect(queue.busy).toBe(true);

        release();
        await first;
        await second;

        // The second request started with the revision the first one returned.
        expect(seen).toEqual([3, 4]);
        expect(store.state.revision).toBe(5);
        expect(queue.busy).toBe(false);
    });

    it("run_RequestFails_RunsTheNextOneAndKeepsTheRevision", async () => {
        const store = createDesignerStore();
        store.state.revision = 2;
        const queue = createRevisionQueue(store);

        const failed = queue.run(() => Promise.reject(new Error("offline")));
        const next = queue.run((revision) => Promise.resolve({ revision: revision + 1, valid: true }));

        await expect(failed).rejects.toThrow("offline");
        await expect(next).resolves.toEqual({ revision: 3, valid: true });
        expect(store.state.revision).toBe(3);
    });
});
