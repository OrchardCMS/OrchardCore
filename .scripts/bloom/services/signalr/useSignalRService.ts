import dbg from "debug";
import SignalRApp, { SignalRAppOptions } from "./signalr-app";
import {
    connectionFailed,
    connectionSuccess,
    entityCreated,
    entityDeleted,
    entityUpdated,
    signalRLogger,
    transientNotification,
} from "./eventbus";

const debug = dbg("orchardcore:bloom:signalr");

export interface SignalRConnectionBootstrap {
    apiEndpoint: string;
    token: string;
}

export interface UseSignalROptions {
    siteId: string;
    getConnection: () => Promise<SignalRConnectionBootstrap>;
}

export const useSignalRService = (url: string, options: UseSignalROptions) => {
    return {
        signalRService: new SignalRService(url, options),
    };
};

class SignalRService {
    private readonly _url: string;
    private readonly _siteId: string;
    private readonly _getConnection: () => Promise<SignalRConnectionBootstrap>;
    private readonly _signalR: SignalRApp;
    private _connectionData: SignalRConnectionBootstrap | null;

    constructor(url: string, options: UseSignalROptions) {
        this._url = url;
        this._siteId = options.siteId;
        this._getConnection = options.getConnection;
        this._signalR = new SignalRApp(url);
        this._connectionData = null;
    }

    public async start(): Promise<void> {
        const localStorageName = `signalr-${this._siteId}`;
        const lsEntry = localStorage.getItem(localStorageName);

        if (lsEntry) {
            try {
                this._connectionData = JSON.parse(lsEntry) as SignalRConnectionBootstrap;
            }
            catch {
                this._connectionData = null;
            }
        }

        if (!this._connectionData) {
            this._connectionData = await this._getConnection();
            localStorage.setItem(localStorageName, JSON.stringify(this._connectionData));
        }

        this.connect();
    }

    private connect(): void {
        if (!this._connectionData) {
            debug("No connection data available for SignalR");
            return;
        }

        const connectionData = this._connectionData;

        const signalROptions: SignalRAppOptions = {
            url: connectionData.apiEndpoint,
            getToken: () => connectionData.token,
            isTokenRequired: true,
        };

        this._signalR.init(signalROptions);
        this.registerHubEvents();

        this._signalR.onConnect(
            (data) => {
                connectionSuccess.emit(data);
                signalRLogger.emit("Connection established successfully with the server");
            },
            (err) => {
                connectionFailed.emit(err);
                signalRLogger.emit(`Connection failed: ${err}`);
            });
    }

    private registerHubEvents(): void {
        if (!this._signalR.connection) {
            return;
        }

        this._signalR.connection.on("TransientNotification", (payload: unknown) => transientNotification.emit(payload));

        this._signalR.connection.on("EntityCreated", (payload: { type?: string } & Record<string, unknown>) => {
            if (payload?.type) {
                entityCreated.emit(payload.type, payload);
            }
        });

        this._signalR.connection.on("EntityUpdated", (payload: { type?: string } & Record<string, unknown>) => {
            if (payload?.type) {
                entityUpdated.emit(payload.type, payload);
            }
        });

        this._signalR.connection.on("EntityDeleted", (payload: { type?: string } & Record<string, unknown>) => {
            if (payload?.type) {
                entityDeleted.emit(payload.type, payload);
            }
        });
    }
}
