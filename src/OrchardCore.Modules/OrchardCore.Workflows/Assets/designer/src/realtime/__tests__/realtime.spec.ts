import { describe, expect, it, vi } from "vitest";
import { startRealtime } from "../realtime";
import { createFakeConnection } from "./fakeConnection";

describe("realtime", () => {
    it("start_WorkflowType_SubscribesAndTracksWhoIsThere", async () => {
        const fake = createFakeConnection();
        const presence = vi.fn();

        const session = await startRealtime({ url: "/hub", workflowTypeId: "type-1", onPresenceChanged: presence, factory: () => fake.connection });

        expect(session).not.toBeNull();
        expect(fake.connection.invoke).toHaveBeenCalledWith("SubscribeWorkflowType", "type-1");

        fake.raise("PresenceJoined", { connectionId: "c2", userName: "bob" });

        expect(fake.connection.invoke).toHaveBeenCalledWith("AnnouncePresence", "type-1", "c2");
        expect(presence).toHaveBeenLastCalledWith([{ connectionId: "c2", userName: "bob" }]);

        fake.raise("PresenceHere", { connectionId: "c3", userName: "carol" });
        fake.raise("PresenceLeft", "c2");

        expect(presence).toHaveBeenLastCalledWith([{ connectionId: "c3", userName: "carol" }]);
    });

    it("reconnect_SubscribesAgainAndForgetsWhoWasThere", async () => {
        const fake = createFakeConnection();
        const presence = vi.fn();
        await startRealtime({ url: "/hub", workflowId: "workflow-1", onPresenceChanged: presence, factory: () => fake.connection });

        fake.raise("PresenceHere", { connectionId: "c2" });
        fake.reconnect();
        await Promise.resolve();

        expect(presence).toHaveBeenLastCalledWith([]);
        expect(fake.connection.invoke).toHaveBeenCalledTimes(2);
        expect(fake.connection.invoke).toHaveBeenLastCalledWith("SubscribeInstance", "workflow-1");
    });

    it("messages_TypeAndInstanceChanges_AreForwarded", async () => {
        const fake = createFakeConnection();
        const typeChanged = vi.fn();
        const instanceChanged = vi.fn();
        await startRealtime({ url: "/hub", workflowTypeId: "type-1", onWorkflowTypeChanged: typeChanged, onInstanceChanged: instanceChanged, factory: () => fake.connection });

        fake.raise("WorkflowTypeChanged", { kind: "Published", workflowTypeId: "type-1", revision: 0 });
        fake.raise("InstanceChanged", { workflowId: "w", workflowTypeId: "type-1", status: "Finished", isDeleted: false });

        expect(typeChanged).toHaveBeenCalledWith(expect.objectContaining({ kind: "Published" }));
        expect(instanceChanged).toHaveBeenCalledWith(expect.objectContaining({ status: "Finished" }));
    });

    it("start_NoClientOrFailedConnection_ResolvesToNull", async () => {
        const fake = createFakeConnection();
        fake.connection.start.mockRejectedValueOnce(new Error("offline"));

        expect(await startRealtime({ url: "/hub", factory: () => null })).toBeNull();
        expect(await startRealtime({ url: "/hub", workflowTypeId: "type-1", factory: () => fake.connection })).toBeNull();
    });
});
