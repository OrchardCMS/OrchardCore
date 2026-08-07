import SignalRApp from "@bloom/services/signalr/signalr-app";
import { signalRReceivedData } from "@bloom/services/signalr/eventbus";
import { applyMediaChange, type IMediaChangedMessage } from "./applyMediaChange";
import { useFileLibraryManager } from "./FileLibraryManager";
import { useGlobals } from "./Globals";
import { getAccessToken, isAuthConfigured } from "./media-gallery-auth";
import { useRuntimeConfig } from "./RuntimeConfig";

/**
 * Connects to the OrchardCore MediaHub and listens for MediaChanged events.
 * When a media change is detected, it refreshes the store.
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

  if (app.connection) {
    app.connection.on("MediaChanged", async (message: unknown) => {
      console.debug("MediaChanged event received", message);

      // The payload carries the affected entry, so the store can usually be patched in place. Only fall
      // back to a full reload when it cannot be — otherwise every client reloads the directory on every
      // change, which costs a listing (plus a HasChildren probe per subfolder) per connected client.
      if (applyMediaChange(message as IMediaChangedMessage)) {
        return;
      }

      await loadDirectoryFiles(selectedDirectory.value?.directoryPath ?? "", true);
    });
  }

  signalRReceivedData.on((data: unknown) => {
    console.debug("SignalR received data:", data);
  });

  app.onConnect(
    () => {
      console.debug("SignalR connected to MediaHub");
    },
    (err: unknown) => {
      console.error("SignalR connection error:", err);
    }
  );
}
