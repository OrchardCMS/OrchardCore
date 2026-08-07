import { describe, it, expect, beforeEach, vi } from "vitest";
import type { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { applyMediaChange } from "../applyMediaChange";
import { useGlobals } from "../Globals";

const invalidateFileCache = vi.fn();

vi.mock("../FileLibraryManager", () => ({
  useFileLibraryManager: () => ({ invalidateFileCache }),
}));

function file(filePath: string): IFileLibraryItemDto {
  const index = filePath.lastIndexOf("/");

  return {
    name: index >= 0 ? filePath.substring(index + 1) : filePath,
    directoryPath: index >= 0 ? filePath.substring(0, index) : "",
    filePath,
    isDirectory: false,
    url: `/media/${filePath}`,
  };
}

describe("applyMediaChange", () => {
  const { setSelectedDirectory, setFileItems, fileItems } = useGlobals();

  beforeEach(() => {
    invalidateFileCache.mockClear();
    setSelectedDirectory({ directoryPath: "photos" } as IFileLibraryItemDto);
    setFileItems([file("photos/a.jpg"), file("photos/b.jpg")]);
  });

  it("adds an uploaded file to the current directory without a reload", () => {
    const handled = applyMediaChange({ action: "fileUploaded", path: "photos/c.jpg", item: file("photos/c.jpg") });

    expect(handled).toBe(true);
    expect(fileItems.value.map(f => f.filePath)).toContain("photos/c.jpg");
    expect(fileItems.value).toHaveLength(3);
  });

  it("does not duplicate a file that is overwritten", () => {
    const handled = applyMediaChange({ action: "fileUploaded", path: "photos/a.jpg", item: file("photos/a.jpg") });

    expect(handled).toBe(true);
    expect(fileItems.value.filter(f => f.filePath === "photos/a.jpg")).toHaveLength(1);
  });

  it("requests a reload when an upload carries no item", () => {
    const handled = applyMediaChange({ action: "fileUploaded", path: "photos/c.jpg" });

    expect(handled).toBe(false);
  });

  it("ignores an upload into another directory but still invalidates its cache", () => {
    const handled = applyMediaChange({ action: "fileUploaded", path: "docs/c.jpg", item: file("docs/c.jpg") });

    expect(handled).toBe(true);
    expect(fileItems.value).toHaveLength(2);
    expect(invalidateFileCache).toHaveBeenCalledWith("docs");
  });

  it("removes a deleted file from the current directory", () => {
    const handled = applyMediaChange({ action: "fileDeleted", path: "photos/a.jpg" });

    expect(handled).toBe(true);
    expect(fileItems.value.map(f => f.filePath)).toEqual(["photos/b.jpg"]);
  });

  it("handles a rename as a remove plus an add", () => {
    const handled = applyMediaChange({
      action: "fileMoved",
      path: "photos/a.jpg",
      newPath: "photos/renamed.jpg",
      item: file("photos/renamed.jpg"),
    });

    expect(handled).toBe(true);

    const paths = fileItems.value.map(f => f.filePath);
    expect(paths).not.toContain("photos/a.jpg");
    expect(paths).toContain("photos/renamed.jpg");
    expect(paths).toHaveLength(2);
  });

  it("removes a file moved out of the current directory", () => {
    const handled = applyMediaChange({
      action: "fileMoved",
      path: "photos/a.jpg",
      newPath: "docs/a.jpg",
      item: file("docs/a.jpg"),
    });

    expect(handled).toBe(true);
    expect(fileItems.value.map(f => f.filePath)).toEqual(["photos/b.jpg"]);
    expect(invalidateFileCache).toHaveBeenCalledWith("docs");
  });

  it("adds a copied file to the current directory", () => {
    const handled = applyMediaChange({
      action: "fileCopied",
      path: "photos/a.jpg",
      newPath: "photos/a-copy.jpg",
      item: file("photos/a-copy.jpg"),
    });

    expect(handled).toBe(true);
    expect(fileItems.value.map(f => f.filePath)).toContain("photos/a-copy.jpg");
  });

  it.each(["directoryCreated", "directoryDeleted"])("requests a reload for %s", (action) => {
    expect(applyMediaChange({ action, path: "photos/sub" })).toBe(false);
  });

  it("requests a reload for an unknown or missing action", () => {
    expect(applyMediaChange({ action: "somethingNew", path: "photos/a.jpg" })).toBe(false);
    expect(applyMediaChange({})).toBe(false);
  });
});
