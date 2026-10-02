// The configuration rendered by Views/WorkflowType/Designer.cshtml into the data-config attribute of
// #workflow-designer. Every URL is generated on the server, so it includes the tenant prefix.

export interface DesignerUrls {
    definition: string;
    library: string;
    save: string;
    addActivity: string;
    editor: string;
    settings: string;
    publish: string;
    discard: string;
}

export interface DesignerConfig {
    workflowTypeId: number;
    readOnly: boolean;
    urls: DesignerUrls;
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
