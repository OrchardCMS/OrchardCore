///<reference path='../Lib/jquery/typings.d.ts' />
///<reference path='../Lib/jsplumb/typings.d.ts' />
///<reference path='./workflow-models.ts' />

abstract class WorkflowCanvas {
    private minCanvasHeight: number = 400;

    constructor(protected container: HTMLElement, protected workflowType: Workflows.WorkflowType) {
    }

    protected getActivityElements = (): JQuery => {
        return $(this.container).find('.activity');
    }

    protected getDefaults = () => {
        return {
            Anchor: "Continuous",
            DragOptions: { cursor: 'pointer', zIndex: 2000 },
            EndpointStyles: [{ fillStyle: '#225588' }],
            Endpoints: [["Dot", { radius: 7 }], ["Blank"]],
            ConnectionOverlays: [
                ["Arrow", { width: 12, length: 12, location: -5 }],
            ],
            ConnectorZIndex: 5
        }
    };

    protected createJsPlumbInstance = () => {
        return jsPlumb.getInstance({
            DragOptions: { cursor: 'pointer', zIndex: 2000 },
            ConnectionOverlays: [
                ['Arrow', {
                    location: 1,
                    visible: true,
                    width: 11,
                    length: 11
                }],
                ['Label', {
                    location: 0.5,
                    id: 'label',
                    cssClass: 'connection-label'
                }]
            ],
            Container: this.container
        });
    };

    protected getEndpointColor = (activity: Workflows.Activity) => {
        return activity.isBlocking || activity.isStart ? '#7ab02c' : activity.isEvent ? '#3a8acd' : '#7ab02c';
    }

    protected getSourceEndpointOptions = (activity: Workflows.Activity, outcome: Workflows.Outcome): EndpointOptions => {
        // The definition of source endpoints.
        const paintColor = this.getEndpointColor(activity);
        return {
            endpoint: 'Dot',
            anchor: 'Continuous',
            paintStyle: {
                stroke: paintColor,
                fill: paintColor,
                radius: 7,
                strokeWidth: 1
            },
            isSource: true,
            connector: ['Flowchart', { stub: [40, 60], gap: 0, cornerRadius: 5, alwaysRespectStubs: true }],
            connectorStyle: {
                strokeWidth: 2,
                stroke: '#999999',
                joinstyle: 'round',
                outlineStroke: 'white',
                outlineWidth: 2
            },
            hoverPaintStyle: {
                fill: '#216477',
                stroke: '#216477'
            },
            connectorHoverStyle: {
                strokeWidth: 3,
                stroke: '#216477',
                outlineWidth: 5,
                outlineStroke: 'white'
            },
            connectorOverlays: [['Label', { location: [3, -1.5], cssClass: 'endpointSourceLabel' }]],
            dragOptions: {},
            uuid: `${activity.id}-${outcome.name}`,
            parameters: {
                outcome: outcome
            }
        };
    };

    protected getActivity = function (id: string, activities: Array<Workflows.Activity> = null): Workflows.Activity {
        if (!activities) {
            activities = this.workflowType.activities;
        }
        return $.grep(activities, (x: Workflows.Activity) => x.id === id)[0];
    }

    protected updateConnections = (plumber: jsPlumbInstance) => {
        var workflowId: number = this.workflowType.id;

        // Connect activities.
        for (let transitionModel of this.workflowType.transitions) {
            const sourceEndpointUuid: string = `${transitionModel.sourceActivityId}-${transitionModel.sourceOutcomeName}`;
            const sourceEndpoint: Endpoint = plumber.getEndpoint(sourceEndpointUuid);
            const destinationElementId: string = `activity-${workflowId}-${transitionModel.destinationActivityId}`;

            plumber.connect({
                source: sourceEndpoint,
                target: destinationElementId
            });
        }
    }

    protected updateCanvasHeight = function () {
        const $container = $(this.container);

        // Get the activity element with the highest Y coordinate.
        const $activityElements = $container.find(".activity");
        let currentElementTop = 0;
        let currentActivityHeight = 0;

        for (let activityElement of $activityElements.toArray()) {
            const $activityElement = $(activityElement);
            const top = $activityElement.position().top;

            if (top > currentElementTop) {
                currentElementTop = top;
                currentActivityHeight = $activityElement.height();
            }
        }

        let newCanvasHeight = currentElementTop + currentActivityHeight;
        const elementBottom = currentElementTop + currentActivityHeight;
        const stretchValue = 100;

        if (newCanvasHeight - elementBottom <= stretchValue) {
            newCanvasHeight += stretchValue;
        }

        $container.height(Math.max(this.minCanvasHeight, newCanvasHeight));
    };
}
