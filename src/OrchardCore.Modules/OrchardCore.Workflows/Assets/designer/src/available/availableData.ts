import type { DesignerNode, DesignerTransition, GlobalValue, ProvidedValue, ProvidedValueMember, VariableDefinition } from "../api/types";

// The data an activity can use: the workflow's variables, the values the activities on a path to it provide
// (ProvidedValue, declared by the activities), and a few values of the workflow itself. Each one comes with the
// JavaScript and Liquid expressions that read it.

export type AvailableSource = "Variable" | "Input" | "Output" | "Properties" | "LastResult" | "CorrelationId" | "Global" | "Function";

export interface AvailableValue {
    // Unique in a group.
    key: string;
    name: string;
    source: AvailableSource;
    typeName: string;
    description?: string | null;
    // Null when scripts can't read the value, like the Liquid-only global values.
    javaScript: string | null;
    // Null when Liquid can't read it, like the functions scripts call.
    liquid: string | null;
    // The fields of the value, with their own expressions.
    members?: AvailableValue[];
}

export interface AvailableActivityGroup {
    activityId: string;
    title: string;
    values: AvailableValue[];
}

export interface AvailableData {
    variables: AvailableValue[];
    // The values the activity sets before it evaluates its own expressions.
    self: AvailableValue[];
    activities: AvailableActivityGroup[];
    workflow: AvailableValue[];
    // The inputs the workflow starts with: those of its variables marked as inputs.
    inputs: AvailableValue[];
    // The global Liquid values, such as Site, and the functions scripts call.
    global: AvailableValue[];
    functions: AvailableValue[];
}

const IDENTIFIER = /^[A-Za-z_][A-Za-z0-9_]*$/;

// A field can be a path, such as the Identity.Name of the user.
const MEMBER_PATH = /^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$/;

// A JavaScript string literal, as the editors' completions write it.
const quoted = (name: string) => JSON.stringify(name);

// Workflow.Input.name, or Workflow.Input["first name"] for a name that isn't an identifier.
const liquidMember = (collection: string, name: string) =>
    IDENTIFIER.test(name) ? `{{ Workflow.${collection}.${name} }}` : `{{ Workflow.${collection}[${JSON.stringify(name)}] }}`;

/**
 * The JavaScript and Liquid expressions that read a value.
 */
// The global values and functions come with their own expressions.
export const expressionsOf = (source: Exclude<AvailableSource, "Global" | "Function">, name: string): { javaScript: string; liquid: string } => {
    switch (source) {
        case "Variable":
            return { javaScript: `variable(${quoted(name)})`, liquid: liquidMember("Variables", name) };
        case "Input":
            return { javaScript: `input(${quoted(name)})`, liquid: liquidMember("Input", name) };
        case "Properties":
            return { javaScript: `property(${quoted(name)})`, liquid: liquidMember("Properties", name) };
        case "Output":
            // output(name, value) only writes, so scripts read the output through the workflow.
            return { javaScript: `workflow().Output[${quoted(name)}]`, liquid: liquidMember("Output", name) };
        case "LastResult":
            return { javaScript: "lastResult()", liquid: "{{ Workflow.LastResult }}" };
        case "CorrelationId":
            return { javaScript: "correlationId()", liquid: "{{ Workflow.CorrelationId }}" };
    }
};

/**
 * The expressions that read a field of a value: input("ContentEvent").ContentType and
 * {{ Workflow.Input.ContentEvent.ContentType }}.
 */
export const memberExpressionsOf = (value: { javaScript: string | null; liquid: string | null }, name: string) => {
    const access = MEMBER_PATH.test(name) ? `.${name}` : `[${JSON.stringify(name)}]`;
    const path = value.liquid?.replace(/^\{\{\s*/, "").replace(/\s*\}\}$/, "");

    return { javaScript: value.javaScript === null ? null : `${value.javaScript}${access}`, liquid: path === undefined ? null : `{{ ${path}${access} }}` };
};

const membersOf = (parent: AvailableValue, members: ProvidedValueMember[] | undefined): AvailableValue[] =>
    (members ?? []).map((member) => ({
        key: `${parent.key}.${member.name}`,
        name: member.name,
        source: parent.source,
        typeName: member.typeName,
        description: member.description,
        ...memberExpressionsOf(parent, member.name),
    }));

/**
 * The activities that can run before an activity: those with a path to it, nearest first.
 */
export const upstreamNodeIds = (transitions: DesignerTransition[], nodeId: string): string[] => {
    const seen = new Set([nodeId]);
    const order: string[] = [];
    let frontier = [nodeId];

    while (frontier.length > 0) {
        const next: string[] = [];

        for (const id of frontier) {
            for (const transition of transitions) {
                if (transition.destinationActivityId === id && !seen.has(transition.sourceActivityId)) {
                    seen.add(transition.sourceActivityId);
                    order.push(transition.sourceActivityId);
                    next.push(transition.sourceActivityId);
                }
            }
        }

        frontier = next;
    }

    return order;
};

export interface AvailableLabels {
    lastResult: string;
    lastResultDescription: string;
    // "Retrieve Owner: The content item the activity retrieved."
    lastResultFrom: (activity: string, description: string) => string;
    correlationId: string;
    correlationIdDescription: string;
    // What an input is, for those whose variable has no description.
    inputDescription: string;
}

const sameFields = (members: ProvidedValueMember[][]) => new Set(members.map((list) => list.map((member) => member.name).join("\n"))).size === 1;

/**
 * The last result an activity reads: what the activity that ran just before set. When every activity with a
 * transition to it declares its last result, it has their type, and their fields when they all have the same.
 */
const lastResultOf = (byId: Map<string, DesignerNode>, transitions: DesignerTransition[], nodeId: string, labels: AvailableLabels): AvailableValue => {
    const lastResult: AvailableValue = {
        key: "LastResult",
        name: labels.lastResult,
        source: "LastResult",
        typeName: "any",
        description: labels.lastResultDescription,
        ...expressionsOf("LastResult", ""),
    };

    const sources = [...new Set(transitions.filter((transition) => transition.destinationActivityId === nodeId).map((transition) => transition.sourceActivityId))]
        .map((id) => byId.get(id))
        .filter((node): node is DesignerNode => !!node);
    const declared = sources.map((node) => ({ node, value: node.providedValues?.find((value) => value.source === "LastResult") }));

    if (declared.length === 0 || declared.some((item) => !item.value)) {
        return lastResult;
    }

    const values = declared.map((item) => item.value!);
    const sameType = new Set(values.map((value) => value.typeName)).size === 1;
    const value: AvailableValue = {
        ...lastResult,
        typeName: sameType ? values[0].typeName : "any",
        description: [...new Set(declared.map((item) => labels.lastResultFrom(item.node.title, item.value!.description ?? "")))].join(" "),
    };

    return { ...value, members: sameType && sameFields(values.map((item) => item.members ?? [])) ? membersOf(value, values[0].members) : [] };
};

/**
 * The data available to an activity.
 */
export const availableData = (
    nodes: DesignerNode[],
    transitions: DesignerTransition[],
    variables: VariableDefinition[],
    nodeId: string,
    labels: AvailableLabels,
    globals: GlobalValue[] = [],
): AvailableData => {
    const byId = new Map(nodes.map((node) => [node.id, node]));
    const variableNames = new Set(variables.map((variable) => variable.name.toLowerCase()));

    // The last result is listed with the workflow's values, and a variable, which is also a property, with the
    // variables.
    const valuesOf = (values: ProvidedValue[]) =>
        values
            .filter((value) => value.source !== "LastResult" && !(value.source === "Properties" && variableNames.has(value.name.toLowerCase())))
            .map((value) => {
                const available: AvailableValue = {
                    key: `${value.source}:${value.name}`,
                    name: value.name,
                    source: value.source,
                    typeName: value.typeName,
                    description: value.description,
                    ...expressionsOf(value.source, value.name),
                };

                return { ...available, members: membersOf(available, value.members) };
            });

    const activities = upstreamNodeIds(transitions, nodeId)
        .map((id) => byId.get(id))
        .filter((node): node is DesignerNode => !!node)
        .map((node) => ({ activityId: node.id, title: node.title, values: valuesOf(node.providedValues ?? []) }))
        .filter((group) => group.values.length > 0);

    const globalsOf = (kind: GlobalValue["kind"]) =>
        globals
            .filter((value) => value.kind === kind)
            .map((value) => {
                const available: AvailableValue = {
                    key: `${kind}:${value.name}`,
                    name: value.name,
                    source: kind === "Value" ? "Global" : "Function",
                    typeName: value.typeName,
                    description: value.description,
                    javaScript: value.javaScript ?? null,
                    liquid: value.liquidPath ? `{{ ${value.liquidPath} }}` : null,
                };

                return { ...available, members: membersOf(available, value.members) };
            });

    return {
        variables: variables.map((variable) => ({
            key: `Variable:${variable.name}`,
            name: variable.name,
            source: "Variable",
            typeName: variable.typeName,
            description: variable.description,
            ...expressionsOf("Variable", variable.name),
        })),
        self: valuesOf((byId.get(nodeId)?.providedValues ?? []).filter((value) => value.availableToItself)),
        activities,
        workflow: [
            lastResultOf(byId, transitions, nodeId, labels),
            {
                key: "CorrelationId",
                name: labels.correlationId,
                source: "CorrelationId",
                typeName: "string",
                description: labels.correlationIdDescription,
                ...expressionsOf("CorrelationId", ""),
            },
        ],
        inputs: variables
            .filter((variable) => variable.isInput)
            .map((variable) => ({
                key: `Input:${variable.name}`,
                name: variable.name,
                source: "Input",
                typeName: variable.typeName,
                description: variable.description || labels.inputDescription,
                ...expressionsOf("Input", variable.name),
            })),
        global: globalsOf("Value"),
        functions: globalsOf("Function"),
    };
};

const byIdTitle = (nodes: DesignerNode[], nodeId: string) => nodes.find((node) => node.id === nodeId)?.title ?? "";

/**
 * The values the activity itself and the activities on a path to it provide, without repeats, for the editors'
 * completions.
 */
export const upstreamValues = (nodes: DesignerNode[], transitions: DesignerTransition[], variables: VariableDefinition[], nodeId: string | null | undefined) => {
    if (!nodeId) {
        return [];
    }

    const data = availableData(nodes, transitions, variables, nodeId, {
        lastResult: "",
        lastResultDescription: "",
        lastResultFrom: () => "",
        correlationId: "",
        correlationIdDescription: "",
        inputDescription: "",
    });
    const seen = new Set<string>();

    const self = byIdTitle(nodes, nodeId);

    return [{ title: self, values: data.self }, ...data.activities]
        .flatMap((group) => group.values.flatMap((value) => [value, ...(value.members ?? [])].map((item) => ({ ...item, activityTitle: group.title }))))
        .filter((value) => (seen.has(value.key) ? false : (seen.add(value.key), true)));
};
