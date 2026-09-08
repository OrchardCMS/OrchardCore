import mitt from "mitt";

declare global {
    interface Window {
        signalREventBus: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    }
}

let signalREventBus = window.signalREventBus;

if (!signalREventBus) {
    signalREventBus = mitt();
    window.signalREventBus = signalREventBus;
}

function wrapEvent(name: string) {
    return {
        emit: (obj: any) => signalREventBus.emit(name, obj), // eslint-disable-line @typescript-eslint/no-explicit-any
        on: (handler: any) => signalREventBus.on(name, handler), // eslint-disable-line @typescript-eslint/no-explicit-any
        off: (handler: any) => signalREventBus.off(name, handler), // eslint-disable-line @typescript-eslint/no-explicit-any
    };
}

function wrapNamedEvent(name: string) {
    return {
        emit: (postfix: any, obj: any) => signalREventBus.emit(`${name}-${postfix}`, obj), // eslint-disable-line @typescript-eslint/no-explicit-any
        on: (postfix: any, handler: any) => signalREventBus.on(`${name}-${postfix}`, handler), // eslint-disable-line @typescript-eslint/no-explicit-any
        off: (postfix: any, handler: any) => signalREventBus.off(`${name}-${postfix}`, handler), // eslint-disable-line @typescript-eslint/no-explicit-any
    };
}

export const signalRBus = signalREventBus;
export const signalRLogger = wrapEvent("signalr-logger");
export const signalRReceivedData = wrapEvent("signalr-received-data");
export const connectionSuccess = wrapEvent("signalr-connection-success");
export const connectionFailed = wrapEvent("signalr-connection-failed");
export const transientNotification = wrapEvent("transient-notification");

export const entityCreated = wrapNamedEvent("hub-entity-created");
export const entityUpdated = wrapNamedEvent("hub-entity-updated");
export const entityDeleted = wrapNamedEvent("hub-entity-deleted");
