import type { DesignerConnection, DesignerNode, DesignerPort, LibraryStep, PortKind } from "../api/types";

// The rules of the connections between steps: from an output port to an input port of the same kind, never in a loop.

/**
 * The key of a connection: "source:port->target:port".
 */
export const connectionKey = (connection: DesignerConnection) =>
    `${connection.sourceStepId}:${connection.sourcePort}->${connection.targetStepId}:${connection.targetPort}`;

export const sameConnection = (a: DesignerConnection, b: DesignerConnection) => connectionKey(a) === connectionKey(b);

export const findOutput = (node: Pick<DesignerNode, "outputs"> | undefined, port: string) => node?.outputs.find((item) => item.name === port);

export const findInput = (node: Pick<DesignerNode, "inputs"> | undefined, port: string) => node?.inputs.find((item) => item.name === port);

/**
 * Why a connection can't be drawn, or null when it can:
 * - `missing`: a step or a port doesn't exist;
 * - `self`: from a step to itself;
 * - `kind`: the ports carry different kinds (records and files);
 * - `cycle`: the target already flows into the source, so the pipeline would loop.
 */
export type ConnectRejection = "missing" | "self" | "kind" | "cycle";

/**
 * Whether `targetId` flows into `sourceId` through the connections, so connecting source to target would make a loop.
 */
export const createsCycle = (connections: DesignerConnection[], sourceId: string, targetId: string) => {
    const next = new Map<string, string[]>();

    for (const connection of connections) {
        next.set(connection.sourceStepId, [...(next.get(connection.sourceStepId) ?? []), connection.targetStepId]);
    }

    const visited = new Set<string>();
    const stack = [targetId];

    while (stack.length > 0) {
        const id = stack.pop()!;

        if (id === sourceId) {
            return true;
        }

        if (!visited.has(id)) {
            visited.add(id);
            stack.push(...(next.get(id) ?? []));
        }
    }

    return false;
};

export const rejectConnection = (nodes: DesignerNode[], connections: DesignerConnection[], connection: DesignerConnection): ConnectRejection | null => {
    if (connection.sourceStepId === connection.targetStepId) {
        return "self";
    }

    const output = findOutput(
        nodes.find((node) => node.id === connection.sourceStepId),
        connection.sourcePort,
    );
    const input = findInput(
        nodes.find((node) => node.id === connection.targetStepId),
        connection.targetPort,
    );

    if (!output || !input) {
        return "missing";
    }

    if (output.kind !== input.kind) {
        return "kind";
    }

    return createsCycle(connections, connection.sourceStepId, connection.targetStepId) ? "cycle" : null;
};

/**
 * The connections already going into an input that accepts only one, which a new connection to it replaces.
 */
export const replacedBy = (nodes: DesignerNode[], connections: DesignerConnection[], connection: DesignerConnection) => {
    const input = findInput(
        nodes.find((node) => node.id === connection.targetStepId),
        connection.targetPort,
    );

    if (!input || input.allowsMany) {
        return [];
    }

    return connections.filter(
        (existing) => existing.targetStepId === connection.targetStepId && existing.targetPort === connection.targetPort && !sameConnection(existing, connection),
    );
};

/**
 * The first input of `node` that can receive `kind`: a free one first, then one whose connection would be replaced.
 * Used when a connection is dropped on a step rather than on one of its ports.
 */
export const firstCompatibleInput = (node: DesignerNode, kind: PortKind, connections: DesignerConnection[]): DesignerPort | undefined => {
    const inputs = node.inputs.filter((input) => input.kind === kind);
    const isTaken = (input: DesignerPort) =>
        !input.allowsMany && connections.some((connection) => connection.targetStepId === node.id && connection.targetPort === input.name);

    return inputs.find((input) => !isTaken(input)) ?? inputs[0];
};

/**
 * Whether a step type can come after an output of `kind`: its first input receives that kind (AddStepPayload.connectFrom
 * connects the new step's first compatible input).
 */
export const acceptsKind = (step: Pick<LibraryStep, "inputs">, kind: PortKind) => step.inputs.length > 0 && step.inputs[0].kind === kind;

/**
 * The connections that use a port the step no longer has, after its settings changed its ports.
 */
export const danglingConnections = (node: DesignerNode, connections: DesignerConnection[]) =>
    connections.filter(
        (connection) =>
            (connection.sourceStepId === node.id && !findOutput(node, connection.sourcePort)) ||
            (connection.targetStepId === node.id && !findInput(node, connection.targetPort)),
    );
