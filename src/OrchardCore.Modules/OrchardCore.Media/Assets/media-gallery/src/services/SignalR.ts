import SignalRApp from "@bloom/services/signalr/signalr-app";
import { signalRReceivedData } from "@bloom/services/signalr/eventbus";
import { HubConnectionState } from "@microsoft/signalr";
import { getCurrentScope, onScopeDispose, watch } from "vue";
import { useFileLibraryManager } from "./FileLibraryManager";
import { useGlobals } from "./Globals";
import { getAccessToken, isAuthConfigured } from "./media-gallery-auth";
import { useRuntimeConfig } from "./RuntimeConfig";

/**
 * Connects to the OrchardCore MediaHub and listens for MediaChanged events.
 * When a media change is detected, it refreshes the store.
 *
 * Folder-level authorization is enforced server-side: the client calls SubscribePath for the
 * folder it is currently viewing and the Hub only adds the connection to the corresponding
 * SignalR group when the user has ManageMediaFolder permission for that path. This ensures that
 * notifications (which include the affected path) are only delivered to authorized clients, and
 * that it works correctly when a SignalR backplane is configured.
 */
export function useSignalR() {
  const { loadDirectoryFiles } = useFileLibraryManager();
  const { selectedDirectory } = useGlobals();
  const hubUrl = useRuntimeConfig().hubUrl;
  const app = new SignalRApp(hubUrl);

  // In bearer mode, hand the MediaHub a silently-acquired access token. This async factory is
  // invoked by SignalR on connect and on every reconnect, so it returns a freshly renewed token
  // (avoiding the connect-time race and re-authenticating after expiry when the connection drops).
  app.init({
    url: hubUrl,
    isTokenRequired: isAuthConfigured(),
    getToken: async () => (await getAccessToken()) ?? "",
    // Bearer mode authenticates with the token alone; the SignalR client's default
    // credentials:include would make the cross-origin negotiate fail against the
    // credential-less CORS policy. Cookie mode keeps the default so the admin cookie flows.
    ...(isAuthConfigured() ? { withCredentials: false } : {}),
  });

  // Folder navigation can happen before the initial connection finishes negotiating, or during
  // the gap while an automatic reconnect is in progress. HubConnection.invoke() throws
  // synchronously outside the Connected state, so guard on connection.state first rather than
  // relying on the promise rejection — that avoided throw would otherwise still spam the console
  // on every folder switch during that window. It's safe to just skip: onConnect / onreconnected
  // below always (re)subscribe to whatever folder is CURRENTLY selected once the connection is
  // actually up, so no subscription is permanently lost — it's just deferred to that point.
  const subscribePath = (path: string) => {
    if (app.connection?.state !== HubConnectionState.Connected) {
      return;
    }
    app.connection.invoke("SubscribePath", path).catch((err: unknown) => {
      console.error("SignalR SubscribePath failed:", err);
    });
  };

  const unsubscribePath = (path: string) => {
    if (app.connection?.state !== HubConnectionState.Connected) {
      return;
    }
    app.connection.invoke("UnsubscribePath", path).catch((err: unknown) => {
      console.error("SignalR UnsubscribePath failed:", err);
    });
  };

  if (app.connection) {
    app.connection.on("MediaChanged", async (message: unknown) => {
      console.debug("MediaChanged event received", message);
      await loadDirectoryFiles(selectedDirectory.value?.directoryPath ?? "", true);
    });

    // Re-subscribe to the current folder after an automatic reconnect, because SignalR group
    // membership is not preserved across reconnections.
    app.connection.onreconnected(() => {
      console.debug("SignalR reconnected to MediaHub, re-subscribing to current folder");
      subscribePath(selectedDirectory.value?.directoryPath ?? "");
    });
  }

  const onReceivedData = (data: unknown) => {
    console.debug("SignalR received data:", data);
  };
  signalRReceivedData.on(onReceivedData);

  // The picker mounts a fresh app per modal open, so the hub connection and the
  // module-level event-bus handler must be torn down with the component scope —
  // otherwise every open leaks a live connection. On the admin page the app
  // lives for the whole page, so this never fires there.
  if (getCurrentScope()) {
    onScopeDispose(() => {
      signalRReceivedData.off(onReceivedData);
      app.onDisconnect(
        () => console.debug("SignalR disconnected from MediaHub"),
        (err: unknown) => console.error("SignalR disconnect error:", err),
      );
    });
  }

  // Watch the selected directory and update group subscriptions when the user navigates.
  watch(selectedDirectory, (newDir, oldDir) => {
    const newPath = newDir?.directoryPath ?? "";
    const oldPath = oldDir?.directoryPath ?? "";

    if (newPath === oldPath) {
      return;
    }

    unsubscribePath(oldPath);
    subscribePath(newPath);
  });

  app.onConnect(
    () => {
      console.debug("SignalR connected to MediaHub");
      subscribePath(selectedDirectory.value?.directoryPath ?? "");
    },
    (err: unknown) => {
      console.error("SignalR connection error:", err);
    }
  );
}
