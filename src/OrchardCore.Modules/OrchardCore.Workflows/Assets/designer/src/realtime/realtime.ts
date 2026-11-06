// Live updates from the workflows hub (OrchardCore.Workflows.SignalR). The page loads the SignalR client with the
// "signalr" resource, which exposes it as window.signalR; real time is optional, so every failure here is quiet.

export interface Presence {
    connectionId: string;
    userId?: string | null;
    userName?: string | null;
}

// A change of the workflow type (SignalRWorkflowDesignerNotifier).
export interface WorkflowTypeChangedMessage {
    kind: "DraftChanged" | "Published" | "DraftDiscarded" | string;
    workflowTypeId: string;
    revision: number;
    versionId?: string | null;
    userId?: string | null;
    userName?: string | null;
}

// A change of a workflow instance.
export interface InstanceChangedMessage {
    workflowId: string;
    workflowTypeId: string;
    status: string;
    isDeleted: boolean;
}

// The part of the @microsoft/signalr connection the designer uses.
export interface HubConnectionLike {
    start(): Promise<void>;
    stop(): Promise<void>;
    invoke(methodName: string, ...args: unknown[]): Promise<unknown>;
    on(methodName: string, handler: (...args: never[]) => void): void;
    onreconnected(callback: (connectionId?: string) => void): void;
}

interface HubConnectionBuilderLike {
    withUrl(url: string): HubConnectionBuilderLike;
    withAutomaticReconnect(): HubConnectionBuilderLike;
    build(): HubConnectionLike;
}

declare global {
    interface Window {
        signalR?: { HubConnectionBuilder: new () => HubConnectionBuilderLike };
    }
}

export type ConnectionFactory = (url: string) => HubConnectionLike | null;

const defaultFactory: ConnectionFactory = (url) => (window.signalR ? new window.signalR.HubConnectionBuilder().withUrl(url).withAutomaticReconnect().build() : null);

export interface RealtimeOptions {
    url: string;
    // The workflow type whose changes and presence the designer follows.
    workflowTypeId?: string | null;
    // The instance the viewer follows.
    workflowId?: string | null;
    onWorkflowTypeChanged?: (message: WorkflowTypeChangedMessage) => void;
    onInstanceChanged?: (message: InstanceChangedMessage) => void;
    onPresenceChanged?: (presence: Presence[]) => void;
    factory?: ConnectionFactory;
}

export interface RealtimeSession {
    stop(): Promise<void>;
}

/**
 * Connects to the hub and subscribes to a workflow type or an instance. Resolves to null when real time isn't
 * available (no SignalR client, or the connection failed).
 */
export const startRealtime = async (options: RealtimeOptions): Promise<RealtimeSession | null> => {
    const connection = (options.factory ?? defaultFactory)(options.url);

    if (!connection) {
        return null;
    }

    // The others who have the workflow type open, by connection.
    const others = new Map<string, Presence>();
    const publishPresence = () => options.onPresenceChanged?.([...others.values()]);

    connection.on("WorkflowTypeChanged", (message: WorkflowTypeChangedMessage) => options.onWorkflowTypeChanged?.(message));
    connection.on("InstanceChanged", (message: InstanceChangedMessage) => options.onInstanceChanged?.(message));

    // Someone arrived: they learn who is here from each of the others.
    connection.on("PresenceJoined", (presence: Presence) => {
        others.set(presence.connectionId, presence);
        publishPresence();
        connection.invoke("AnnouncePresence", options.workflowTypeId, presence.connectionId).catch(() => undefined);
    });

    connection.on("PresenceHere", (presence: Presence) => {
        others.set(presence.connectionId, presence);
        publishPresence();
    });

    connection.on("PresenceLeft", (connectionId: string) => {
        if (others.delete(connectionId)) {
            publishPresence();
        }
    });

    const subscribe = async () => {
        if (options.workflowTypeId) {
            await connection.invoke("SubscribeWorkflowType", options.workflowTypeId);
        }

        if (options.workflowId) {
            await connection.invoke("SubscribeInstance", options.workflowId);
        }
    };

    // A reconnected connection has a new id and lost its groups: subscribe again, and the others announce
    // themselves again.
    connection.onreconnected(() => {
        others.clear();
        publishPresence();
        subscribe().catch(() => undefined);
    });

    try {
        await connection.start();
        await subscribe();
    } catch {
        return null;
    }

    return {
        stop: () => connection.stop().catch(() => undefined),
    };
};
