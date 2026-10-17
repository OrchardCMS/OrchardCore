import { reactive } from "vue";
import { endsKey, type EdgeEnds, type Graph, type GraphModel, type GraphNode, type GraphPort } from "../model";

// A pipeline: steps with typed ports, from a port to a port.
export interface Step extends GraphNode {
    inputs: GraphPort[];
    outputs: GraphPort[];
}

export interface Connection {
    sourceStepId: string;
    sourcePort: string;
    targetStepId: string;
    targetPort: string;
}

const records = (name: string, allowsMany = false): GraphPort => ({ name, kind: "Records", allowsMany });
const files = (name: string, allowsMany = false): GraphPort => ({ name, kind: "Files", allowsMany });

const fanOut = (port: GraphPort): GraphPort => ({ ...port, allowsMany: true });

export const pipelineModel: GraphModel<Step, Connection> = {
    ends: (connection) => ({ sourceId: connection.sourceStepId, sourcePort: connection.sourcePort, targetId: connection.targetStepId, targetPort: connection.targetPort }),
    create: (ends) => ({ sourceStepId: ends.sourceId, sourcePort: ends.sourcePort, targetStepId: ends.targetId, targetPort: ends.targetPort }),
    key: (connection) => endsKey(pipelineModel.ends(connection)),
    inputs: (step) => step.inputs,
    // An output leads to any number of steps.
    outputs: (step) => step.outputs.map(fanOut),
};

export const step = (id: string, inputs: GraphPort[], outputs: GraphPort[], x = 0, y = 0): Step => ({ id, x, y, inputs, outputs });

export const createPipeline = (): Graph<Step, Connection> => {
    const state = reactive({
        nodes: [
            step("source", [], [records("Output")]),
            step("filter", [records("Input")], [records("Matched"), records("Unmatched")], 300),
            step("join", [records("Left"), records("Right")], [records("Output")], 600),
            step("table", [records("Input", true)], [], 900),
            step("csv", [records("Input")], [files("File")], 900, 200),
            step("email", [files("Attachments", true)], [], 1200, 200),
        ],
        edges: [
            { sourceStepId: "source", sourcePort: "Output", targetStepId: "filter", targetPort: "Input" },
            { sourceStepId: "filter", sourcePort: "Matched", targetStepId: "join", targetPort: "Left" },
        ] as Connection[],
    });

    return {
        get nodes() {
            return state.nodes;
        },
        get edges() {
            return state.edges;
        },
        model: pipelineModel,
    };
};

export const ends = (sourceId: string, sourcePort: string, targetId: string, targetPort: string): EdgeEnds => ({ sourceId, sourcePort, targetId, targetPort });

// A workflow: activities with outcomes, from an outcome to an activity, which has a single implicit input.
export interface Activity extends GraphNode {
    isStart: boolean;
    outcomes: string[];
}

export interface Transition {
    sourceActivityId: string;
    sourceOutcomeName: string;
    destinationActivityId: string;
}

export const workflowModel = (followAll: () => boolean): GraphModel<Activity, Transition> => ({
    ends: (transition) => ({ sourceId: transition.sourceActivityId, sourcePort: transition.sourceOutcomeName, targetId: transition.destinationActivityId, targetPort: "" }),
    create: (ends) => ({ sourceActivityId: ends.sourceId, sourceOutcomeName: ends.sourcePort, destinationActivityId: ends.targetId }),
    key: (transition) => `${transition.sourceActivityId}:${transition.sourceOutcomeName}:${transition.destinationActivityId}`,
    inputs: () => [{ name: "", allowsMany: true }],
    outputs: (activity) => activity.outcomes.map((name) => ({ name, allowsMany: followAll() })),
});

export const createWorkflow = (followAll = false): Graph<Activity, Transition> => ({
    nodes: [
        { id: "start", x: 0, y: 0, isStart: true, outcomes: ["Done"] },
        { id: "check", x: 300, y: 0, isStart: false, outcomes: ["True", "False"] },
        { id: "notify", x: 600, y: 0, isStart: false, outcomes: ["Done"] },
    ],
    edges: [
        { sourceActivityId: "start", sourceOutcomeName: "Done", destinationActivityId: "check" },
        { sourceActivityId: "check", sourceOutcomeName: "True", destinationActivityId: "notify" },
    ],
    model: workflowModel(() => followAll),
});
