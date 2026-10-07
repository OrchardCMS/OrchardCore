// The configuration rendered by Views/WorkflowType/Designer.cshtml into the data-config attribute of
// #workflow-designer. Every URL is generated on the server, so it includes the tenant prefix.

// The JSON endpoints; those a mode doesn't use are null.
export interface DesignerUrls {
    definition: string;
    library: string;
    save: string;
    addActivity: string;
    editor: string;
    settings: string;
    publish: string;
    discard: string;
    versions?: string | null;
    restore?: string | null;
    compare?: string | null;
    variables?: string | null;
    outputBindings?: string | null;
}

/**
 * - `designer`: edits the workflow type;
 * - `instance`: shows an instance, read-only;
 * - `version`: shows a version, read-only;
 * - `compare`: shows two definitions side by side.
 */
export type DesignerMode = "designer" | "instance" | "version" | "compare";

export interface DesignerConfig {
    workflowTypeId: number;
    mode?: DesignerMode;
    readOnly: boolean;
    urls: DesignerUrls;
    // The pages of the workflow type: the designer, a version (with a versionId query string parameter), and the
    // comparison of two definitions (from and to).
    designerUrl?: string | null;
    versionPageUrl?: string | null;
    comparePageUrl?: string | null;
    instancesUrl: string;
    exportUrl: string;
    listUrl: string;
    /**
     * The signed-in user's id, to tell whether someone else last edited the draft.
     */
    currentUserId?: string | null;
    /**
     * The activity to select and open when the designer loads (the `activityId` query string parameter,
     * which the old activity edit URLs redirect with).
     */
    initialActivityId?: string | null;
    translations: Record<string, string>;
}

export const readConfig = (element: HTMLElement): DesignerConfig => {
    const raw = element.dataset.config;

    if (!raw) {
        throw new Error("The workflow designer element has no data-config attribute.");
    }

    return JSON.parse(raw) as DesignerConfig;
};
