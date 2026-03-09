import * as SignalR from "@microsoft/signalr";
import dbg from "debug";
import { signalRLogger, signalRReceivedData } from "./eventbus";

const debug = dbg("orchardcore:bloom:signalr");

export interface SignalRAppOptions {
    isTokenRequired?: boolean;
    getToken?: () => string;
    transportType?: "ws" | "lp" | "sse";
    skipNegotiation?: boolean;
    url: string;
}

export default class SignalRApp {
    public connection: SignalR.HubConnection | null;

    private readonly _url: string;
    private _processResponse: ((input: unknown) => void) | null;

    constructor(url: string) {
        this._url = url;
        this.connection = null;
        this._processResponse = null;
    }

    public init(options: SignalRAppOptions): void {
        const configuration: SignalR.IHttpConnectionOptions = {};

        if (options.isTokenRequired && options.getToken) {
            configuration.accessTokenFactory = options.getToken;
        }

        switch (options.transportType) {
            case "lp":
                configuration.transport = SignalR.HttpTransportType.LongPolling;
                break;
            case "sse":
                configuration.transport = SignalR.HttpTransportType.ServerSentEvents;
                break;
            default:
                configuration.transport = SignalR.HttpTransportType.WebSockets;
                break;
        }

        if (options.skipNegotiation !== undefined) {
            configuration.skipNegotiation = options.skipNegotiation;
        }

        this.connection = new SignalR.HubConnectionBuilder()
            .withUrl(options.url, configuration)
            .configureLogging(SignalR.LogLevel.Information)
            .withAutomaticReconnect([0, 3000, 5000, 10000, 15000, 30000])
            .build();

        // Intercept incoming data for logging. The internal method name changed
        // from `processIncomingData` (v7) to `_processIncomingData` (v10+).
        const conn = this.connection as any; // eslint-disable-line @typescript-eslint/no-explicit-any
        const methodName = typeof conn.processIncomingData === "function"
            ? "processIncomingData"
            : typeof conn._processIncomingData === "function"
                ? "_processIncomingData"
                : null;

        if (methodName) {
            this._processResponse = conn[methodName].bind(this.connection);
            conn[methodName] = (data: unknown) => {
                this._processResponse?.(data);
                this.handleResponse(data);
            };
        }

        this.connection.onreconnecting((error: Error | undefined) => {
            signalRLogger.emit(`Connection lost due to error \"${error}\". Reconnecting.`);
            debug("On reconnecting", error);
        });

        this.connection.onreconnected((connectionId: string | undefined) => {
            signalRLogger.emit("Reconnected successfully");
            debug("On reconnected", connectionId);
        });
    }

    public onConnect(onSuccess: (data: unknown) => void, onError: (err: unknown) => void): void {
        if (!this.connection) {
            onError("SignalR connection is not initialized.");
            return;
        }

        this.connection
            .start()
            .then(() => onSuccess({ url: this._url }))
            .catch((err: unknown) => onError(err));
    }

    public onDisconnect(onSuccess: () => void, onError: (err: unknown) => void): void {
        if (!this.connection) {
            onSuccess();
            return;
        }

        this.connection
            .stop()
            .then(() => onSuccess())
            .catch((err: unknown) => onError(err));
    }

    private handleResponse(input: unknown): void {
        if (input === null || input === undefined) {
            return;
        }

        signalRLogger.emit(input.toString());

        const output = this.parseResponse(input);
        if (!output) {
            return;
        }

        output.forEach((item) => {
            const jsonObj = JSON.parse(item);
            if (jsonObj && Object.prototype.hasOwnProperty.call(jsonObj, "target")) {
                signalRReceivedData.emit({ ClientMethod: jsonObj.target, Data: jsonObj.arguments });
            }
        });
    }

    private parseResponse(input: unknown): string[] | null {
        if (typeof input !== "string") {
            debug("Invalid input for JSON hub protocol. Expected a string.", input);
            return null;
        }

        const separator = String.fromCharCode(0x1e);
        if (input[input.length - 1] !== separator) {
            debug("Message is incomplete", input);
            return null;
        }

        const messages = input.split(separator);
        messages.pop();
        return messages;
    }
}
