import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { nextTick, type Ref } from "vue";
import { useGlobals } from "../Globals";
import type { IFileLibraryItemDto } from "@bloom/media/interfaces";

// Track callbacks for testing
let onConnectSuccessCb: ((data: unknown) => void) | null = null;
let onConnectErrorCb: ((err: unknown) => void) | null = null;
let mediaChangedCb: ((message: unknown) => Promise<void>) | null = null;
let onReconnectedCb: (() => void) | null = null;
let signalRReceivedDataCb: ((data: unknown) => void) | null = null;

const mockGetFileLibraryStoreAsync = vi.fn(() => Promise.resolve([]));
const mockLoadDirectoryFiles = vi.fn(() => Promise.resolve(null));
const { setSelectedDirectory } = useGlobals();

const setDirectory = (directoryPath?: string) => {
  setSelectedDirectory({ directoryPath } as IFileLibraryItemDto);
};

const mockConnection = {
  on: vi.fn((event: string, cb: (...args: any[]) => any) => { // eslint-disable-line @typescript-eslint/no-explicit-any
    if (event === "MediaChanged") mediaChangedCb = cb;
  }),
  onreconnected: vi.fn((cb: () => void) => {
    onReconnectedCb = cb;
  }),
  invoke: vi.fn(() => Promise.resolve()),
  start: vi.fn(() => Promise.resolve()),
};

const mockApp = {
  init: vi.fn(),
  connection: mockConnection,
  onConnect: vi.fn((onSuccess: (data: unknown) => void, onError: (err: unknown) => void) => {
    onConnectSuccessCb = onSuccess;
    onConnectErrorCb = onError;
  }),
};

vi.mock("@bloom/services/signalr/signalr-app", () => ({
  default: vi.fn(function () { return mockApp; }),
}));

vi.mock("@bloom/services/signalr/eventbus", () => ({
  signalRReceivedData: {
    on: vi.fn((cb: (data: unknown) => void) => {
      signalRReceivedDataCb = cb;
    }),
    emit: vi.fn(),
    off: vi.fn(),
  },
}));

vi.mock("../FileLibraryManager", () => ({
  useFileLibraryManager: () => ({
    getFileLibraryStoreAsync: mockGetFileLibraryStoreAsync,
    loadDirectoryFiles: mockLoadDirectoryFiles,
  }),
}));

describe("SignalR", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    onConnectSuccessCb = null;
    onConnectErrorCb = null;
    mediaChangedCb = null;
    onReconnectedCb = null;
    signalRReceivedDataCb = null;
    setDirectory();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("creates a SignalRApp and initialises it", async () => {
    const SignalRApp = (await import("@bloom/services/signalr/signalr-app")).default;
    const { useSignalR } = await import("../SignalR");
    useSignalR();
    expect(SignalRApp).toHaveBeenCalledWith("/hubs/media");
    expect(mockApp.init).toHaveBeenCalledWith({ url: "/hubs/media", isTokenRequired: false, getToken: expect.any(Function) });
  });

  it("disables credentials on the hub connection in bearer mode", async () => {
    // Bearer authenticates with the token alone; the client's default credentials:include would
    // fail the cross-origin negotiate against the credential-less CORS policy (standalone app).
    vi.resetModules();
    vi.doMock("../media-gallery-auth", () => ({
      isAuthConfigured: () => true,
      getAccessToken: vi.fn(async () => "token"),
    }));
    const { useSignalR } = await import("../SignalR");
    useSignalR();
    expect(mockApp.init).toHaveBeenCalledWith(
      expect.objectContaining({ isTokenRequired: true, withCredentials: false }),
    );
    vi.doUnmock("../media-gallery-auth");
    vi.resetModules();
  });

  it("registers MediaChanged handler on the connection", async () => {
    const { useSignalR } = await import("../SignalR");
    useSignalR();
    expect(mockConnection.on).toHaveBeenCalledWith("MediaChanged", expect.any(Function));
  });

  it("subscribes to the selected folder after connecting", async () => {
    setDirectory("/Images");

    const { useSignalR } = await import("../SignalR");
    useSignalR();
    onConnectSuccessCb?.({ url: "/hubs/media" });

    expect(mockConnection.invoke).toHaveBeenCalledWith("SubscribePath", "/Images");
  });

  it("re-subscribes to the selected folder after reconnecting", async () => {
    setDirectory("/Images");

    const { useSignalR } = await import("../SignalR");
    useSignalR();
    onReconnectedCb?.();

    expect(mockConnection.invoke).toHaveBeenCalledWith("SubscribePath", "/Images");
  });

  it("updates folder subscriptions when the selected folder changes", async () => {
    setDirectory("/Old");

    const { useSignalR } = await import("../SignalR");
    useSignalR();
    mockConnection.invoke.mockClear();

    setDirectory("/New");
    await nextTick();

    expect(mockConnection.invoke).toHaveBeenNthCalledWith(1, "UnsubscribePath", "/Old");
    expect(mockConnection.invoke).toHaveBeenNthCalledWith(2, "SubscribePath", "/New");
  });

  it("MediaChanged callback calls loadDirectoryFiles", async () => {
    const { useSignalR } = await import("../SignalR");
    useSignalR();

    // Trigger the MediaChanged event
    if (mediaChangedCb) {
      await mediaChangedCb("test-message");
    }

    expect(mockLoadDirectoryFiles).toHaveBeenCalled();
  });

  it("registers a handler for signalRReceivedData", async () => {
    const { signalRReceivedData } = await import("@bloom/services/signalr/eventbus");
    const { useSignalR } = await import("../SignalR");
    useSignalR();
    expect(signalRReceivedData.on).toHaveBeenCalledWith(expect.any(Function));
  });

  it("signalRReceivedData callback does not throw", async () => {
    const { useSignalR } = await import("../SignalR");
    useSignalR();

    expect(() => {
      if (signalRReceivedDataCb) {
        signalRReceivedDataCb({ ClientMethod: "test", Data: [] });
      }
    }).not.toThrow();
  });

  it("calls onConnect with success and error handlers", async () => {
    const { useSignalR } = await import("../SignalR");
    useSignalR();
    expect(mockApp.onConnect).toHaveBeenCalledWith(expect.any(Function), expect.any(Function));
  });

  it("onConnect success handler does not throw", async () => {
    const { useSignalR } = await import("../SignalR");
    useSignalR();

    expect(() => {
      if (onConnectSuccessCb) onConnectSuccessCb({ url: "/hubs/media" });
    }).not.toThrow();
  });

  it("onConnect error handler does not throw", async () => {
    const { useSignalR } = await import("../SignalR");
    useSignalR();

    expect(() => {
      if (onConnectErrorCb) onConnectErrorCb(new Error("connection failed"));
    }).not.toThrow();
  });
});
