import type { DesignerDefinition, DesignerNode, DesignerPort, DesignerRun, Library, LibraryStep, StepCategory } from "../api/types";

export const records = (name: string, overrides: Partial<DesignerPort> = {}): DesignerPort => ({
    name,
    displayName: name,
    kind: "Records",
    isRequired: true,
    allowsMany: false,
    ...overrides,
});

export const files = (name: string, overrides: Partial<DesignerPort> = {}): DesignerPort => ({ ...records(name, overrides), kind: "Files", ...overrides });

export const createNode = (id: string, overrides: Partial<DesignerNode> = {}): DesignerNode => ({
    id,
    type: "FilterStep",
    title: null,
    displayText: `Step ${id}`,
    category: "Transform",
    icon: null,
    isMissing: false,
    hasEditor: true,
    x: 0,
    y: 0,
    inputs: [records("Input")],
    outputs: [records("Output")],
    designHtml: `<p>Summary of ${id}</p>`,
    ...overrides,
});

/**
 * A pipeline: a source, a filter with two outputs, a join with two inputs, a CSV file step, an email destination that
 * takes files, and a table destination that takes many inputs. Connected: source → filter, filter.Matched → join.Left.
 */
export const createDefinition = (): DesignerDefinition => ({
    pipelineId: "pipeline-1",
    revision: 0,
    hasDraft: false,
    settings: { name: "Monthly orders", description: "Orders of the month" },
    nodes: [
        createNode("source", { type: "ContentItemsSource", displayText: "Content items", category: "Source", inputs: [], outputs: [records("Output", { displayName: "Rows" })] }),
        createNode("filter", { x: 300, outputs: [records("Matched"), records("Unmatched")] }),
        createNode("join", { type: "JoinStep", x: 600, inputs: [records("Left"), records("Right")] }),
        createNode("csv", { type: "CsvFile", category: "File", x: 900, inputs: [records("Input")], outputs: [files("File")] }),
        createNode("email", { type: "EmailDestination", category: "Destination", x: 1200, inputs: [files("Attachments", { allowsMany: true })], outputs: [] }),
        createNode("table", { type: "TableDestination", category: "Destination", x: 1200, y: 300, inputs: [records("Input", { allowsMany: true })], outputs: [] }),
    ],
    connections: [
        { sourceStepId: "source", sourcePort: "Output", targetStepId: "filter", targetPort: "Input" },
        { sourceStepId: "filter", sourcePort: "Matched", targetStepId: "join", targetPort: "Left" },
    ],
    issues: [],
    publishedVersion: null,
    version: null,
    canRun: true,
    lastRun: null,
});

const libraryStep = (name: string, displayText: string, category: StepCategory, overrides: Partial<LibraryStep> = {}): LibraryStep => ({
    name,
    displayText,
    description: `${displayText} description`,
    category,
    icon: null,
    inputs: [records("Input")],
    outputs: [records("Output")],
    ...overrides,
});

// The categories are out of order on purpose: the toolbox sorts them.
export const library: Library = {
    categories: [
        {
            category: "Destination",
            displayName: "Destinations",
            steps: [
                libraryStep("EmailDestination", "Send by email", "Destination", { inputs: [files("Attachments", { allowsMany: true })], outputs: [] }),
                libraryStep("TableDestination", "Write to a table", "Destination", { outputs: [] }),
            ],
        },
        {
            category: "Source",
            displayName: "Sources",
            steps: [
                libraryStep("ContentItemsSource", "Content items", "Source", { inputs: [] }),
                libraryStep("UsersSource", "Utilisateurs", "Source", { inputs: [], description: "Les comptes" }),
            ],
        },
        {
            category: "Transform",
            displayName: "Transforms",
            steps: [
                libraryStep("FilterStep", "Filter", "Transform", { outputs: [records("Matched"), records("Unmatched")] }),
                libraryStep("JoinStep", "Join", "Transform", { inputs: [records("Left"), records("Right")] }),
            ],
        },
        {
            category: "File",
            displayName: "Files",
            steps: [
                libraryStep("CsvFile", "Write a CSV file", "File", { outputs: [files("File")] }),
                libraryStep("ZipFiles", "Zip the files", "File", { inputs: [files("Files", { allowsMany: true })], outputs: [files("Archive")] }),
            ],
        },
    ],
};

export const createRun = (overrides: Partial<DesignerRun> = {}): DesignerRun => ({
    runId: "run-1",
    status: "Running",
    versionNumber: 3,
    trigger: "Manual",
    triggeredBy: "admin",
    queuedUtc: "2026-10-01T10:00:00Z",
    startedUtc: "2026-10-01T10:00:01Z",
    completedUtc: null,
    error: null,
    failedStepId: null,
    steps: [
        { stepId: "source", title: "Content items", status: "Succeeded", rowsIn: 0, rowsOut: 1234, filesIn: 0, filesOut: 0, warnings: 0 },
        { stepId: "filter", title: "Filter", status: "Running", rowsIn: 1234, rowsOut: 1, filesIn: 0, filesOut: 0, warnings: 2 },
        { stepId: "csv", title: "CSV", status: "Pending", rowsIn: 0, rowsOut: 0, filesIn: 0, filesOut: 0, warnings: 0 },
    ],
    deliveries: [],
    log: [],
    url: "/admin/runs/run-1",
    canCancel: true,
    ...overrides,
});
