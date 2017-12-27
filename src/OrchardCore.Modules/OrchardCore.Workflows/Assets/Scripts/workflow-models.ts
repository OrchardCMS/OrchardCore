namespace Workflows {

    export interface Workflow {
        id: number;
        activities: Array<Activity>;
        transitions: Array<Transition>;
    }

    export interface Activity {
        id: number;
        x: number;
        y: number;
        outcomes: Array<Outcome>;
    }

    export interface Outcome {
        name: string;
        displayName: string;
    }

    export interface Transition {
        sourceActivityId: number;
        destinationActivityId: number;
        sourceOutcomeName: string;
    }
}