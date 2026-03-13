import SignalRApp from "@bloom/services/signalr/signalr-app";
import { signalRReceivedData } from "@bloom/services/signalr/eventbus";
import { useFileLibraryManager } from "./FileLibraryManager";

const { getFileLibraryStoreAsync } = useFileLibraryManager();

/**
 * Connects to the OrchardCore MediaHub and listens for MediaChanged events.
 * When a media change is detected, it refreshes the store.
 */
export function useSignalR() {
  const app = new SignalRApp("/hubs/media");

  app.init({
    url: "/hubs/media",
  });

  if (app.connection) {
    app.connection.on("MediaChanged", async (message: unknown) => {
      console.debug("MediaChanged event received", message);
      await getFileLibraryStoreAsync();
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
