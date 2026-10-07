import type { DesignerNode, DesignerTransition, ProvidedValueMember, VariableDefinition } from "../api/types";

// The data an activity can use: the workflow's variables, the values the activities on a path to it provide
// (ProvidedValue, declared by the activities), and a few values of the workflow itself. Each one comes with the
// JavaScript and Liquid expressions that read it.

export type AvailableSource = "Variable" | "Input" | "Output" | "Properties" | "LastResult" | "CorrelationId";

export interface AvailableValue {
    // Unique in a group.
    key: string;
    name: string;
    source: AvailableSource;
    typeName: string;
    description?: string | null;
    javaScript: string;
    liquid: string;
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
    activities: AvailableActivityGroup[];
    workflow: AvailableValue[];
}

const IDENTIFIER = /^[A-Za-z_][A-Za-z0-9_]*$/;

// A JavaScript string literal, as the editors' completions write it.
const quoted = (name: string) => JSON.stringify(name);

// Workflow.Input.name, or Workflow.Input["first name"] for a name that isn't an identifier.
const liquidMember = (collection: string, name: string) =>
    IDENTIFIER.test(name) ? `{{ Workflow.${collection}.${name} }}` : `{{ Workflow.${collection}[${JSON.stringify(name)}] }}`;

/**
 * The JavaScript and Liquid expressions that read a value.
 */
export const expressionsOf = (source: AvailableSource, name: string): { javaScript: string; liquid: string } => {
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
export const memberExpressionsOf = (value: { javaScript: string; liquid: string }, name: string) => {
    const access = IDENTIFIER.test(name) ? `.${name}` : `[${JSON.stringify(name)}]`;
    const path = value.liquid.replace(/^\{\{\s*/, "").replace(/\s*\}\}$/, "");

    return { javaScript: `${value.javaScript}${access}`, liquid: `{{ ${path}${access} }}` };
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
): AvailableData => {
    const byId = new Map(nodes.map((node) => [node.id, node]));
    const variableNames = new Set(variables.map((variable) => variable.name.toLowerCase()));

    const activities = upstreamNodeIds(transitions, nodeId)
        .map((id) => byId.get(id))
        .filter((node): node is DesignerNode => !!node)
        .map((node) => ({
            activityId: node.id,
            title: node.title,
            values: (node.providedValues ?? [])
                // The last result is listed with the workflow's values, and a variable, which is also a property,
                // with the variables.
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
                }),
        }))
        .filter((group) => group.values.length > 0);

    return {
        variables: variables.map((variable) => ({
            key: `Variable:${variable.name}`,
            name: variable.name,
            source: "Variable",
            typeName: variable.typeName,
            description: variable.description,
            ...expressionsOf("Variable", variable.name),
        })),
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
    };
};

/**
 * The values the activities on a path to an activity provide, without repeats, for the editors' completions.
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
    });
    const seen = new Set<string>();

    return data.activities
        .flatMap((group) => group.values.flatMap((value) => [value, ...(value.members ?? [])].map((item) => ({ ...item, activityTitle: group.title }))))
        .filter((value) => (seen.has(value.key) ? false : (seen.add(value.key), true)));
};
