import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

// Track callbacks for testing
let onConnectSuccessCb: ((data: unknown) => void) | null = null;
let onConnectErrorCb: ((err: unknown) => void) | null = null;
let mediaChangedCb: ((message: unknown) => Promise<void>) | null = null;
let signalRReceivedDataCb: ((data: unknown) => void) | null = null;

const mockGetFileLibraryStoreAsync = vi.fn(() => Promise.resolve([]));

const mockConnection = {
  on: vi.fn((event: string, cb: (...args: any[]) => any) => { // eslint-disable-line @typescript-eslint/no-explicit-any
    if (event === "MediaChanged") mediaChangedCb = cb;
  }),
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
  default: vi.fn(() => mockApp),
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
  }),
}));

describe("SignalR", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    onConnectSuccessCb = null;
    onConnectErrorCb = null;
    mediaChangedCb = null;
    signalRReceivedDataCb = null;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("creates a SignalRApp and initialises it", async () => {
    const SignalRApp = (await import("@bloom/services/signalr/signalr-app")).default;
    const { useSignalR } = await import("../SignalR");
    useSignalR();
    expect(SignalRApp).toHaveBeenCalledWith("/hubs/media");
    expect(mockApp.init).toHaveBeenCalledWith({ url: "/hubs/media" });
  });

  it("registers MediaChanged handler on the connection", async () => {
    const { useSignalR } = await import("../SignalR");
    useSignalR();
    expect(mockConnection.on).toHaveBeenCalledWith("MediaChanged", expect.any(Function));
  });

  it("MediaChanged callback calls getFileLibraryStoreAsync", async () => {
    const { useSignalR } = await import("../SignalR");
    useSignalR();

    // Trigger the MediaChanged event
    if (mediaChangedCb) {
      await mediaChangedCb("test-message");
    }

    expect(mockGetFileLibraryStoreAsync).toHaveBeenCalled();
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
