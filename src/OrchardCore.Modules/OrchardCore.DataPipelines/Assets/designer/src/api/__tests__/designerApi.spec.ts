import { describe, expect, it, vi } from "vitest";
import { createDesignerApi, DesignerApiError } from "../designerApi";
import type { ApiService } from "@bloom/services/api-service";
import type { DesignerUrls } from "../../config";

const base = "/t/Admin/DataPipelines/p1/Designer";

const urls: DesignerUrls = {
    definition: `${base}/Definition`,
    library: `${base}/Library`,
    save: `${base}/Save`,
    addStep: `${base}/AddStep`,
    editor: `${base}/Editor`,
    settings: `${base}/Settings`,
    fields: `${base}/Fields`,
    preview: `${base}/Preview`,
    publish: `${base}/Publish`,
    discard: `${base}/Discard`,
    versions: `${base}/Versions`,
    restore: `${base}/Restore`,
    run: `${base}/Run`,
    runs: `${base}/Runs`,
    cancelRun: `${base}/CancelRun`,
};

const createService = () =>
    ({
        get: vi.fn(),
        post: vi.fn(),
    }) as unknown as ApiService & { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn> };

describe("designerApi", () => {
    it("save_Conflict_ThrowsErrorWithProblemDetails", async () => {
        const service = createService();
        service.post.mockRejectedValue({ response: { status: 409, data: { title: "Changed", currentRevision: 5, modifiedBy: "alice" } } });
        const api = createDesignerApi(urls, service);

        const error = await api.save({ revision: 4, nodes: [], connections: [], removedStepIds: [] }).catch((e) => e);

        expect(error).toBeInstanceOf(DesignerApiError);
        expect(error.isConflict).toBe(true);
        expect(error.problem).toMatchObject({ currentRevision: 5, modifiedBy: "alice" });
    });

    it("getDefinition_NetworkFailure_ThrowsNetworkError", async () => {
        const service = createService();
        service.get.mockRejectedValue({ message: "Network Error" });

        const error = await createDesignerApi(urls, service)
            .getDefinition()
            .catch((e) => e);

        expect(error.isNetworkError).toBe(true);
    });

    it("postEditor_Form_PostsMultipartWithStepAndRevision", async () => {
        const service = createService();
        service.post.mockResolvedValue({ data: { valid: false, content: "", scripts: "", styles: "" } });
        const form = new FormData();

        await createDesignerApi(urls, service).postEditor("a b", 3, form);

        const [url, body, config] = service.post.mock.calls[0];
        expect(url).toBe(`${urls.editor}?stepId=a+b&revision=3`);
        expect(body).toBe(form);
        expect(config.headers["Content-Type"]).toBe("multipart/form-data");
    });

    it("endpoints_QueryAndBodies_FollowTheContract", async () => {
        const service = createService();
        service.get.mockResolvedValue({ data: {} });
        service.post.mockResolvedValue({ data: {} });
        const api = createDesignerApi(urls, service);

        await api.addStep({ revision: 2, type: "FilterStep", x: 10, y: 20, connectFrom: { stepId: "s", port: "Output" } });
        await api.preview("s");
        await api.publish(7);
        await api.restore("v1", 8);
        await api.run();
        await api.cancelRun("r1");
        await api.getFields("s");
        await api.getRun("r1");
        await api.getRuns();

        expect(service.post.mock.calls.map(([url, body]) => [url, body])).toEqual([
            [urls.addStep, { revision: 2, type: "FilterStep", x: 10, y: 20, connectFrom: { stepId: "s", port: "Output" } }],
            [urls.preview, { stepId: "s" }],
            [urls.publish, { revision: 7 }],
            [urls.restore, { versionId: "v1", revision: 8 }],
            [urls.run, {}],
            [urls.cancelRun, { runId: "r1" }],
        ]);
        expect(service.get.mock.calls.map(([url]) => url)).toEqual([`${urls.fields}?stepId=s`, `${urls.runs}?runId=r1`, urls.runs]);
    });

    it("endpoint_NotAvailableInTheMode_Throws", async () => {
        const api = createDesignerApi({ ...urls, save: null }, createService());

        await expect(api.save({ revision: 0, nodes: [], connections: [], removedStepIds: [] })).rejects.toThrow("save");
    });
});
