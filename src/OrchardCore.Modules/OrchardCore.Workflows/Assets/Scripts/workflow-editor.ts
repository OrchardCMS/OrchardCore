///<reference path='../Lib/jquery/typings.d.ts' />
///<reference path='../Lib/jsplumb/typings.d.ts' />
///<reference path='./workflow-models.ts' />

class WorkflowEditor {
    private isPopoverVisible: boolean;
    private isDragging: boolean;

    constructor(private container: HTMLElement, workflowDefinitionData: Workflows.Workflow) {

        jsPlumb.ready(() => {
            var plumber = jsPlumb.getInstance({
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
                Container: container
            });

            var getSourceEndpointOptions = function (activity: Workflows.Activity, outcome: Workflows.Outcome): EndpointOptions {
                // The definition of source endpoints.
                return {
                    endpoint: 'Dot',
                    anchor: 'Continuous',
                    paintStyle: {
                        stroke: '#7AB02C',
                        fill: '#7AB02C',
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
                    dragOptions: {},
                    overlays: [
                        ['Label', {
                            location: [0.5, 1.5],
                            //label: outcome.displayName,
                            cssClass: 'endpointSourceLabel',
                            visible: true
                        }]
                    ],
                    uuid: `${activity.id}-${outcome.name}`,
                    parameters: {
                        outcome: outcome
                    }
                };
            };

            // Listen for new connections.
            plumber.bind('connection', function (connInfo, originalEvent) {
                const connection: Connection = connInfo.connection;
                const outcome: Workflows.Outcome = connection.getParameters().outcome;

                const label: any = connection.getOverlay('label');
                label.setLabel(outcome.displayName);
            });

            const activityElements = $(container).find('.activity');

            // Suspend drawing and initialize.
            plumber.batch(function () {
                var workflowModel: Workflows.Workflow = workflowDefinitionData;
                var workflowId = workflowModel.id;

                activityElements.each((index, activityElement) => {
                    const activityElementQuery = $(activityElement);
                    const activityId = activityElementQuery.data('activity-id');

                    // Make the activity draggable.
                    plumber.draggable(activityElement, { grid: [10, 10], });

                    // Configure the activity as a target.
                    plumber.makeTarget(activityElement, {
                        dropOptions: { hoverClass: 'hover' },
                        anchor: 'Continuous',
                        endpoint: ['Blank', { radius: 8 }]
                    });

                    // Add source endpoints.
                    const activity = $.grep(workflowModel.activities, (x: Workflows.Activity) => x.id == activityId)[0];
                    const hasMultipleOutcomes = activity.outcomes.length > 1;

                    for (let outcome of activity.outcomes) {
                        const sourceEndpointOptions = getSourceEndpointOptions(activity, outcome);
                        plumber.addEndpoint(activityElement, sourceEndpointOptions);
                    }
                });

                // Connect activities.
                for (let transitionModel of workflowModel.transitions) {
                    const sourceEndpointUuid: string = `${transitionModel.sourceActivityId}-${transitionModel.sourceOutcomeName}`;
                    const sourceEndpoint: Endpoint = plumber.getEndpoint(sourceEndpointUuid);
                    const destinationElementId: string = `activity-${workflowId}-${transitionModel.destinationActivityId}`;

                    plumber.connect({
                        source: sourceEndpoint,
                        target: destinationElementId
                    });
                }

                plumber.bind('contextmenu', function (component, originalEvent) {
                });

                plumber.bind('connectionDrag', function (connection) {
                    console.log('connection ' + connection.id + ' is being dragged. suspendedElement is ', connection.suspendedElement, ' of type ', connection.suspendedElementType);
                });

                plumber.bind('connectionDragStop', function (connection) {
                    console.log('connection ' + connection.id + ' was dragged');
                });

                plumber.bind('connectionMoved', function (params) {
                    console.log('connection ' + params.connection.id + ' was moved');
                });
            });

            // Initialize popovers.
            activityElements.popover({
                trigger: 'manual',
                html: true,
                content: function () {
                    const activityElement = $(this);
                    const content: JQuery = activityElement.find('.activity-commands').clone().show();
                    return content.get(0);
                }
            });

            $(container).on('click', '.activity', e => {

                if (this.isDragging) {
                    return;
                }

                // if any other popovers are visible, hide them
                if (this.isPopoverVisible) {
                    activityElements.popover('hide');
                }

                const sender = $(e.currentTarget);
                sender.popover('show');

                // handle clicking on the popover itself.
                $('.popover').off('click').on('click', e2 => {
                    e2.stopPropagation();
                })

                e.stopPropagation();
                this.isPopoverVisible = true;
            });

            $(container).on('dblclick', '.activity', e => {
                const sender = $(e.currentTarget);
                sender.find('.activity-edit-action').get(0).click();
            });

            // Hide all popovers when clicking anywhere but on an activity.
            $('body').on('click', e => {
                activityElements.popover('hide');
                this.isPopoverVisible = false;
            });

            this.jsPlumbInstance = plumber;
        });
    }

    private jsPlumbInstance: jsPlumbInstance;

    public serialize = function (): string {
        const allActivityElements = $(this.container).find('.activity');
        const workflow: any = {
            activities: [],
            transitions: []
        };

        // Collect activity positions.
        for (var i = 0; i < allActivityElements.length; i++) {
            var activityElementQuery = $(allActivityElements[i]);
            var activityId: number = activityElementQuery.data('activity-id');
            var activityPosition = activityElementQuery.position();

            workflow.activities.push({
                id: activityId,
                x: activityPosition.left,
                y: activityPosition.top
            });
        }

        // Collect activity connections.
        const allConnections = this.jsPlumbInstance.getConnections();
        for (var i = 0; i < allConnections.length; i++) {
            var connection = allConnections[i];
            var sourceEndpoint: Endpoint = connection.endpoints[0];
            var sourceOutcomeName = sourceEndpoint.getParameters().outcome.name;
            var sourceActivityId: number = $(connection.source).data('activity-id');
            var destinationActivityId: number = $(connection.target).data('activity-id');

            workflow.transitions.push({
                sourceActivityId: sourceActivityId,
                destinationActivityId: destinationActivityId,
                sourceOutcomeName: sourceOutcomeName
            });
        }
        return JSON.stringify(workflow);
    }
}

$.fn.workflowEditor = function (this: JQuery): JQuery {
    this.each((index, element) => {
        var $element = $(element);
        var workflowDefinitionData: Workflows.Workflow = $element.data('workflow-definition');

        $element.data('workflowEditor', new WorkflowEditor(element, workflowDefinitionData));
    });

    return this;
};

$(document).ready(function () {
    const workflowEditor: WorkflowEditor = $('.workflow-editor-canvas').workflowEditor().data('workflowEditor');

    $('#workflowEditorForm').on('submit', (s, e) => {
        const state = workflowEditor.serialize();
        $('#workflowStateInput').val(state);
    });
});