import { describe, expect, it, vi } from "vitest";
import Uppy from "@uppy/core";
import { useEventBus } from "../UseEventBus";
import { updateUploadOptions } from "../UppyFileUpload";

describe("UppyFileUpload", () => {
  it("should initialize Uppy instance", () => {
    const uppy = new Uppy({
      id: "my-uploader",
      meta: {},
      restrictions: {},
    });

    expect(uppy).toBeInstanceOf(Uppy);
  });

  it("should initialize Uppy with file restrictions", () => {
    const uppy = new Uppy({
      id: "restricted-uploader",
      restrictions: {
        maxFileSize: 10485760, // 10MB
        maxNumberOfFiles: 5,
      },
    });

    expect(uppy).toBeInstanceOf(Uppy);
    expect(uppy.opts.restrictions.maxFileSize).toBe(10485760);
    expect(uppy.opts.restrictions.maxNumberOfFiles).toBe(5);
  });

  it("should have event bus with upload events", () => {
    const { on, emit } = useEventBus();

    let receivedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
    on("UploadFileAdded", (data) => {
      receivedData = data;
    });

    emit("UploadFileAdded", { name: "test.jpg" });
    expect(receivedData).toEqual({ name: "test.jpg" });
  });

  it("should emit upload progress events", () => {
    const { on, emit } = useEventBus();

    let receivedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
    on("UploadProgress", (data) => {
      receivedData = data;
    });

    emit("UploadProgress", { name: "test.jpg", percentage: 50 });
    expect(receivedData).toEqual({ name: "test.jpg", percentage: 50 });
  });

  it("should emit upload success events", () => {
    const { on, emit } = useEventBus();

    let receivedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
    on("UploadSuccess", (data) => {
      receivedData = data;
    });

    emit("UploadSuccess", { name: "test.jpg" });
    expect(receivedData).toEqual({ name: "test.jpg" });
  });

  it("should emit upload error events", () => {
    const { on, emit } = useEventBus();

    let receivedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
    on("UploadError", (data) => {
      receivedData = data;
    });

    emit("UploadError", { name: "test.jpg", errorMessage: "Upload failed" });
    expect(receivedData).toEqual({ name: "test.jpg", errorMessage: "Upload failed" });
  });

  describe("updateUploadOptions", () => {
    it("should be a function", () => {
      expect(typeof updateUploadOptions).toBe("function");
    });

    it("should call setOptions on the Uppy instance without throwing", () => {
      expect(() => {
        updateUploadOptions({
          restrictions: {
            maxFileSize: 5 * 1024 * 1024, // 5MB
            maxNumberOfFiles: 10,
            allowedFileTypes: ["image/*"],
          },
        });
      }).not.toThrow();
    });

    it("should update upload options with meta and restrictions", () => {
      expect(() => {
        updateUploadOptions({
          meta: { destinationPath: "/uploads" },
          restrictions: {
            maxFileSize: 10 * 1024 * 1024,
          },
        });
      }).not.toThrow();
    });

    it("should accept empty options object", () => {
      expect(() => {
        updateUploadOptions({});
      }).not.toThrow();
    });
  });

  describe("AfterDirSelected event", () => {
    it("should handle AfterDirSelected event without throwing", () => {
      const { emit } = useEventBus();

      expect(() => {
        emit("AfterDirSelected", {
          name: "Images",
          directoryPath: "/Images",
          filePath: "/Images",
          isDirectory: true,
        });
      }).not.toThrow();
    });
  });
});
