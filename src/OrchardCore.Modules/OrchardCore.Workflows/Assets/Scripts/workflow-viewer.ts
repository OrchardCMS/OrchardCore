///<reference path='../Lib/jquery/typings.d.ts' />
///<reference path='../Lib/jsplumb/typings.d.ts' />
///<reference path='./workflow-models.ts' />
///<reference path='./workflow-canvas.ts' />

class WorkflowViewer extends WorkflowCanvas {
    private jsPlumbInstance: jsPlumbInstance;

    constructor(protected container: HTMLElement, protected workflowType: Workflows.WorkflowType) {
        super(container, workflowType);
        const self = this;

        jsPlumb.ready(() => {
            jsPlumb.importDefaults(this.getDefaults());

            const plumber = this.createJsPlumbInstance();

            // Listen for new connections.
            plumber.bind('connection', function (connInfo, originalEvent) {
                const connection: Connection = connInfo.connection;
                const outcome: Workflows.Outcome = connection.getParameters().outcome;

                const label: any = connection.getOverlay('label');
                label.setLabel(outcome.displayName);
            });

            let activityElements = this.getActivityElements();

            var areEqualOutcomes = function (outcomes1: Workflows.Outcome[], outcomes2: Workflows.Outcome[]): boolean {
                if (outcomes1.length != outcomes2.length) {
                    return false;
                }

                for (let i = 0; i < outcomes1.length; i++) {
                    const outcome1 = outcomes1[i];
                    const outcome2 = outcomes2[i];

                    if (outcome1.name != outcome2.displayName || outcome1.displayName != outcome2.displayName) {
                        return false;
                    }
                }

                return true;
            }

            // Suspend drawing and initialize.
            plumber.batch(() => {
                var workflowId: number = this.workflowType.id;

                activityElements.each((index, activityElement) => {
                    const $activityElement = $(activityElement);
                    const activityId = $activityElement.data('activity-id');
                    const activity = this.getActivity(activityId);

                    // Configure the activity as a target.
                    plumber.makeTarget(activityElement, {
                        dropOptions: { hoverClass: 'hover' },
                        anchor: 'Continuous',
                        endpoint: ['Blank', { radius: 8 }]
                    });

                    // Add source endpoints.
                    for (let outcome of activity.outcomes) {
                        const sourceEndpointOptions = this.getSourceEndpointOptions(activity, outcome);
                        plumber.addEndpoint(activityElement, { connectorOverlays: [['Label', { label: outcome.displayName, cssClass: 'connection-label' }]] }, sourceEndpointOptions);
                    }
                });

                // Connect activities.
                this.updateConnections(plumber);

                // Re-query the activity elements.
                activityElements = this.getActivityElements();

                // Make all activity elements visible.
                activityElements.show();

                this.updateCanvasHeight();
            });

            this.jsPlumbInstance = plumber;
        });
    }

    protected getEndpointColor = (activity: Workflows.Activity) => {
        return activity.isBlocking ? '#7ab02c' : activity.isEvent ? '#3a8acd' : '#7ab02c';
    }
}

$.fn.workflowViewer = function (this: JQuery): JQuery {
    this.each((index, element) => {
        var $element = $(element);
        var workflowType: Workflows.WorkflowType = $element.data('workflow-type');

        $element.data('workflowViewer', new WorkflowViewer(element, workflowType));
    });

    return this;
};

$(document).ready(function () {
    const workflowViewer: WorkflowViewer = $('.workflow-canvas').workflowViewer().data('workflowViewer');
});
