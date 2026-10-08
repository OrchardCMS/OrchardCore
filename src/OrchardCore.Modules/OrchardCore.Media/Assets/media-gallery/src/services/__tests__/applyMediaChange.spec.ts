import { describe, it, expect, beforeEach } from "vitest";
import type { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { applyMediaChange, toMediaChange } from "../applyMediaChange";
import { useGlobals } from "../Globals";

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

describe("toMediaChange", () => {
  it("maps an upload to a file addition", () => {
    const item = file("photos/c.jpg");

    expect(toMediaChange({ action: "fileUploaded", path: "photos/c.jpg", item }))
      .toEqual({ kind: "fileAdded", item });
  });

  it("maps a deletion to a file removal", () => {
    expect(toMediaChange({ action: "fileDeleted", path: "photos/a.jpg" }))
      .toEqual({ kind: "fileRemoved", filePath: "photos/a.jpg" });
  });

  it("maps a move to the old path plus the new entry", () => {
    const item = file("docs/a.jpg");

    expect(toMediaChange({ action: "fileMoved", path: "photos/a.jpg", newPath: "docs/a.jpg", item }))
      .toEqual({ kind: "fileMoved", oldPath: "photos/a.jpg", item });
  });

  it("describes a created directory from its path alone", () => {
    const change = toMediaChange({ action: "directoryCreated", path: "photos/2026" });

    expect(change).toMatchObject({ kind: "directoryAdded", parentPath: "photos" });
    expect(change).toMatchObject({ item: { name: "2026", directoryPath: "photos/2026", isDirectory: true } });
  });

  it("maps a deleted directory", () => {
    expect(toMediaChange({ action: "directoryDeleted", path: "photos/2026" }))
      .toEqual({ kind: "directoryRemoved", directoryPath: "photos/2026" });
  });

  it("gives up when an entry is required but missing", () => {
    expect(toMediaChange({ action: "fileUploaded", path: "photos/c.jpg" })).toBeNull();
    expect(toMediaChange({ action: "fileMoved", path: "photos/a.jpg", newPath: "docs/a.jpg" })).toBeNull();
  });

  it("gives up on an action it does not know", () => {
    expect(toMediaChange({ action: "somethingNew", path: "photos/a.jpg" })).toBeNull();
    expect(toMediaChange({})).toBeNull();
  });
});

describe("applyMediaChange", () => {
  const { setSelectedDirectory, setFileItems, fileItems } = useGlobals();

  beforeEach(() => {
    setSelectedDirectory({ directoryPath: "photos" } as IFileLibraryItemDto);
    setFileItems([file("photos/a.jpg"), file("photos/b.jpg")]);
  });

  it("adds an uploaded file to the current directory", () => {
    expect(applyMediaChange({ action: "fileUploaded", path: "photos/c.jpg", item: file("photos/c.jpg") })).toBe(true);
    expect(fileItems.value.map(f => f.filePath)).toContain("photos/c.jpg");
    expect(fileItems.value).toHaveLength(3);
  });

  it("does not duplicate a file that is overwritten", () => {
    expect(applyMediaChange({ action: "fileUploaded", path: "photos/a.jpg", item: file("photos/a.jpg") })).toBe(true);
    expect(fileItems.value.filter(f => f.filePath === "photos/a.jpg")).toHaveLength(1);
  });

  it("leaves the view alone for another directory", () => {
    expect(applyMediaChange({ action: "fileUploaded", path: "docs/c.jpg", item: file("docs/c.jpg") })).toBe(true);
    expect(fileItems.value).toHaveLength(2);
  });

  it("removes a deleted file", () => {
    expect(applyMediaChange({ action: "fileDeleted", path: "photos/a.jpg" })).toBe(true);
    expect(fileItems.value.map(f => f.filePath)).toEqual(["photos/b.jpg"]);
  });

  it("handles a rename as a removal plus an addition", () => {
    const applied = applyMediaChange({
      action: "fileMoved",
      path: "photos/a.jpg",
      newPath: "photos/renamed.jpg",
      item: file("photos/renamed.jpg"),
    });

    expect(applied).toBe(true);

    const paths = fileItems.value.map(f => f.filePath);
    expect(paths).not.toContain("photos/a.jpg");
    expect(paths).toContain("photos/renamed.jpg");
    expect(paths).toHaveLength(2);
  });

  it("removes a file moved out of the current directory", () => {
    expect(applyMediaChange({
      action: "fileMoved",
      path: "photos/a.jpg",
      newPath: "docs/a.jpg",
      item: file("docs/a.jpg"),
    })).toBe(true);

    expect(fileItems.value.map(f => f.filePath)).toEqual(["photos/b.jpg"]);
  });

  it("applies directory changes instead of asking for a reload", () => {
    // Before the store mutations were shared, these fell back to reloading the directory because the
    // tree handling lived inside the local operations only.
    expect(applyMediaChange({ action: "directoryCreated", path: "photos/2026" })).toBe(true);
    expect(applyMediaChange({ action: "directoryDeleted", path: "photos/2026" })).toBe(true);
  });

  it("asks for a reload when the change cannot be described", () => {
    expect(applyMediaChange({ action: "fileUploaded", path: "photos/c.jpg" })).toBe(false);
    expect(applyMediaChange({ action: "somethingNew", path: "photos/a.jpg" })).toBe(false);
  });
});
