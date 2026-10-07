// The JSON contract of WorkflowDesignerController (camelCase). Keep in sync with
// OrchardCore.Workflows/ViewModels/WorkflowDesigner*.cs and Models/WorkflowDesignIssue.cs.

export type IssueSeverity = "Error" | "Warning";

export interface DesignIssue {
    severity: IssueSeverity;
    code: string;
    message: string;
    activityId?: string | null;
    transitionKey?: string | null;
}

export interface Outcome {
    name: string;
    displayName: string;
}

// How the default value of a variable type is edited; 'none' has no default.
export type VariableEditor = "text" | "number" | "boolean" | "datetime" | "json" | "none";

// A declared workflow variable (WorkflowVariableDefinition.cs).
export interface VariableDefinition {
    name: string;
    typeName: string;
    // Any JSON value, or null for no default.
    defaultValue?: unknown;
    description?: string | null;
    // Set by a workflow that runs this one as an activity, or by the input of the same name.
    isInput?: boolean;
    // Returned to a workflow that runs this one as an activity.
    isOutput?: boolean;
}

// A variable type the designer offers (WorkflowDesignerVariableType).
export interface VariableType {
    name: string;
    displayName: string;
    editor: VariableEditor | string;
}

// What is wrong with a declaration, by its position in the posted list.
export interface VariableError {
    index: number;
    name?: string | null;
    message: string;
}

// An output an activity declares (WorkflowDesignerOutput).
export interface ActivityOutput {
    name: string;
    typeName: string;
    displayName: string;
}

export interface DesignerNode {
    id: string;
    name: string;
    x: number;
    y: number;
    isStart: boolean;
    isEvent: boolean;
    hasEditor: boolean;
    isMissing: boolean;
    title: string;
    displayText: string;
    category: string;
    designHtml: string;
    outcomes: Outcome[];
    icon?: string | null;
    // The outputs the activity declares, and the variable each one is bound to, by output name.
    outputs?: ActivityOutput[];
    outputBindings?: Record<string, string>;
}

export interface DesignerTransition {
    sourceActivityId: string;
    sourceOutcomeName: string;
    destinationActivityId: string;
}

export interface WorkflowSettings {
    name: string;
    isEnabled: boolean;
    isSingleton: boolean;
    lockTimeout: number;
    lockExpiration: number;
    deleteFinishedWorkflows: boolean;
}

// A record of an instance's journal (WorkflowDesignerJournalRecord).
export interface JournalRecord {
    sequence: number;
    activityId: string;
    activityName: string;
    activityTitle?: string | null;
    isResume: boolean;
    status: "Completed" | "Halted" | "Faulted" | string;
    outcomes: string[];
    startedUtc: string;
    completedUtc: string;
    durationMilliseconds: number;
    error?: string | null;
}

// The workflow instance shown by the read-only instance viewer.
export interface DesignerInstance {
    id: number;
    workflowId: string;
    status: string;
    // The activities the instance waits on.
    blockingActivityIds: string[];
    // The stored values of the declared variables that have one, by name.
    variableValues?: Record<string, unknown>;
    faultMessage?: string | null;
    // The activity whose execution faulted the instance.
    faultedActivityId?: string | null;
    // The most recent journal records, oldest first.
    journal?: JournalRecord[];
    // How many times each activity ran, and each transition (by transition key) was taken.
    executedActivityCounts?: Record<string, number>;
    executedTransitionCounts?: Record<string, number>;
}

export interface RetryResult {
    status: string;
    faultMessage?: string | null;
}

// A version of the workflow type: what an instance started on, or a past publish.
export interface DesignerVersion {
    versionId: string;
    version: number;
    // The name of the workflow type when the version was created.
    name: string;
    createdUtc: string;
    createdBy?: string | null;
    // Whether new instances start on this version.
    isPublished: boolean;
    // The number of instances that run on it, when listed.
    instanceCount?: number | null;
}

export interface DesignerDefinition {
    id: number;
    workflowTypeId: string;
    revision: number;
    hasDraft: boolean;
    draftModifiedBy?: string | null;
    draftModifiedByUserId?: string | null;
    draftModifiedUtc?: string | null;
    settings: WorkflowSettings;
    nodes: DesignerNode[];
    transitions: DesignerTransition[];
    issues: DesignIssue[];
    runningInstanceCount: number;
    variables?: VariableDefinition[];
    variableTypes?: VariableType[];
    instance?: DesignerInstance | null;
    // The version new instances start on.
    publishedVersion?: DesignerVersion | null;
    // The version shown by a version page, or the one the viewed instance runs on.
    version?: DesignerVersion | null;
}

export interface DesignerVersions {
    // The most recent first.
    versions: DesignerVersion[];
    draft: { revision: number; modifiedUtc: string; modifiedBy?: string | null } | null;
}

// What changed from one definition to another (WorkflowTypeChanges.cs).
export interface WorkflowTypeChanges {
    addedActivityIds: string[];
    removedActivityIds: string[];
    changedActivityIds: string[];
    movedActivityIds: string[];
    addedTransitionKeys: string[];
    removedTransitionKeys: string[];
    changedSettings: string[];
    hasChanges: boolean;
}

export interface DesignerComparison {
    from: DesignerDefinition;
    to: DesignerDefinition;
    changes: WorkflowTypeChanges;
}

export interface VariablesResult {
    revision: number;
    variables: VariableDefinition[];
    issues: DesignIssue[];
}

export interface OutputBindingsResult {
    revision: number;
    node: DesignerNode;
    issues: DesignIssue[];
}

export interface RestoreResult {
    revision: number;
    issues: DesignIssue[];
}

export interface LibraryActivity {
    name: string;
    // The id of a preset of the activity (ActivityPreset), which adds it with preset properties.
    preset?: string | null;
    displayText: string;
    category: string;
    isEvent: boolean;
    hasEditor: boolean;
    thumbnailHtml: string;
    icon?: string | null;
}

export interface LibraryCategory {
    name: string;
    activities: LibraryActivity[];
}

export interface Library {
    categories: LibraryCategory[];
}

export interface SaveNode {
    id: string;
    x: number;
    y: number;
    isStart: boolean;
}

export interface SavePayload {
    revision: number;
    nodes: SaveNode[];
    transitions: DesignerTransition[];
    removedActivityIds: string[];
    restoredActivityIds: string[];
}

export interface SaveResult {
    revision: number;
    issues: DesignIssue[];
}

export interface AddActivityResult {
    revision: number;
    node: DesignerNode;
    issues: DesignIssue[];
}

// A server-rendered form (activity editor or workflow settings).
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
    removedTransitions: DesignerTransition[];
    issues: DesignIssue[];
}

export interface SettingsApplied {
    valid: true;
    revision: number;
    settings: WorkflowSettings;
    issues: DesignIssue[];
}

// An invalid post returns the form again, rendered with its errors.
export type EditorApplyResult = EditorApplied | (FormFragment & { valid: false });

export type SettingsApplyResult = SettingsApplied | (FormFragment & { valid: false });

export interface PublishResult {
    issues: DesignIssue[];
    publishedUtc: string;
    // The version the publish created (or the published one, when nothing that runs changed).
    version?: DesignerVersion | null;
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
    // Invalid variable declarations (the Variables endpoint).
    variableErrors?: VariableError[];
}
