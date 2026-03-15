import SignalRApp from "@bloom/services/signalr/signalr-app";
import { signalRReceivedData } from "@bloom/services/signalr/eventbus";
import { useFileLibraryManager } from "./FileLibraryManager";
import { useGlobals } from "./Globals";

/**
 * Connects to the OrchardCore MediaHub and listens for MediaChanged events.
 * When a media change is detected, it refreshes the store.
 */
export function useSignalR() {
  const { loadDirectoryFiles } = useFileLibraryManager();
  const { selectedDirectory } = useGlobals();
  const app = new SignalRApp("/hubs/media");

  app.init({
    url: "/hubs/media",
  });

  if (app.connection) {
    app.connection.on("MediaChanged", async (message: unknown) => {
      console.debug("MediaChanged event received", message);
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
