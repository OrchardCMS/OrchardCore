import { beforeEach, describe, expect, it, vi } from "vitest";
import { useGlobals } from "../Globals";
import { useRouterService } from "../RouterService";
import { useLocalStorage } from "../LocalStorage";
import { assetsStoreData } from "../../__tests__/mockdata";

// Mock the router so we can control currentRoute
vi.mock("../../router", () => ({
  default: {
    currentRoute: {
      value: {
        params: { path: undefined as string | undefined },
      },
    },
  },
}));

import router from "../../router";

const { setAssetsStore, setRootDirectory, selectedDirectory, setSelectedDirectory } = useGlobals();
const { setLocalStorage, localStorageData } = useLocalStorage();

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

describe("RouterService", () => {
  beforeEach(() => {
    setAssetsStore(assetsStoreData);
    setRootDirectory(rootDir);
    setSelectedDirectory(rootDir);
    // Reset params to undefined by default
    (router.currentRoute.value as any).params = { path: undefined };
    // Clear localStorage
    localStorage.clear();
  });

  it("Router root folder - no localStorage, no path param → selects root", () => {
    (router.currentRoute.value as any).params = { path: undefined };
    useRouterService();
    expect(selectedDirectory.value.directoryPath).toBe("/");
  });

  it("Router root folder - with localStorage dir that exists → selects stored dir", () => {
    // Set localStorage with a directory that exists in the store
    localStorage.setItem(
      "MediaLibraryPrefsV1",
      JSON.stringify({
        smallThumbs: false,
        selectedDirectory: imagesDir,
        gridView: false,
      }),
    );
    setLocalStorage();

    (router.currentRoute.value as any).params = { path: undefined };
    useRouterService();
    expect(selectedDirectory.value.directoryPath).toBe("/Images");
  });

  it("Router root folder - with localStorage dir that does NOT exist → selects root", () => {
    // Set localStorage with a directory NOT in the store
    localStorage.setItem(
      "MediaLibraryPrefsV1",
      JSON.stringify({
        smallThumbs: false,
        selectedDirectory: {
          filePath: "/NonExistent",
          directoryPath: "/NonExistent",
          name: "NonExistent",
          isDirectory: true,
        },
        gridView: false,
      }),
    );
    setLocalStorage();

    (router.currentRoute.value as any).params = { path: undefined };
    useRouterService();
    expect(selectedDirectory.value.directoryPath).toBe("/");
  });

  it("Router folder route - path param matches a directory → selects that dir", () => {
    (router.currentRoute.value as any).params = { path: "/Images" };
    useRouterService();
    expect(selectedDirectory.value.directoryPath).toBe("/Images");
  });

  it("Router folder route - path param does NOT match any directory → selects root", () => {
    (router.currentRoute.value as any).params = { path: "/DoesNotExist" };
    useRouterService();
    expect(selectedDirectory.value.directoryPath).toBe("/");
  });

  it("Router - no currentRoute value, with localStorage dir that exists → selects stored dir", () => {
    // Simulate no currentRoute.value
    (router as any).currentRoute = { value: null };

    // Set localStorage with Images dir
    localStorage.setItem(
      "MediaLibraryPrefsV1",
      JSON.stringify({
        smallThumbs: false,
        selectedDirectory: imagesDir,
        gridView: false,
      }),
    );
    setLocalStorage();

    useRouterService();
    expect(selectedDirectory.value.directoryPath).toBe("/Images");

    // Restore router mock
    (router as any).currentRoute = {
      value: { params: { path: undefined } },
    };
  });

  it("Router - no currentRoute value, with localStorage dir that does NOT exist → selects root", () => {
    (router as any).currentRoute = { value: null };

    localStorage.setItem(
      "MediaLibraryPrefsV1",
      JSON.stringify({
        smallThumbs: false,
        selectedDirectory: {
          filePath: "/NonExistent",
          directoryPath: "/NonExistent",
          name: "NonExistent",
          isDirectory: true,
        },
        gridView: false,
      }),
    );
    setLocalStorage();

    useRouterService();
    expect(selectedDirectory.value.directoryPath).toBe("/");

    // Restore router mock
    (router as any).currentRoute = {
      value: { params: { path: undefined } },
    };
  });

  it("Router - no currentRoute value and no localStorage → does nothing", () => {
    (router as any).currentRoute = { value: null };

    // No localStorage set
    setSelectedDirectory(rootDir);

    useRouterService();
    // Should remain at root since there's no localStorage data
    expect(selectedDirectory.value.directoryPath).toBe("/");

    // Restore router mock
    (router as any).currentRoute = {
      value: { params: { path: undefined } },
    };
  });
});
