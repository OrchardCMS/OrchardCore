import { describe, it, expect } from "vitest";
import {
  getIconClassForFilename,
  isImageExtension,
} from "../FontAwesomeThumbnails";

describe("getIconClassForFilename", () => {
  describe("image extensions", () => {
    it.each(["jpg", "jpeg", "png", "gif", "webp", "svg", "bmp", "ico"])(
      "returns image icon for .%s",
      (ext) => {
        expect(getIconClassForFilename(`photo.${ext}`)).toBe(
          "fa-regular fa-image"
        );
      }
    );
  });

  describe("document extensions", () => {
    it("returns pdf icon for .pdf", () => {
      expect(getIconClassForFilename("report.pdf")).toBe(
        "fa-regular fa-file-pdf"
      );
    });

    it.each(["doc", "docx"])("returns word icon for .%s", (ext) => {
      expect(getIconClassForFilename(`document.${ext}`)).toBe(
        "fa-regular fa-file-word"
      );
    });

    it.each(["ppt", "pptx"])("returns powerpoint icon for .%s", (ext) => {
      expect(getIconClassForFilename(`slides.${ext}`)).toBe(
        "fa-regular fa-file-powerpoint"
      );
    });

    it.each(["xls", "xlsx"])("returns excel icon for .%s", (ext) => {
      expect(getIconClassForFilename(`spreadsheet.${ext}`)).toBe(
        "fa-regular fa-file-excel"
      );
    });

    it("returns file icon for .csv", () => {
      expect(getIconClassForFilename("data.csv")).toBe("fa-regular fa-file");
    });
  });

  describe("audio extensions", () => {
    it.each(["aac", "mp3", "ogg"])("returns audio icon for .%s", (ext) => {
      expect(getIconClassForFilename(`track.${ext}`)).toBe(
        "fa-regular fa-file-audio"
      );
    });
  });

  describe("video extensions", () => {
    it.each(["avi", "flv", "mkv", "mp4", "webm"])(
      "returns video icon for .%s",
      (ext) => {
        expect(getIconClassForFilename(`clip.${ext}`)).toBe(
          "fa-regular fa-file-video"
        );
      }
    );
  });

  describe("archive extensions", () => {
    it.each(["gz", "zip"])("returns archive icon for .%s", (ext) => {
      expect(getIconClassForFilename(`archive.${ext}`)).toBe(
        "fa-regular fa-file-zipper"
      );
    });
  });

  describe("code extensions", () => {
    it.each(["css", "html", "js"])("returns code icon for .%s", (ext) => {
      expect(getIconClassForFilename(`source.${ext}`)).toBe(
        "fa-regular fa-file-code"
      );
    });
  });

  describe("text extension", () => {
    it("returns text icon for .txt", () => {
      expect(getIconClassForFilename("readme.txt")).toBe(
        "fa-regular fa-file-lines"
      );
    });
  });

  describe("unknown and edge cases", () => {
    it("returns default file icon for unknown extension", () => {
      expect(getIconClassForFilename("data.xyz")).toBe("fa-regular fa-file");
    });

    it("returns default file icon for empty filename", () => {
      expect(getIconClassForFilename("")).toBe("fa-regular fa-file");
    });

    it("returns default file icon for filename without extension", () => {
      expect(getIconClassForFilename("Makefile")).toBe("fa-regular fa-file");
    });
  });

  describe("case insensitivity", () => {
    it("handles uppercase JPG", () => {
      expect(getIconClassForFilename("photo.JPG")).toBe(
        "fa-regular fa-image"
      );
    });

    it("handles uppercase PDF", () => {
      expect(getIconClassForFilename("report.PDF")).toBe(
        "fa-regular fa-file-pdf"
      );
    });

    it("handles mixed case Png", () => {
      expect(getIconClassForFilename("image.Png")).toBe(
        "fa-regular fa-image"
      );
    });
  });

  describe("sizeClass parameter", () => {
    it("appends sizeClass when provided", () => {
      expect(getIconClassForFilename("photo.jpg", "fa-2x")).toBe(
        "fa-regular fa-image fa-2x"
      );
    });

    it("does not append extra space when sizeClass is empty", () => {
      expect(getIconClassForFilename("photo.jpg", "")).toBe(
        "fa-regular fa-image"
      );
    });

    it("appends sizeClass to default icon for unknown extension", () => {
      expect(getIconClassForFilename("data.xyz", "fa-3x")).toBe(
        "fa-regular fa-file fa-3x"
      );
    });
  });
});

describe("isImageExtension", () => {
  describe("returns true for image extensions", () => {
    it.each(["gif", "jpeg", "jpg", "png", "webp", "svg", "bmp", "ico"])(
      "returns true for %s",
      (ext) => {
        expect(isImageExtension(ext)).toBe(true);
      }
    );
  });

  describe("returns false for non-image extensions", () => {
    it.each(["pdf", "doc", "mp3", "mp4", "zip", "js", "txt", "csv"])(
      "returns false for %s",
      (ext) => {
        expect(isImageExtension(ext)).toBe(false);
      }
    );
  });

  it("handles leading dot", () => {
    expect(isImageExtension(".jpg")).toBe(true);
    expect(isImageExtension(".png")).toBe(true);
    expect(isImageExtension(".pdf")).toBe(false);
  });

  it("is case insensitive", () => {
    expect(isImageExtension("JPG")).toBe(true);
    expect(isImageExtension("PNG")).toBe(true);
    expect(isImageExtension("Gif")).toBe(true);
    expect(isImageExtension("SVG")).toBe(true);
  });

  it("returns false for empty string", () => {
    expect(isImageExtension("")).toBe(false);
  });
});
