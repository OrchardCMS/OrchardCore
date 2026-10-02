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

// The workflow instance shown by the read-only instance viewer.
export interface DesignerInstance {
    id: number;
    workflowId: string;
    status: string;
    // The activities the instance waits on.
    blockingActivityIds: string[];
    // TODO: Phase 5 records the executed activities; the viewer then highlights the executed path.
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
    instance?: DesignerInstance | null;
}

export interface LibraryActivity {
    name: string;
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
