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

// A field of a value an activity provides.
export interface ProvidedValueMember {
    name: string;
    typeName: string;
    description?: string | null;
}

// A value or function every expression can use (WorkflowDesignerGlobalValue): a Liquid value has a Liquid path,
// a function a JavaScript call.
export interface GlobalValue {
    kind: "Value" | "Function";
    name: string;
    typeName: string;
    description?: string | null;
    liquidPath?: string | null;
    javaScript?: string | null;
    members?: ProvidedValueMember[];
}

// A value an activity provides to the activities after it (WorkflowDesignerProvidedValue).
export interface ProvidedValue {
    // LastResult: what the activity sets as the last result, which the activities right after it read.
    source: "Input" | "Output" | "Properties" | "LastResult";
    name: string;
    typeName: string;
    description?: string | null;
    // The fields scripts and Liquid templates read, for example the ContentType of a content item.
    members?: ProvidedValueMember[];
    // Whether the expressions of the activity that provides the value can read it too.
    availableToItself?: boolean;
}

// An output an activity declares (WorkflowDesignerOutput).
export interface ActivityOutput {
    name: string;
    typeName: string;
    displayName: string;
    // What the output is, shown under its name.
    description?: string | null;
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
    // How many times the task is retried when it faults, by its retry policy.
    retries?: number;
    title: string;
    displayText: string;
    category: string;
    designHtml: string;
    outcomes: Outcome[];
    icon?: string | null;
    // The outputs the activity declares, and the variable each one is bound to, by output name.
    outputs?: ActivityOutput[];
    outputBindings?: Record<string, string>;
    // The values the activity provides to the activities after it.
    providedValues?: ProvidedValue[];
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
    isSingletonPerCorrelation?: boolean;
    lockTimeout: number;
    lockExpiration: number;
    deleteFinishedWorkflows: boolean;
    isActivity?: boolean;
    // How an outcome with several transitions is followed (WorkflowBranchingMode).
    branchingMode?: "FirstOnly" | "All";
    faultOnScriptErrors?: boolean;
}

// A record of an instance's journal (WorkflowDesignerJournalRecord).
export interface JournalRecord {
    sequence: number;
    activityId: string;
    activityName: string;
    activityTitle?: string | null;
    isResume: boolean;
    status: "Completed" | "Halted" | "Faulted" | "Retrying" | "Failed" | string;
    outcomes: string[];
    startedUtc: string;
    completedUtc: string;
    durationMilliseconds: number;
    error?: string | null;
    // Whether the record has the data of the execution, which the viewer loads when it's opened.
    hasData?: boolean;
    // The instance the activity started, or whose result it resumed with, and its page when it still exists.
    childWorkflowId?: string | null;
    childInstanceUrl?: string | null;
}

// An expression an activity evaluated, and its result (WorkflowExpressionEvaluation).
export interface JournalEvaluation {
    // The activity property, such as "Condition" or "Inputs.amount", when it's known.
    property?: string | null;
    syntax?: string | null;
    expression: string;
    // The result, as JSON text.
    result: string;
}

// What an activity evaluated, set and changed in one execution (WorkflowExecutionData). Values are JSON text.
export interface JournalData {
    evaluations: JournalEvaluation[];
    outputs: Record<string, string>;
    variables: Record<string, string>;
    properties: Record<string, string>;
    lastResult?: string | null;
    // Whether values were cut or left out to keep the record small.
    isTruncated: boolean;
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
    // The next attempt of the task a faulted instance is retried from, by the task's retry policy.
    pendingRetry?: PendingRetry | null;
    // The instance that runs this one as an activity, and its page when it still exists.
    parentWorkflowId?: string | null;
    parentInstanceUrl?: string | null;
    // The most recent journal records, oldest first.
    journal?: JournalRecord[];
    // How many times each activity ran, and each transition (by transition key) was taken.
    executedActivityCounts?: Record<string, number>;
    executedTransitionCounts?: Record<string, number>;
}

// The next attempt of a task that faulted and is retried later (WorkflowPendingRetry).
export interface PendingRetry {
    activityId: string;
    // How many attempts failed so far, the first one included.
    failedAttempts: number;
    maxRetries: number;
    dueUtc: string;
}

// How the Run dialog runs the published version (WorkflowDesignerRun): with its inputs when it starts with Started by
// Workflow, with a request to its URL when it starts with an HTTP Request event, or not at all.
export interface DesignerRun {
    mode: "inputs" | "http" | null;
    isEnabled: boolean;
    activityId?: string | null;
    httpMethod?: string | null;
    inputs: VariableDefinition[];
}

// What a run started (WorkflowDesignerRunResult).
export interface RunResult {
    // Null when the instance was deleted once finished, or when there's none.
    instanceId?: number | null;
    instanceUrl?: string | null;
    status?: string | null;
    faultMessage?: string | null;
    outputs?: Record<string, unknown>;
}

// The answer to the request the Run dialog sent to an HTTP Request event.
export interface HttpRunResponse {
    status: number;
    body: string;
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
    globalValues?: GlobalValue[];
    instance?: DesignerInstance | null;
    // The version new instances start on.
    publishedVersion?: DesignerVersion | null;
    // How the designer runs the published version (WorkflowDesignerRun).
    run?: DesignerRun | null;
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
