import { createApiService, type ApiService } from "@bloom/services/api-service";
import { DesignerApiError, call, multipart, withQuery } from "@bloom/designer/core/api";
import { PRESET_KEY_PREFIX } from "./presets";
import type { DesignerUrls } from "../config";
import type {
    AddActivityResult,
    DesignerComparison,
    DesignerDefinition,
    DesignerVersions,
    EditorApplyResult,
    FormFragment,
    HttpRunResponse,
    JournalData,
    Library,
    OutputBindingsResult,
    PublishResult,
    RestoreResult,
    RetryResult,
    RunResult,
    SavePayload,
    SaveResult,
    SettingsApplyResult,
    VariableDefinition,
    VariablesResult,
} from "./types";

export { DesignerApiError, withQuery };

/**
 * The typed client of WorkflowDesignerController. The endpoint URLs are generated on the server, so they
 * include the tenant prefix.
 */
export const createDesignerApi = (urls: DesignerUrls, service: ApiService = createApiService({ authType: "cookie" })) => {
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
        run: (inputs: Record<string, unknown>) => call(() => service.post<RunResult>(urls.run!, { inputs })),
        getLatestInstance: () => call(() => service.get<RunResult>(urls.latestInstance!)),
        /**
         * Generates the URL of an HTTP Request event, valid for a day.
         */
        generateHttpUrl: (workflowTypeId: number, activityId: string) =>
            call(() => service.post<string>(withQuery(urls.generateHttpUrl!, { workflowTypeId, activityId, tokenLifeSpan: 1 }), {})),
        /**
         * Sends the request of a run to an HTTP Request event's URL, as a client would.
         */
        sendHttpRequest: async (url: string, method: string, body: string | null, contentType: string): Promise<HttpRunResponse> => {
            try {
                const response = await fetch(url, {
                    method,
                    credentials: "same-origin",
                    headers: body === null ? undefined : { "Content-Type": contentType },
                    body,
                });

                return { status: response.status, body: await response.text() };
            } catch (error) {
                throw new DesignerApiError(0, undefined, (error as Error)?.message);
            }
        },
        getJournalData: (sequence: number) => call(() => service.get<JournalData>(withQuery(urls.journalData!, { sequence }))),
        saveOutputBindings: (activityId: string, revision: number, bindings: Record<string, string>) =>
            call(() => service.post<OutputBindingsResult>(urls.outputBindings!, { activityId, revision, bindings })),
    };
};

export type DesignerApi = ReturnType<typeof createDesignerApi>;
