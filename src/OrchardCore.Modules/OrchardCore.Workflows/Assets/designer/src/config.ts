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
    translations: Record<string, string>;
}

export const readConfig = (element: HTMLElement): DesignerConfig => {
    const raw = element.dataset.config;

    if (!raw) {
        throw new Error("The workflow designer element has no data-config attribute.");
    }

    return JSON.parse(raw) as DesignerConfig;
};
