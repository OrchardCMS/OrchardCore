namespace Workflows {

    export interface Workflow {
        activities: Array<Activity>;
        connections: Array<Connection>;
    }

    export interface Activity {
        id: number;
        left: number;
        top: number;
        outcomes: Array<Outcome>;
    }

    export interface Outcome {
        name: string;
        displayName: string;
    }

    export interface Connection {
        outcomeName: string;
        sourceId: number;
        targetId: number;
    }
}