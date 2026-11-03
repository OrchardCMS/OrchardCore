import { describe, expect, it } from "vitest";
import { flushPromises } from "@vue/test-utils";
import { createRevisionQueue } from "../revisionQueue";

describe("revisionQueue", () => {
    it("run_TwoRequests_RunsThemOneAtATimeWithTheCurrentRevision", async () => {
        const store = { state: { revision: 3 } };
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

        expect(seen).toEqual([3, 4]);
        expect(store.state.revision).toBe(5);
        expect(queue.busy).toBe(false);
    });

    it("run_FailedRequest_RunsTheNextOne", async () => {
        const store = { state: { revision: 1 } };
        const queue = createRevisionQueue(store);

        const failed = queue.run(() => Promise.reject(new Error("Failed")));
        const next = queue.run((revision) => Promise.resolve({ revision: revision + 1 }));

        await expect(failed).rejects.toThrow("Failed");
        await expect(next).resolves.toEqual({ revision: 2 });
        expect(store.state.revision).toBe(2);
    });

    it("run_ResultWithoutRevision_KeepsTheRevision", async () => {
        const store = { state: { revision: 6 } };
        const queue = createRevisionQueue(store);

        await queue.run(() => Promise.resolve({}));

        expect(store.state.revision).toBe(6);
    });
});
