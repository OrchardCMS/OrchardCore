import { describe, expect, it, vi } from "vitest";
import { createDesignerApi, DesignerApiError } from "../designerApi";
import type { ApiService } from "@bloom/services/api-service";
import type { DesignerUrls } from "../../config";

const urls: DesignerUrls = {
    definition: "/t/Admin/Workflows/Types/7/Designer/Definition",
    library: "/t/Admin/Workflows/Types/7/Designer/Library",
    save: "/t/Admin/Workflows/Types/7/Designer/Save",
    addActivity: "/t/Admin/Workflows/Types/7/Designer/AddActivity",
    editor: "/t/Admin/Workflows/Types/7/Designer/Editor",
    settings: "/t/Admin/Workflows/Types/7/Designer/Settings",
    publish: "/t/Admin/Workflows/Types/7/Designer/Publish",
    discard: "/t/Admin/Workflows/Types/7/Designer/Discard",
};

const createService = () => ({
    get: vi.fn(),
    post: vi.fn(),
}) as unknown as ApiService & { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn> };

describe("designerApi", () => {
    it("save_Conflict_ThrowsErrorWithProblemDetails", async () => {
        const service = createService();
        service.post.mockRejectedValue({ response: { status: 409, data: { title: "Changed", currentRevision: 5, modifiedBy: "alice" } } });
        const api = createDesignerApi(urls, service);

        const error = await api.save({ revision: 4, nodes: [], transitions: [], removedActivityIds: [], restoredActivityIds: [] }).catch((e) => e);

        expect(error).toBeInstanceOf(DesignerApiError);
        expect(error.isConflict).toBe(true);
        expect(error.problem.currentRevision).toBe(5);
        expect(error.problem.modifiedBy).toBe("alice");
    });

    it("getDefinition_NetworkFailure_ThrowsNetworkError", async () => {
        const service = createService();
        service.get.mockRejectedValue({ message: "Network Error" });
        const api = createDesignerApi(urls, service);

        const error = await api.getDefinition().catch((e) => e);

        expect(error.status).toBe(0);
        expect(error.isNetworkError).toBe(true);
    });

    it("postEditor_Form_PostsMultipartWithActivityAndRevision", async () => {
        const service = createService();
        service.post.mockResolvedValue({ data: { valid: false, content: "<p></p>", scripts: "", styles: "" } });
        const api = createDesignerApi(urls, service);
        const form = new FormData();
        form.append("NotifyTask.Message", "Hi");

        const result = await api.postEditor("a b", 3, form);

        expect(result.valid).toBe(false);
        const [url, body, config] = service.post.mock.calls[0];
        expect(url).toBe(`${urls.editor}?activityId=a+b&revision=3`);
        expect(body).toBe(form);
        expect(config.headers["Content-Type"]).toBe("multipart/form-data");
    });

    it("getEditor_ActivityId_AddsQueryString", async () => {
        const service = createService();
        service.get.mockResolvedValue({ data: { valid: true, content: "", scripts: "", styles: "" } });
        const api = createDesignerApi(urls, service);

        await api.getEditor("abc");

        expect(service.get).toHaveBeenCalledWith(`${urls.editor}?activityId=abc`);
    });
});
