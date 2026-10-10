// The configuration rendered by Views/DataPipeline/Designer.cshtml into the data-config attribute of
// #data-pipeline-designer. Every URL is generated on the server, so it includes the tenant prefix.

// The JSON endpoints of DataPipelineDesignerController; those a mode doesn't use are null.
export interface DesignerUrls {
    definition: string;
    library: string;
    save?: string | null;
    addStep?: string | null;
    // GET ?stepId= renders the step editor; POST ?stepId=&revision= applies it (multipart form).
    editor?: string | null;
    // GET renders the pipeline settings form; POST ?revision= applies it (multipart form).
    settings?: string | null;
    // GET ?stepId= returns the fields flowing into and out of the step.
    fields: string;
    // POST { stepId } previews the step.
    preview?: string | null;
    publish?: string | null;
    discard?: string | null;
    versions?: string | null;
    // POST { versionId, revision } restores a version into the draft.
    restore?: string | null;
    // POST {} queues a run of the published version.
    run?: string | null;
    // GET returns the most recent runs; GET ?runId= returns one run.
    runs?: string | null;
    // POST { runId } cancels a run.
    cancelRun?: string | null;
}

/**
 * - `designer`: edits the pipeline's draft;
 * - `version`: shows a published version, read-only.
 */
export type DesignerMode = "designer" | "version";

export interface DesignerConfig {
    pipelineId: string;
    mode?: DesignerMode;
    readOnly: boolean;
    urls: DesignerUrls;
    // The pages: the list of pipelines, the designer, a version (with a versionId query string parameter), and the
    // runs of the pipeline.
    listUrl: string;
    designerUrl?: string | null;
    versionPageUrl?: string | null;
    runsPageUrl?: string | null;
    // The signed-in user's id, to tell whether someone else last edited the draft.
    currentUserId?: string | null;
    // The step to select and open when the designer loads.
    initialStepId?: string | null;
    translations: Record<string, string>;
}

export const readConfig = (element: HTMLElement): DesignerConfig => {
    const raw = element.dataset.config;

    if (!raw) {
        throw new Error("The data pipeline designer element has no data-config attribute.");
    }

    return JSON.parse(raw) as DesignerConfig;
};
