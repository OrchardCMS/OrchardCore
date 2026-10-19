import { endsKey, type EdgeEnds, type Graph, type GraphModel } from "@bloom/designer/graph/model";
import {
    createsCycle as graphCreatesCycle,
    danglingEdges,
    firstCompatibleInput as graphFirstCompatibleInput,
    rejectConnection as graphRejectConnection,
    replacedBy as graphReplacedBy,
    type ConnectRejection,
} from "@bloom/designer/graph/rules";
import type { DesignerConnection, DesignerNode, DesignerPort, LibraryStep, PortKind } from "../api/types";

// The rules of the connections between steps (see @bloom/designer/graph/rules): from an output port to an input port of
// the same kind, never in a loop. An output leads to any number of steps; an input accepts one connection, unless it
// `allowsMany`.

export type { ConnectRejection };

const connectionEnds = (connection: DesignerConnection): EdgeEnds => ({
    sourceId: connection.sourceStepId,
    sourcePort: connection.sourcePort,
    targetId: connection.targetStepId,
    targetPort: connection.targetPort,
});

/**
 * A pipeline as a graph: its steps and their connections.
 */
export const pipelineModel: GraphModel<DesignerNode, DesignerConnection> = {
    ends: connectionEnds,
    create: (ends) => ({ sourceStepId: ends.sourceId, sourcePort: ends.sourcePort, targetStepId: ends.targetId, targetPort: ends.targetPort }),
    key: (connection) => endsKey(connectionEnds(connection)),
    inputs: (node) => node.inputs,
    outputs: (node) => node.outputs.map((port) => ({ ...port, allowsMany: true })),
};

const graphOf = (nodes: DesignerNode[], connections: DesignerConnection[]): Graph<DesignerNode, DesignerConnection> => ({
    nodes,
    edges: connections,
    model: pipelineModel,
});

/**
 * The key of a connection: "source:port->target:port".
 */
export const connectionKey = pipelineModel.key;

export const sameConnection = (a: DesignerConnection, b: DesignerConnection) => connectionKey(a) === connectionKey(b);

export const findOutput = (node: Pick<DesignerNode, "outputs"> | undefined, port: string) => node?.outputs.find((item) => item.name === port);

export const findInput = (node: Pick<DesignerNode, "inputs"> | undefined, port: string) => node?.inputs.find((item) => item.name === port);

/**
 * Whether `targetId` flows into `sourceId` through the connections, so connecting source to target would make a loop.
 */
export const createsCycle = (connections: DesignerConnection[], sourceId: string, targetId: string) => graphCreatesCycle(graphOf([], connections), sourceId, targetId);

export const rejectConnection = (nodes: DesignerNode[], connections: DesignerConnection[], connection: DesignerConnection): ConnectRejection | null =>
    graphRejectConnection(graphOf(nodes, connections), connectionEnds(connection));

/**
 * The connections already going into an input that accepts only one, which a new connection to it replaces.
 */
export const replacedBy = (nodes: DesignerNode[], connections: DesignerConnection[], connection: DesignerConnection) =>
    graphReplacedBy(graphOf(nodes, connections), connectionEnds(connection));

/**
 * The first input of `node` that can receive `kind`: a free one first, then one whose connection would be replaced.
 * Used when a connection is dropped on a step rather than on one of its ports.
 */
export const firstCompatibleInput = (node: DesignerNode, kind: PortKind, connections: DesignerConnection[]) =>
    graphFirstCompatibleInput(graphOf([node], connections), node, kind) as DesignerPort | undefined;

/**
 * Whether a step type can come after an output of `kind`: its first input receives that kind (AddStepPayload.connectFrom
 * connects the new step's first compatible input).
 */
export const acceptsKind = (step: Pick<LibraryStep, "inputs">, kind: PortKind) => step.inputs.length > 0 && step.inputs[0].kind === kind;

/**
 * The connections that use a port the step no longer has, after its settings changed its ports.
 */
export const danglingConnections = (node: DesignerNode, connections: DesignerConnection[]) => danglingEdges(graphOf([node], connections), node);
