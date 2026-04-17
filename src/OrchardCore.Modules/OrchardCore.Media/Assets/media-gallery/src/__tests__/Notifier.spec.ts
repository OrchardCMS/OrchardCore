import { describe, expect, it, vi, beforeEach } from "vitest";
import { notify, NotificationMessage, registerNotificationBus } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";

describe("notifier", () => {
  beforeEach(() => {
    registerNotificationBus();
  });

  describe("notify", () => {
    it("should emit an error notification when message is null or undefined", () => {
      vi.spyOn(window.notificationBus, "emit");
      expect(() => notify(null)).not.toThrowError();
      expect(() => notify(undefined)).not.toThrowError();
      expect(window.notificationBus.emit).toHaveBeenCalledTimes(2);
    });

    it("should notify with ProblemDetails-like object", () => {
      const problemDetails = { title: "Test title", detail: "Test detail" };
      vi.spyOn(window.notificationBus, "emit");
      expect(() => notify(problemDetails, SeverityLevel.Error)).not.toThrowError();
      expect(window.notificationBus.emit).toHaveBeenCalledTimes(1);
      expect(window.notificationBus.emit).toHaveBeenCalledWith("notify", {
        summary: "Test title",
        detail: "Test detail",
        severity: SeverityLevel.Error,
      });
    });

    it("should notify with ValidationProblemDetails-like object", () => {
      const validationProblem = {
        title: "Validation Error",
        detail: "Validation failed",
        errors: { field: ["Error 1", "Error 2"] },
      };
      vi.spyOn(window.notificationBus, "emit");
      expect(() => notify(validationProblem, SeverityLevel.Error)).not.toThrowError();
      expect(window.notificationBus.emit).toHaveBeenCalledTimes(1);
      expect(window.notificationBus.emit).toHaveBeenCalledWith("notify", {
        summary: "Validation Error",
        detail: "Error 1\r\nError 2",
        severity: SeverityLevel.Error,
      });
    });

    it("should notify with NotificationMessage", () => {
      const message = new NotificationMessage({
        summary: "Test title",
        detail: "Test detail",
        severity: SeverityLevel.Info,
      });

      vi.spyOn(window.notificationBus, "emit");
      expect(() => notify(message, SeverityLevel.Error)).not.toThrowError();
      expect(window.notificationBus.emit).toHaveBeenCalledTimes(1);
      expect(window.notificationBus.emit).toHaveBeenCalledWith("notify", message);
    });

    it("should notify with Error object", () => {
      const error = new Error("Something went wrong");

      vi.spyOn(window.notificationBus, "emit");
      expect(() => notify(error, SeverityLevel.Error)).not.toThrowError();
      expect(window.notificationBus.emit).toHaveBeenCalledTimes(1);
      expect(window.notificationBus.emit).toHaveBeenCalledWith("notify", {
        summary: "Server Error",
        detail: "Something went wrong",
        severity: SeverityLevel.Error,
      });
    });

    it("should not throw an error if window.notificationBus is not defined", () => {
      const message = new NotificationMessage({
        summary: "Test title",
        detail: "Test detail",
        severity: SeverityLevel.Info,
      });
      delete (window as any).notificationBus; // eslint-disable-line @typescript-eslint/no-explicit-any
      expect(() => notify(message)).not.toThrowError();
    });
  });
});
