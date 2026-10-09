import type { DesignerDefinition, DesignerNode } from "../../api/types";

export const createNode = (id: string, overrides: Partial<DesignerNode> = {}, outcomes = ["Done"]): DesignerNode => ({
    id,
    name: "NotifyTask",
    x: 0,
    y: 0,
    isStart: false,
    isEvent: false,
    hasEditor: true,
    isMissing: false,
    title: `Title ${id}`,
    displayText: "Notify Task",
    category: "Primitives",
    designHtml: `<header><h4>Title ${id}</h4></header><em>"Hello"</em>`,
    outcomes: outcomes.map((name) => ({ name, displayName: `${name} label` })),
    ...overrides,
});

export const createDefinition = (): DesignerDefinition => ({
    id: 1,
    workflowTypeId: "type-1",
    revision: 0,
    hasDraft: false,
    settings: { name: "Test", isEnabled: true, isSingleton: false, lockTimeout: 0, lockExpiration: 0, deleteFinishedWorkflows: false },
    nodes: [
        createNode("start", { isEvent: true, isStart: true, name: "HttpRequestEvent", displayText: "Http Request" }),
        createNode("fork", { x: 300, name: "ForkTask", displayText: "Fork" }, ["A", "B"]),
        createNode("a", { x: 600 }),
        createNode("b", { x: 600, y: 200 }),
    ],
    transitions: [
        { sourceActivityId: "start", sourceOutcomeName: "Done", destinationActivityId: "fork" },
        { sourceActivityId: "fork", sourceOutcomeName: "A", destinationActivityId: "a" },
        { sourceActivityId: "fork", sourceOutcomeName: "B", destinationActivityId: "b" },
    ],
    issues: [],
    runningInstanceCount: 0,
});
