export type IssueSeverity = "Error" | "Warning";

/**
 * A problem of a definition the server found, such as a node without its required connection. A designer's issues
 * may also target a node, through a property of their own, such as `activityId` or `stepId`.
 */
export interface DesignIssue {
    severity: IssueSeverity;
    message: string;
    code?: string | null;
}

/**
 * The autosave status: `offline` is retrying after a network or server error, `failed` was rejected by the server and
 * waits for a retry, and `conflict` waits for the user to reload or overwrite.
 */
export type SaveStatus = "saved" | "saving" | "unsaved" | "offline" | "failed" | "conflict";

/**
 * The state every designer has to save its draft: the revision the draft is at, the save status, and a counter of the
 * changes of the graph, which autosave compares with the one it last saved.
 */
export interface DraftState {
    revision: number;
    saveStatus: SaveStatus;
    changeVersion: number;
}

/**
 * A store autosave can save: it builds the body of a Save request, and records a successful one.
 */
export interface SavableStore<TPayload, TResult> {
    state: DraftState;
    toSavePayload(): TPayload;
    markSaved(payload: TPayload, result: TResult): void;
}

/**
 * A published version, as the Versions dialog lists it.
 */
export interface VersionSummary {
    versionId: string;
    // The number of the version, from 1.
    number: number;
    // Whether it is the version in use.
    isPublished?: boolean;
}

/**
 * The draft of a definition, as the Versions dialog lists it.
 */
export interface DraftSummary {
    modifiedUtc: string;
    modifiedBy?: string | null;
}

/**
 * The published versions of a definition, the most recent first, and its draft if it has one.
 */
export interface VersionsData<TVersion extends VersionSummary = VersionSummary> {
    versions: TVersion[];
    draft: DraftSummary | null;
}

/**
 * A server-rendered form, with the scripts and styles it needs.
 */
export interface FormFragment {
    valid: boolean;
    content: string;
    scripts: string;
    styles: string;
}
