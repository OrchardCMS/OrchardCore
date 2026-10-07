import { createApiService, type ApiService } from "@bloom/services/api-service";
import { PRESET_KEY_PREFIX } from "./presets";
import type { DesignerUrls } from "../config";
import type {
    AddActivityResult,
    DesignerComparison,
    DesignerDefinition,
    DesignerVersions,
    EditorApplyResult,
    FormFragment,
    Library,
    OutputBindingsResult,
    ProblemDetails,
    PublishResult,
    RestoreResult,
    RetryResult,
    SavePayload,
    SaveResult,
    SettingsApplyResult,
    VariableDefinition,
    VariablesResult,
} from "./types";

/**
 * An HTTP failure of a designer endpoint, with the ProblemDetails body when the server sent one.
 * `status` is 0 when the request never reached the server (offline, network error).
 */
export class DesignerApiError extends Error {
    readonly status: number;
    readonly problem: ProblemDetails;

    constructor(status: number, problem: ProblemDetails | undefined, message?: string) {
        super(problem?.title ?? message ?? `Request failed with status ${status}`);
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

/**
 * The typed client of WorkflowDesignerController. The endpoint URLs are generated on the server, so they
 * include the tenant prefix.
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
        save: (payload: SavePayload) => call(() => service.post<SaveResult>(urls.save, payload)),
        /**
         * Adds an activity by its name, or a preset by its toolbox key (see toolboxKey).
         */
        addActivity: (revision: number, name: string, x: number, y: number) =>
            call(() =>
                service.post<AddActivityResult>(
                    urls.addActivity,
                    name.startsWith(PRESET_KEY_PREFIX) ? { revision, preset: name.slice(PRESET_KEY_PREFIX.length), x, y } : { revision, name, x, y },
                ),
            ),
        getEditor: (activityId: string) => call(() => service.get<FormFragment>(withQuery(urls.editor, { activityId }))),
        postEditor: (activityId: string, revision: number, form: FormData) =>
            call(() => service.post<EditorApplyResult>(withQuery(urls.editor, { activityId, revision }), form, multipart)),
        getSettings: () => call(() => service.get<FormFragment>(urls.settings)),
        postSettings: (revision: number, form: FormData) =>
            call(() => service.post<SettingsApplyResult>(withQuery(urls.settings, { revision }), form, multipart)),
        publish: (revision: number) => call(() => service.post<PublishResult>(urls.publish, { revision })),
        discard: () => call(() => service.post<Record<string, never>>(urls.discard, {})),
        getVersions: () => call(() => service.get<DesignerVersions>(urls.versions!)),
        restore: (versionId: string, revision: number) => call(() => service.post<RestoreResult>(urls.restore!, { versionId, revision })),
        getComparison: () => call(() => service.get<DesignerComparison>(urls.compare!)),
        saveVariables: (revision: number, variables: VariableDefinition[]) =>
            call(() => service.post<VariablesResult>(urls.variables!, { revision, variables })),
        retry: (instanceId: number, activityId: string) => call(() => service.post<RetryResult>(urls.retry!, { instanceId, activityId })),
        saveOutputBindings: (activityId: string, revision: number, bindings: Record<string, string>) =>
            call(() => service.post<OutputBindingsResult>(urls.outputBindings!, { activityId, revision, bindings })),
    };
};

export type DesignerApi = ReturnType<typeof createDesignerApi>;
