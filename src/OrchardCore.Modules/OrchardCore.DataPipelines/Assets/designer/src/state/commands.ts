import { connectCommand as connectGraphCommand } from "@bloom/designer/graph/commands";
import type { Graph as GraphOf } from "@bloom/designer/graph/model";
import type { DesignerConnection, DesignerNode } from "../api/types";
import { pipelineModel } from "./ports";

export { addNodeCommand, disconnectCommand, moveNodesCommand, removeNodesCommand, type NodeMove } from "@bloom/designer/graph/commands";

/**
 * The part of the designer state the commands change. The arrays are the store's reactive arrays; the connections are
 * the edges of the graph.
 */
export interface Graph extends GraphOf<DesignerNode, DesignerConnection> {
    readonly connections: DesignerConnection[];
}

/**
 * Adds a connection. An input that accepts one connection (not `allowsMany`) keeps the new one only: the existing one
 * is replaced, and reverting restores it.
 */
export const connectCommand = (graph: Graph, connection: DesignerConnection) => connectGraphCommand(graph, pipelineModel.ends(connection));
