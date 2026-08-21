namespace EtlPipeline {
    export interface Pipeline {
        id: number;
        name: string;
        activities: Array<Activity>;
        transitions: Array<Transition>;
        removedActivities: Array<string>;
    }

    export interface Activity {
        id: string;
        x: number;
        y: number;
        name: string;
        isStart: boolean;
        isSource: boolean;
        isTransform: boolean;
        isLoad: boolean;
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
