import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { ref } from "vue";

// ── Mocks (must be declared before any import that triggers main.ts) ────────

vi.mock("vue-final-modal", () => ({
  VueFinalModal: {
    name: "VueFinalModal",
    template: "<div><slot /></div>",
    props: ["modelValue"],
  },
  createVfm: () => ({ install: vi.fn() }),
}));

vi.mock("vue-final-modal/style.css", () => ({}));
vi.mock("../assets/css/field.css", () => ({}));

const mockTranslationsRef = ref<Record<string, string>>({});
const mockSetTranslations = vi.fn((t: Record<string, string>) => {
  mockTranslationsRef.value = t;
});

vi.mock("@bloom/helpers/localizations", () => ({
  useLocalizations: () => ({
    translations: mockTranslationsRef,
    setTranslations: mockSetTranslations,
  }),
}));

vi.mock("@media-app", () => ({
  mountMediaAppAsPicker: vi.fn(),
}));

// Stub the three field components so Vue can mount without real templates
vi.mock("../components/MediaFieldBasic.vue", () => ({
  default: {
    name: "MediaFieldBasic",
    template: "<div class='stub-basic'></div>",
    props: ["config", "inputName"],
  },
}));

vi.mock("../components/MediaFieldAttached.vue", () => ({
  default: {
    name: "MediaFieldAttached",
    template: "<div class='stub-attached'></div>",
    props: ["config", "inputName"],
  },
  // The interface re-export is only a type; we don't need a runtime value.
}));

vi.mock("../components/MediaFieldGallery.vue", () => ({
  default: {
    name: "MediaFieldGallery",
    template: "<div class='stub-gallery'></div>",
    props: ["config", "inputName"],
  },
}));

// Stub PrimeVue and its theme so createFieldApp doesn't blow up
vi.mock("primevue/config", () => ({
  default: { install: vi.fn() },
}));
vi.mock("@primevue/themes/aura", () => ({ default: {} }));

// Stub FontAwesome so library.add / FontAwesomeIcon don't fail
vi.mock("@fortawesome/fontawesome-svg-core", () => ({
  library: { add: vi.fn() },
}));
vi.mock("@fortawesome/vue-fontawesome", () => ({
  FontAwesomeIcon: {
    name: "FontAwesomeIcon",
    template: "<i></i>",
    props: ["icon"],
  },
}));
vi.mock("@fortawesome/free-solid-svg-icons", () => ({ fas: {} }));
vi.mock("@fortawesome/free-regular-svg-icons", () => ({ far: {} }));

// ── Helpers ─────────────────────────────────────────────────────────────────

function createMountEl(
  type: string,
  attrs: Record<string, string> = {}
): HTMLElement {
  const el = document.createElement("div");
  el.dataset.mediaFieldType = type;
  el.dataset.inputName = "TestField";
  el.dataset.paths = "[]";
  el.dataset.mediaItemUrl = "/api/media";
  Object.entries(attrs).forEach(([k, v]) => {
    el.dataset[k] = v;
  });
  document.body.appendChild(el);
  return el;
}

// ── Tests ───────────────────────────────────────────────────────────────────

describe("main.ts", () => {
  // We use dynamic import + vi.resetModules so the top-level autoMount()
  // runs against a clean DOM for every test.
  let main: typeof import("../main");

  beforeEach(async () => {
    document.body.innerHTML = "";
    mockSetTranslations.mockClear();
    vi.resetModules();
    main = await import("../main");
  });

  afterEach(() => {
    // Destroy any leftover Vue apps by clearing the DOM
    document.body.innerHTML = "";
  });

  // ── mountMediaField ─────────────────────────────────────────────────────

  describe("mountMediaField", () => {
    it("mounts a Vue app on the given element", () => {
      const el = createMountEl("basic");
      const app = main.mountMediaField(el);
      expect(app).toBeDefined();
      // The stub component should have rendered inside the element
      expect(el.querySelector(".stub-basic")).not.toBeNull();
    });

    it("reads config from data attributes", () => {
      const el = createMountEl("basic", {
        multiple: "true",
        allowMediaText: "true",
        allowAnchors: "true",
        allowedExtensions: ".jpg,.png",
        basePath: "/media",
        uploadFilesUrl: "/api/upload",
      });

      // Mounting should succeed and call setTranslations
      main.mountMediaField(el);
      expect(mockSetTranslations).toHaveBeenCalled();
    });

    it("reads translations with defaults when data-translations is absent", () => {
      const el = createMountEl("basic");
      main.mountMediaField(el);

      const lastCall =
        mockSetTranslations.mock.calls[
          mockSetTranslations.mock.calls.length - 1
        ];
      const translations = lastCall[0] as Record<string, string>;
      // Verify some default keys
      expect(translations.noImages).toBe("No images");
      expect(translations.addMedia).toBe("Add media");
      expect(translations.removeMedia).toBe("Remove");
      expect(translations.ok).toBe("OK");
      expect(translations.cancel).toBe("Cancel");
    });

    it("reads custom translations from data-translations", () => {
      const custom = { noImages: "Nothing here", addMedia: "Pick file" };
      const el = createMountEl("basic", {
        translations: JSON.stringify(custom),
      });
      main.mountMediaField(el);

      const lastCall =
        mockSetTranslations.mock.calls[
          mockSetTranslations.mock.calls.length - 1
        ];
      const translations = lastCall[0] as Record<string, string>;
      expect(translations.noImages).toBe("Nothing here");
      expect(translations.addMedia).toBe("Pick file");
      // Defaults still present for keys not overridden
      expect(translations.ok).toBe("OK");
    });
  });

  // ── mountAttachedMediaField ─────────────────────────────────────────────

  describe("mountAttachedMediaField", () => {
    it("reads attached config including uploadAction and tempUploadFolder", () => {
      const el = createMountEl("attached", {
        uploadAction: "/api/attached-upload",
        tempUploadFolder: "tmp-folder",
        maxUploadChunkSize: "2048",
      });
      const app = main.mountAttachedMediaField(el);
      expect(app).toBeDefined();
      expect(el.querySelector(".stub-attached")).not.toBeNull();
    });

    it("handles missing attached-specific attributes gracefully", () => {
      const el = createMountEl("attached");
      const app = main.mountAttachedMediaField(el);
      expect(app).toBeDefined();
    });
  });

  // ── mountGalleryMediaField ──────────────────────────────────────────────

  describe("mountGalleryMediaField", () => {
    it("mounts a gallery field on the element", () => {
      const el = createMountEl("gallery");
      const app = main.mountGalleryMediaField(el);
      expect(app).toBeDefined();
      expect(el.querySelector(".stub-gallery")).not.toBeNull();
    });
  });

  // ── readConfig (tested indirectly via mount functions) ──────────────────

  describe("readConfig (via mountMediaField)", () => {
    it("parses paths JSON", () => {
      const paths = [{ path: "img/a.jpg" }, { path: "img/b.png", mediaText: "alt" }];
      const el = createMountEl("basic", {
        paths: JSON.stringify(paths),
      });
      // If paths were invalid the component would receive an empty array;
      // we verify the mount succeeds (no throw) with valid JSON.
      const app = main.mountMediaField(el);
      expect(app).toBeDefined();
    });

    it("handles invalid JSON for paths without throwing", () => {
      const el = createMountEl("basic", {
        paths: "NOT_JSON{{{",
      });
      // Should not throw – falls back to []
      const app = main.mountMediaField(el);
      expect(app).toBeDefined();
    });

    it("reads boolean attributes correctly", () => {
      const elTrue = createMountEl("basic", {
        multiple: "true",
        allowMediaText: "true",
        allowAnchors: "true",
      });
      // Mounts fine with booleans set to true
      expect(() => main.mountMediaField(elTrue)).not.toThrow();

      const elFalse = createMountEl("basic", {
        multiple: "false",
        allowMediaText: "false",
        allowAnchors: "false",
      });
      expect(() => main.mountMediaField(elFalse)).not.toThrow();
    });
  });

  // ── readTranslations (tested indirectly via mount) ──────────────────────

  describe("readTranslations (via mountMediaField)", () => {
    it("falls back to defaults when data-translations is missing", () => {
      const el = createMountEl("basic");
      main.mountMediaField(el);

      const lastCall =
        mockSetTranslations.mock.calls[
          mockSetTranslations.mock.calls.length - 1
        ];
      const t = lastCall[0] as Record<string, string>;
      expect(t.selectMedia).toBe("Select Media");
      expect(t.loadingMediaBrowser).toBe("Loading media browser...");
    });

    it("falls back to defaults when data-translations contains invalid JSON", () => {
      const el = createMountEl("basic", {
        translations: "INVALID{{{",
      });
      main.mountMediaField(el);

      const lastCall =
        mockSetTranslations.mock.calls[
          mockSetTranslations.mock.calls.length - 1
        ];
      const t = lastCall[0] as Record<string, string>;
      expect(t.noImages).toBe("No images");
    });

    it("merges custom translations over defaults", () => {
      const el = createMountEl("basic", {
        translations: JSON.stringify({
          dropFiles: "Drag & drop",
          uploads: "Uploading...",
        }),
      });
      main.mountMediaField(el);

      const lastCall =
        mockSetTranslations.mock.calls[
          mockSetTranslations.mock.calls.length - 1
        ];
      const t = lastCall[0] as Record<string, string>;
      expect(t.dropFiles).toBe("Drag & drop");
      expect(t.uploads).toBe("Uploading...");
      // Un-overridden defaults remain
      expect(t.clearErrors).toBe("Clear errors");
    });
  });

  // ── autoMount ───────────────────────────────────────────────────────────

  describe("autoMount", () => {
    it("auto-mounts elements with data-media-field-type on import", async () => {
      // We need a fresh import with elements already in the DOM.
      document.body.innerHTML = "";
      const el = createMountEl("basic");
      // Remove any mounted marker from a previous test
      delete el.dataset.mediaFieldMounted;

      vi.resetModules();
      await import("../main");

      expect(el.dataset.mediaFieldMounted).toBe("true");
      expect(el.querySelector(".stub-basic")).not.toBeNull();
    });

    it("auto-mounts multiple element types", async () => {
      document.body.innerHTML = "";
      const basicEl = createMountEl("basic");
      const attachedEl = createMountEl("attached");
      const galleryEl = createMountEl("gallery");

      vi.resetModules();
      await import("../main");

      expect(basicEl.dataset.mediaFieldMounted).toBe("true");
      expect(attachedEl.dataset.mediaFieldMounted).toBe("true");
      expect(galleryEl.dataset.mediaFieldMounted).toBe("true");
    });

    it("skips elements that are already mounted", async () => {
      document.body.innerHTML = "";
      const el = createMountEl("basic");
      el.dataset.mediaFieldMounted = "true";

      vi.resetModules();
      await import("../main");

      // The stub component should NOT be rendered because autoMount skipped it
      expect(el.querySelector(".stub-basic")).toBeNull();
    });

    it("ignores elements with unknown media-field-type", async () => {
      document.body.innerHTML = "";
      const el = createMountEl("unknown-type");

      vi.resetModules();
      await import("../main");

      // Should not be marked as mounted
      expect(el.dataset.mediaFieldMounted).toBeUndefined();
    });
  });
});
