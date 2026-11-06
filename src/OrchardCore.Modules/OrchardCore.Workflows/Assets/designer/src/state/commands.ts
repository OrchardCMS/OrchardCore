import type { Command } from "@bloom/designer/core/history";
import {
    addNodeCommand as addGraphNodeCommand,
    connectCommand as connectGraphCommand,
    disconnectCommand,
    moveNodesCommand,
    removeNodesCommand,
    setNodePropertyCommand,
    type NodeMove,
} from "@bloom/designer/graph/commands";
import type { EdgeEnds, Graph as GraphOf, GraphModel } from "@bloom/designer/graph/model";
import { rejectConnection } from "@bloom/designer/graph/rules";
import type { DesignerNode, DesignerTransition } from "../api/types";

export { disconnectCommand, moveNodesCommand, removeNodesCommand, type NodeMove };

/**
 * The part of the designer state the commands change. The arrays are the store's reactive arrays; the transitions are
 * the edges of the graph.
 */
export interface Graph extends GraphOf<DesignerNode, DesignerTransition> {
    readonly transitions: DesignerTransition[];
}

/**
 * The key of a transition, the same format as WorkflowDesignIssue.GetTransitionKey on the server.
 */
export const transitionKey = (transition: DesignerTransition) =>
    `${transition.sourceActivityId}:${transition.sourceOutcomeName}:${transition.destinationActivityId}`;

// An activity has one input, which any number of transitions reach, unnamed.
const INPUT = "";

const transitionEnds = (transition: DesignerTransition): EdgeEnds => ({
    sourceId: transition.sourceActivityId,
    sourcePort: transition.sourceOutcomeName,
    targetId: transition.destinationActivityId,
    targetPort: INPUT,
});

/**
 * A workflow as a graph: the outcomes of an activity are its outputs, and its transitions go from an outcome to the
 * input of an activity. Unless the workflow follows every transition of an outcome (`followsEveryTransition`, its
 * branching mode), an outcome leads to one activity, since the engine only follows its first transition.
 */
export const createWorkflowModel = (followsEveryTransition: () => boolean): GraphModel<DesignerNode, DesignerTransition> => ({
    ends: transitionEnds,
    create: (ends) => ({ sourceActivityId: ends.sourceId, sourceOutcomeName: ends.sourcePort, destinationActivityId: ends.targetId }),
    key: transitionKey,
    inputs: () => [{ name: INPUT, allowsMany: true }],
    outputs: (node) => node.outcomes.map((outcome) => ({ name: outcome.name, allowsMany: followsEveryTransition() })),
});

/**
 * Returns whether a transition may be drawn: both activities exist, the outcome exists on the source, and
 * it isn't a self-loop (the engine never follows those). A transition may lead back to an earlier activity.
 */
export const canConnect = (graph: Graph, transition: DesignerTransition) => rejectConnection(graph, transitionEnds(transition), { allowCycles: true }) === null;

export const addNodeCommand = (graph: Graph, node: DesignerNode): Command => addGraphNodeCommand(graph, node);

/**
 * Adds a transition. An existing transition of the same outcome is replaced, unless the workflow follows every
 * transition of an outcome; reverting restores it.
 */
export const connectCommand = (graph: Graph, transition: DesignerTransition): Command & { replaced: () => DesignerTransition | undefined } => {
    const command = connectGraphCommand(graph, transitionEnds(transition));

    return {
        label: command.label,
        apply: () => command.apply(),
        revert: () => command.revert(),
        replaced: () => command.replaced()[0],
    };
};

export const setStartCommand = (graph: Graph, id: string, isStart: boolean): Command => setNodePropertyCommand(graph, id, "isStart", isStart);
