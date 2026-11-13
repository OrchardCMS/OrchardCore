import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { flushPromises } from "@vue/test-utils";

vi.mock("../../App.vue", () => ({
  default: {
    template: "<div :data-max-file-size='maxFileSize'></div>",
    props: ["maxFileSize"],
  },
}));
vi.mock("../../appSetup", () => ({ configureMediaApp: vi.fn() }));
vi.mock("@bloom/services/notifications/notifier", () => ({ registerNotificationBus: vi.fn() }));
vi.mock("../media-gallery-auth", () => ({ handleSilentRenewCallback: vi.fn() }));

describe("Upload limit configuration", () => {
  beforeEach(() => {
    vi.resetModules();
    document.body.innerHTML = "";
  });

  afterEach(() => {
    document.body.innerHTML = "";
    vi.unstubAllGlobals();
  });

  it.each([1234567, undefined])("passes picker limit %s to App", async (maxFileSize) => {
    const { mountMediaAppAsPicker } = await import("../../main");
    const container = document.createElement("div");
    document.body.appendChild(container);
    const handle = mountMediaAppAsPicker(container, {
      translations: "{}",
      basePath: "/",
      uploadFilesUrl: "/api/media/Upload",
      maxFileSize,
    });
    expect(container.firstElementChild?.getAttribute("data-max-file-size")).toBe(String(maxFileSize ?? 0));
    handle.unmount();
  });

  it.each([1234567, undefined])("passes standalone config limit %s to App", async (maxFileSize) => {
    document.body.innerHTML = '<div id="media-gallery"></div>';
    vi.stubGlobal("fetch", vi.fn()
      .mockResolvedValueOnce({ ok: true, json: async () => ({ orchardBaseUrl: "https://cms.example.com/", maxFileSize }) })
      .mockResolvedValueOnce({ ok: true, json: async () => ({}) }));
    await import("../../standalone");
    await flushPromises();
    expect(document.querySelector("[data-max-file-size]")?.getAttribute("data-max-file-size")).toBe(String(maxFileSize ?? 0));
    expect(fetch).toHaveBeenCalledTimes(2);
  });

  it.each([undefined, 7654321, 0])("forwards programmatic picker limit %s", async (maxFileSize) => {
    const openPicker = vi.fn().mockResolvedValue([]);
    const el = document.createElement("div") as HTMLElement & { __openMediaPicker?: typeof openPicker };
    el.dataset.mediaPickerConfig = "";
    el.__openMediaPicker = openPicker;
    document.body.appendChild(el);
    const { openMediaPicker } = await import("../../picker-api");
    await openMediaPicker({
      translations: "{}",
      basePath: "/",
      uploadFilesUrl: "/api/media/Upload",
      maxFileSize,
    });
    expect(openPicker).toHaveBeenCalledWith("{}", "/", "/api/media/Upload", "", true, maxFileSize);
  });
});
