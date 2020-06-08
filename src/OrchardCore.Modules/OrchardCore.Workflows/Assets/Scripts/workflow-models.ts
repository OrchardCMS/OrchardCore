namespace Workflows {
    export interface WorkflowType {
        id: number;
        activities: Array<Activity>;
        transitions: Array<Transition>;
        removedActivities: Array<number>;
    }

    export interface Activity {
        id: string;
        x: number;
        y: number;
        isStart: boolean;
        isBlocking?: boolean;
        isEvent: boolean;
        outcomes: Array<Outcome>;
    }

    export interface Outcome {
        name: string;
        displayName: string;
    }

    export interface Transition {
        sourceActivityId: string;
        destinationActivityId: string;
        sourceOutcomeName: string;
    }
}
