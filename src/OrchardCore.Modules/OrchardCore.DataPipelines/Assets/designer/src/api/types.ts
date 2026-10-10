// The JSON contract of DataPipelineDesignerController (camelCase). Keep in sync with
// OrchardCore.DataPipelines/ViewModels/Designer/*.cs.

export type IssueSeverity = "Error" | "Warning";

// A problem of the pipeline (DataPipelineIssue): stepId is null for the whole pipeline.
export interface DesignIssue {
    severity: IssueSeverity;
    message: string;
    stepId?: string | null;
}

// What flows through a port: batches of rows, or files.
export type PortKind = "Records" | "Files";

export interface DesignerPort {
    name: string;
    displayName: string;
    kind: PortKind;
    // An input that must be connected for the pipeline to run.
    isRequired: boolean;
    // An input that accepts several connections, read one after the other.
    allowsMany: boolean;
}

export type StepCategory = "Source" | "Transform" | "File" | "Destination";

// The logical type of a field (DataFieldType).
export type FieldType = "Text" | "Integer" | "Decimal" | "Boolean" | "Date" | "DateTime";

export interface DesignerField {
    name: string;
    displayName: string;
    type: FieldType;
    group?: string | null;
}

export interface DesignerNode {
    id: string;
    // The step type name.
    type: string;
    // The title set by the user, or null.
    title?: string | null;
    // The title, or the display name of the step type.
    displayText: string;
    category: StepCategory;
    icon?: string | null;
    // The step type is not available (its feature is disabled).
    isMissing: boolean;
    hasEditor: boolean;
    x: number;
    y: number;
    inputs: DesignerPort[];
    outputs: DesignerPort[];
    // A short summary of the step's settings, rendered on the server.
    designHtml: string;
}

export interface DesignerConnection {
    sourceStepId: string;
    sourcePort: string;
    targetStepId: string;
    targetPort: string;
}

export interface PipelineSettings {
    name: string;
    description?: string | null;
}

// A published version of the pipeline.
export interface DesignerVersion {
    versionId: string;
    number: number;
    publishedUtc: string;
    publishedBy?: string | null;
    // Whether it is the version runs use.
    isPublished: boolean;
}

export type RunStatus = "Queued" | "Running" | "Succeeded" | "Failed" | "Cancelled";

export type StepStatus = "Pending" | "Running" | "Succeeded" | "Failed" | "Cancelled" | "Skipped";

export interface RunStep {
    stepId: string;
    title: string;
    status: StepStatus;
    rowsIn: number;
    rowsOut: number;
    filesIn: number;
    filesOut: number;
    warnings: number;
    error?: string | null;
}

export interface RunDelivery {
    stepId?: string | null;
    description: string;
    url?: string | null;
    deliveredUtc: string;
}

export interface RunLogEntry {
    utc: string;
    level: "Information" | "Warning" | "Error";
    stepId?: string | null;
    message: string;
}

// A run of the pipeline (DataPipelineRun).
export interface DesignerRun {
    runId: string;
    status: RunStatus;
    versionNumber: number;
    trigger: string;
    triggeredBy?: string | null;
    queuedUtc: string;
    startedUtc?: string | null;
    completedUtc?: string | null;
    error?: string | null;
    failedStepId?: string | null;
    steps: RunStep[];
    deliveries: RunDelivery[];
    log: RunLogEntry[];
    // The page of the run in the admin.
    url: string;
    canCancel: boolean;
}

export interface DesignerDefinition {
    pipelineId: string;
    revision: number;
    hasDraft: boolean;
    draftModifiedBy?: string | null;
    draftModifiedByUserId?: string | null;
    draftModifiedUtc?: string | null;
    settings: PipelineSettings;
    nodes: DesignerNode[];
    connections: DesignerConnection[];
    issues: DesignIssue[];
    // The version runs use, or null when the pipeline was never published.
    publishedVersion?: DesignerVersion | null;
    // The version shown by a version page.
    version?: DesignerVersion | null;
    // Whether the user can run the pipeline.
    canRun: boolean;
    // The most recent run, if any.
    lastRun?: DesignerRun | null;
}

export interface DesignerVersions {
    // The most recent first.
    versions: DesignerVersion[];
    draft: { revision: number; modifiedUtc: string; modifiedBy?: string | null } | null;
}

export interface LibraryStep {
    // The step type name.
    name: string;
    displayText: string;
    description: string;
    category: StepCategory;
    icon?: string | null;
    // The ports of a new step of this type, so the toolbox and the quick-add list only offer steps that can connect.
    inputs: DesignerPort[];
    outputs: DesignerPort[];
}

export interface LibraryCategory {
    // The category value, such as "Source".
    category: StepCategory;
    displayName: string;
    steps: LibraryStep[];
}

export interface Library {
    categories: LibraryCategory[];
}

export interface SaveNode {
    id: string;
    x: number;
    y: number;
}

export interface SavePayload {
    revision: number;
    nodes: SaveNode[];
    connections: DesignerConnection[];
    removedStepIds: string[];
    // The steps removed by an earlier save that the user brought back (undo): the server restores them with their
    // settings. Optional: added by the designer.
    restoredStepIds?: string[];
}

export interface SaveResult {
    revision: number;
    issues: DesignIssue[];
}

export interface AddStepPayload {
    revision: number;
    type: string;
    x: number;
    y: number;
    // Connects the new step's first compatible input to this output, when set.
    connectFrom?: { stepId: string; port: string } | null;
}

export interface AddStepResult {
    revision: number;
    node: DesignerNode;
    // The connection made from connectFrom, if any.
    connection?: DesignerConnection | null;
    issues: DesignIssue[];
}

// A server-rendered form (step editor or pipeline settings).
export interface FormFragment {
    valid: boolean;
    content: string;
    scripts: string;
    styles: string;
}

export interface EditorApplied {
    valid: true;
    revision: number;
    node: DesignerNode;
    // Connections removed because the step no longer has their port.
    removedConnections: DesignerConnection[];
    issues: DesignIssue[];
    // The editor must be loaded again (GET editor), because the applied settings change the form itself, such as the
    // fields of a data set that was just chosen.
    reloadEditor?: boolean;
}

export interface SettingsApplied {
    valid: true;
    revision: number;
    settings: PipelineSettings;
    issues: DesignIssue[];
}

// An invalid post returns the form again, rendered with its errors.
export type EditorApplyResult = EditorApplied | (FormFragment & { valid: false });

export type SettingsApplyResult = SettingsApplied | (FormFragment & { valid: false });

export interface PublishResult {
    issues: DesignIssue[];
    publishedUtc: string;
    version?: DesignerVersion | null;
}

export interface RestoreResult {
    revision: number;
    issues: DesignIssue[];
}

// The fields flowing into and out of a step, for the Fields tab (DataPipelineStepFields).
export interface StepPortFields {
    port: string;
    displayName: string;
    kind: PortKind;
    fields: DesignerField[];
}

export interface StepFields {
    stepId: string;
    inputs: StepPortFields[];
    outputs: StepPortFields[];
}

export interface PreviewFile {
    fileName: string;
    contentType: string;
    length: number;
    rowCount?: number | null;
}

export interface PreviewPort {
    name: string;
    displayName: string;
    kind: PortKind;
    fields: DesignerField[];
    // The values of each row, aligned with fields: strings, numbers, booleans, ISO dates, or null.
    rows: (string | number | boolean | null)[][];
    truncated: boolean;
    files: PreviewFile[];
}

// The first rows a step produces, or receives when it is a destination (DataPipelinePreview).
export interface PreviewResult {
    stepId: string;
    error?: string | null;
    // For a destination: the preview shows what it would receive, without executing it.
    isInputPreview: boolean;
    ports: PreviewPort[];
    issues: DesignIssue[];
    durationMilliseconds: number;
}

export interface RunsResult {
    // The most recent first.
    runs: DesignerRun[];
}

// RFC 9457 ProblemDetails with the designer extensions.
export interface ProblemDetails {
    status?: number;
    title?: string;
    detail?: string;
    currentRevision?: number;
    modifiedBy?: string | null;
    modifiedUtc?: string | null;
    issues?: DesignIssue[];
}
