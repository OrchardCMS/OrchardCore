import { beforeEach, describe, expect, it } from "vitest";
import { useLocalStorage } from "../LocalStorage";
import { useGlobals } from "../Globals";
import { LS_ID } from "@bloom/media/constants";

const { setRootDirectory, selectedDirectory, setSelectedDirectory } = useGlobals();

const rootDir = {
  filePath: "/",
  directoryPath: "/",
  name: "",
  isDirectory: true,
};

const imagesDir = {
  filePath: "/Images",
  directoryPath: "/Images",
  name: "Images",
  isDirectory: true,
};

describe("LocalStorage", () => {
  beforeEach(() => {
    localStorage.clear();
    setRootDirectory(rootDir);
    setSelectedDirectory(rootDir);
  });

  describe("setLocalStorage", () => {
    it("defaults to rootDirectory when localStorage is empty", () => {
      const { setLocalStorage } = useLocalStorage();
      setLocalStorage();
      expect(selectedDirectory.value.directoryPath).toBe("/");
    });

    it("restores selected directory from localStorage", () => {
      const { setLocalStorage } = useLocalStorage();

      localStorage.setItem(
        LS_ID,
        JSON.stringify({
          smallThumbs: false,
          selectedDirectory: imagesDir,
          gridView: false,
        }),
      );

      setLocalStorage();
      expect(selectedDirectory.value.directoryPath).toBe("/Images");
    });

    it("restores smallThumbs and gridView from localStorage", () => {
      const { setLocalStorage, smallThumbs, gridView } = useLocalStorage();

      localStorage.setItem(
        LS_ID,
        JSON.stringify({
          smallThumbs: true,
          selectedDirectory: rootDir,
          gridView: true,
        }),
      );

      setLocalStorage();
      expect(smallThumbs.value).toBe(true);
      expect(gridView.value).toBe(true);
    });
  });

  describe("localStorageData computed", () => {
    it("returns current state as ILocalStorageData", () => {
      const { localStorageData } = useLocalStorage();
      setSelectedDirectory(imagesDir);

      expect(localStorageData.value.selectedDirectory.directoryPath).toBe("/Images");
    });

    it("setting localStorageData updates state", () => {
      const { localStorageData } = useLocalStorage();

      localStorageData.value = {
        smallThumbs: true,
        selectedDirectory: imagesDir,
        gridView: true,
      };

      expect(selectedDirectory.value.directoryPath).toBe("/Images");
    });

    it("setting null/falsy localStorageData is a no-op", () => {
      const { localStorageData } = useLocalStorage();
      setSelectedDirectory(rootDir);

      // Setting to null-ish should not crash
      localStorageData.value = null as any; // eslint-disable-line @typescript-eslint/no-explicit-any

      expect(selectedDirectory.value.directoryPath).toBe("/");
    });
  });

  describe("watch - persists to localStorage", () => {
    it("persists state changes to localStorage via watch", async () => {
      const { localStorageData } = useLocalStorage();

      // Trigger a change to kick off the watcher
      setSelectedDirectory(imagesDir);

      // Allow the watcher to run (it's synchronous for immediate watchers but may be deferred)
      await new Promise((resolve) => setTimeout(resolve, 50));

      const stored = localStorage.getItem(LS_ID);
      expect(stored).not.toBeNull();
      if (stored) {
        const parsed = JSON.parse(stored);
        expect(parsed.selectedDirectory.directoryPath).toBe("/Images");
      }
    });
  });

  describe("gridView", () => {
    it("returns gridView ref", () => {
      const { gridView } = useLocalStorage();
      expect(typeof gridView.value).toBe("boolean");
    });
  });

  // Disabling the grid view is one-way for the lifetime of the module (it mirrors the
  // OrchardCore_Media:DisableThumbnails option), so these tests must run last in this file.
  describe("disableGridView", () => {
    it("turns off an already active grid view", () => {
      const { gridView, disableGridView } = useLocalStorage();
      gridView.value = true;

      disableGridView();

      expect(gridView.value).toBe(false);
    });

    it("ignores a persisted grid preference on restore", () => {
      const { setLocalStorage, gridView, disableGridView } = useLocalStorage();

      localStorage.setItem(
        LS_ID,
        JSON.stringify({
          smallThumbs: false,
          selectedDirectory: imagesDir,
          gridView: true,
        }),
      );

      disableGridView();
      setLocalStorage();

      expect(gridView.value).toBe(false);
      // Other preferences are still restored.
      expect(selectedDirectory.value.directoryPath).toBe("/Images");
    });
  });
});
