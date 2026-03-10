import { describe, expect, it } from "vitest";
import { isValidFileExtension, getAllowedFileExtensions } from "../FileExtensionsService";

describe("FileExtensionsService", () => {
  describe("isValidFileExtension", () => {
    it("should return true when file has a valid extension", () => {
      expect(isValidFileExtension("/path", "file.txt")).toBe(true);
    });

    it("should return true for any file extension (server-side validation)", () => {
      expect(isValidFileExtension("/path", "file.pdf")).toBe(true);
      expect(isValidFileExtension("/path", "file.jpg")).toBe(true);
      expect(isValidFileExtension("/path", "file.gc3")).toBe(true);
    });

    it("should return false when file name is empty", () => {
      expect(isValidFileExtension("/path", "")).toBe(false);
    });

    it("should return false when file name is null", () => {
      expect(isValidFileExtension("/path", null as unknown as string)).toBe(false);
    });

    it("should return false when file name does not have an extension", () => {
      expect(isValidFileExtension("/path", "file")).toBe(false);
    });

    it("should return true for files with multiple dots", () => {
      expect(isValidFileExtension("/path", "file.backup.txt")).toBe(true);
    });
  });

  describe("getAllowedFileExtensions", () => {
    it("should return wildcard for any directory", () => {
      expect(getAllowedFileExtensions("/")).toEqual(["*.*"]);
      expect(getAllowedFileExtensions("/Images")).toEqual(["*.*"]);
    });
  });
});
