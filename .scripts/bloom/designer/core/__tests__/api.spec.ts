import { describe, expect, it } from "vitest";
import { DesignerApiError, call, required, toDesignerApiError, withQuery } from "../api";

describe("api", () => {
    it("toDesignerApiError_ConflictResponse_KeepsTheProblem", () => {
        const error = toDesignerApiError({ response: { status: 409, data: { title: "Conflict", detail: "Changed elsewhere.", currentRevision: 7 } } });

        expect(error.isConflict).toBe(true);
        expect(error.isNetworkError).toBe(false);
        expect(error.problem.currentRevision).toBe(7);
        expect(error.message).toBe("Changed elsewhere.");
    });

    it("toDesignerApiError_NoResponse_IsANetworkError", () => {
        const error = toDesignerApiError(new Error("Network Error"));

        expect(error.status).toBe(0);
        expect(error.isNetworkError).toBe(true);
        expect(error.message).toBe("Network Error");
    });

    it("toDesignerApiError_DesignerApiError_ReturnsIt", () => {
        const error = new DesignerApiError(500, { title: "Failed" });

        expect(toDesignerApiError(error)).toBe(error);
    });

    it("call_FailedRequest_ThrowsADesignerApiError", async () => {
        await expect(call(() => Promise.reject({ response: { status: 404 } }))).rejects.toMatchObject({ status: 404 });
        await expect(call(() => Promise.resolve({ data: 42 }))).resolves.toBe(42);
    });

    it("withQuery_UrlWithQuery_AppendsTheValues", () => {
        expect(withQuery("/designer/editor", { activityId: "a b" })).toBe("/designer/editor?activityId=a+b");
        expect(withQuery("/designer?id=1", { revision: 2 })).toBe("/designer?id=1&revision=2");
    });

    it("required_MissingUrl_Throws", () => {
        expect(required("/versions", "versions")).toBe("/versions");
        expect(() => required(null, "versions")).toThrow("versions");
    });
});
