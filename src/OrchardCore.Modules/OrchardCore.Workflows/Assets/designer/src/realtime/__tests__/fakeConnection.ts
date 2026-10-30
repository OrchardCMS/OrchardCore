import { vi } from "vitest";
import type { HubConnectionLike } from "../realtime";

type Handler = (...args: unknown[]) => void;

// A connection that records what the designer invokes, and lets the test raise the hub's messages.
export const createFakeConnection = () => {
    const handlers = new Map<string, Handler>();
    let reconnected: (() => void) | null = null;
    const connection = {
        start: vi.fn(() => Promise.resolve()),
        stop: vi.fn(() => Promise.resolve()),
        invoke: vi.fn(() => Promise.resolve(true)),
        on: vi.fn((name: string, handler: Handler) => handlers.set(name, handler)),
        onreconnected: vi.fn((callback: () => void) => {
            reconnected = callback;
        }),
    };

    return {
        connection: connection as unknown as HubConnectionLike & typeof connection,
        raise: (name: string, ...args: unknown[]) => handlers.get(name)?.(...args),
        reconnect: () => reconnected?.(),
    };
};
