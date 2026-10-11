import { createApiService, type ApiService } from "@bloom/services/api-service";
import type { DesignerUrls } from "../config";
import type {
    AddStepPayload,
    AddStepResult,
    DesignerDefinition,
    DesignerRun,
    DesignerVersions,
    EditorApplyResult,
    FormFragment,
    Library,
    PreviewResult,
    ProblemDetails,
    PublishResult,
    RestoreResult,
    RunsResult,
    SavePayload,
    SaveResult,
    SettingsApplyResult,
    StepFields,
} from "./types";

/**
 * An HTTP failure of a designer endpoint, with the ProblemDetails body when the server sent one.
 * `status` is 0 when the request never reached the server (offline, network error).
 */
export class DesignerApiError extends Error {
    readonly status: number;
    readonly problem: ProblemDetails;

    constructor(status: number, problem: ProblemDetails | undefined, message?: string) {
        super(problem?.detail ?? problem?.title ?? message ?? `Request failed with status ${status}`);
        this.name = "DesignerApiError";
        this.status = status;
        this.problem = problem ?? {};
    }

    get isConflict() {
        return this.status === 409;
    }

    get isNetworkError() {
        return this.status === 0;
    }
}

interface AxiosLikeError {
    message?: string;
    response?: { status: number; data?: unknown };
}

const toDesignerApiError = (error: unknown): DesignerApiError => {
    const axiosError = error as AxiosLikeError;

    if (axiosError?.response) {
        const data = axiosError.response.data;

        return new DesignerApiError(axiosError.response.status, typeof data === "object" && data !== null ? (data as ProblemDetails) : undefined);
    }

    return new DesignerApiError(0, undefined, axiosError?.message);
};

export const withQuery = (url: string, query: Record<string, string | number>) => {
    const separator = url.includes("?") ? "&" : "?";

    return url + separator + new URLSearchParams(Object.entries(query).map(([key, value]) => [key, String(value)])).toString();
};

// An endpoint the mode doesn't have (its URL is null) is a programming error, not a server failure.
const required = (url: string | null | undefined, name: string) => {
    if (!url) {
        throw new Error(`The ${name} endpoint is not available in this mode.`);
    }

    return url;
};

/**
 * The typed client of DataPipelineDesignerController. The endpoint URLs are generated on the server, so they include
 * the tenant prefix. The cookie service sends the page's antiforgery token in the RequestVerificationToken header.
 */
export const createDesignerApi = (urls: DesignerUrls, service: ApiService = createApiService({ authType: "cookie" })) => {
    const call = async <T>(request: () => Promise<{ data: T }>): Promise<T> => {
        try {
            return (await request()).data;
        } catch (error) {
            throw toDesignerApiError(error);
        }
    };

    // Forms are posted as multipart; the explicit header stops axios from converting the FormData to JSON
    // (the service defaults to application/json) and lets the browser add the boundary.
    const multipart = { headers: { "Content-Type": "multipart/form-data" } };

    return {
        getDefinition: () => call(() => service.get<DesignerDefinition>(urls.definition)),
        getLibrary: () => call(() => service.get<Library>(urls.library)),
        save: (payload: SavePayload) => call(() => service.post<SaveResult>(required(urls.save, "save"), payload)),
        addStep: (payload: AddStepPayload) => call(() => service.post<AddStepResult>(required(urls.addStep, "addStep"), payload)),
        getEditor: (stepId: string) => call(() => service.get<FormFragment>(withQuery(required(urls.editor, "editor"), { stepId }))),
        postEditor: (stepId: string, revision: number, form: FormData) =>
            call(() => service.post<EditorApplyResult>(withQuery(required(urls.editor, "editor"), { stepId, revision }), form, multipart)),
        getSettings: () => call(() => service.get<FormFragment>(required(urls.settings, "settings"))),
        postSettings: (revision: number, form: FormData) =>
            call(() => service.post<SettingsApplyResult>(withQuery(required(urls.settings, "settings"), { revision }), form, multipart)),
        getFields: (stepId: string) => call(() => service.get<StepFields>(withQuery(urls.fields, { stepId }))),
        preview: (stepId: string) => call(() => service.post<PreviewResult>(required(urls.preview, "preview"), { stepId })),
        publish: (revision: number) => call(() => service.post<PublishResult>(required(urls.publish, "publish"), { revision })),
        discard: () => call(() => service.post<Record<string, never>>(required(urls.discard, "discard"), {})),
        getVersions: () => call(() => service.get<DesignerVersions>(required(urls.versions, "versions"))),
        restore: (versionId: string, revision: number) =>
            call(() => service.post<RestoreResult>(required(urls.restore, "restore"), { versionId, revision })),
        /**
         * Queues a run of the published version; the server answers with the queued run.
         */
        run: () => call(() => service.post<DesignerRun>(required(urls.run, "run"), {})),
        getRuns: () => call(() => service.get<RunsResult>(required(urls.runs, "runs"))),
        getRun: (runId: string) => call(() => service.get<DesignerRun>(withQuery(required(urls.runs, "runs"), { runId }))),
        cancelRun: (runId: string) => call(() => service.post<DesignerRun>(required(urls.cancelRun, "cancelRun"), { runId })),
    };
};

export type DesignerApi = ReturnType<typeof createDesignerApi>;
