import { createApiService, type ApiService } from "@bloom/services/api-service";
import { DesignerApiError, call, multipart, required, withQuery } from "@bloom/designer/core/api";
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
    PublishResult,
    RestoreResult,
    RunsResult,
    SavePayload,
    SaveResult,
    SettingsApplyResult,
    StepFields,
} from "./types";

export { DesignerApiError, withQuery };

/**
 * The typed client of DataPipelineDesignerController. The endpoint URLs are generated on the server, so they include
 * the tenant prefix. The cookie service sends the page's antiforgery token in the RequestVerificationToken header.
 */
export const createDesignerApi = (urls: DesignerUrls, service: ApiService = createApiService({ authType: "cookie" })) => {
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
