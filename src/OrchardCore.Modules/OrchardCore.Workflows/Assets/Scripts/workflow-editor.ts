///<reference path='../Lib/jquery/typings.d.ts' />
///<reference path='../Lib/jsplumb/typings.d.ts' />
///<reference path='./workflow-models.ts' />

// TODO: Re-implement this using an MVVM approach.
class WorkflowEditor {
    private isPopoverVisible: boolean;
    private isDragging: boolean;
    private minCanvasHeight: number = 400;

    constructor(private container: HTMLElement, private workflowDefinition: Workflows.Workflow, private deleteActivityPrompt: string, private localId: string, loadLocalState: boolean) {
        const self = this;
        jsPlumb.ready(() => {

            jsPlumb.importDefaults({
                Anchor: "Continuous",
                // default drag options
                DragOptions: { cursor: 'pointer', zIndex: 2000 },
                // default to blue at one end and green at the other
                EndpointStyles: [{ fillStyle: '#225588' }],
                // blue endpoints 7 px; Blank endpoints.
                Endpoints: [["Dot", { radius: 7 }], ["Blank"]],
                // the overlays to decorate each connection with.  note that the label overlay uses a function to generate the label text; in this
                // case it returns the 'labelText' member that we set on each connection in the 'init' method below.
                ConnectionOverlays: [
                    ["Arrow", { width: 12, length: 12, location: -5 }],
                    // ["Label", { location: 0.1, id: "label", cssClass: "aLabel" }]
                ],
                ConnectorZIndex: 5
            });

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
                    connectorOverlays: [['Label', { location: [3, -1.5], cssClass: 'endpointSourceLabel' }]],
                    dragOptions: {},
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
            plumber.batch(() => {
                var serverWorkflowDefinition: Workflows.Workflow = this.workflowDefinition;
                var workflowId: number = this.workflowDefinition.id;

                if (loadLocalState) {
                    const localState: Workflows.Workflow = this.loadLocalState();

                    if (localState) {
                        this.workflowDefinition = localState;
                    }
                }

                activityElements.each((index, activityElement) => {
                    const $activityElement = $(activityElement);
                    const activityId = $activityElement.data('activity-id');
                    let activity = this.getActivity(activityId);

                    // Update the activity's visual state.
                    if (loadLocalState) {
                        if (activity == null) {
                            // This is a newly added activity not yet added to local state.
                            activity = this.getActivity(activityId, serverWorkflowDefinition.activities);
                            this.workflowDefinition.activities.push(activity);

                            activity.x = 50;
                            activity.y = 50;
                        }

                        $activityElement
                            .css({ left: activity.x, top: activity.y })
                            .toggleClass('activity-start', activity.isStart)
                            .data('activity-start', activity.isStart);
                    }

                    // Make the activity draggable.
                    plumber.draggable(activityElement, {
                        grid: [10, 10],
                        containment: true,
                        start: () => {
                            this.isDragging = true;
                        },
                        stop: () => {
                            this.updateCanvasHeight();
                        }
                    });

                    // Configure the activity as a target.
                    plumber.makeTarget(activityElement, {
                        dropOptions: { hoverClass: 'hover' },
                        anchor: 'Continuous',
                        endpoint: ['Blank', { radius: 8 }]
                    });

                    // Add source endpoints.
                    for (let outcome of activity.outcomes) {
                        const sourceEndpointOptions = getSourceEndpointOptions(activity, outcome);
                        plumber.addEndpoint(activityElement, { connectorOverlays: [['Label', { label: outcome.displayName, cssClass: 'connection-label' }]] }, sourceEndpointOptions);
                    }
                });

                // Connect activities.
                for (let transitionModel of this.workflowDefinition.transitions) {
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

                this.updateCanvasHeight();
            });

            // Initialize popovers.
            activityElements.popover({
                trigger: 'manual',
                html: true,
                content: function () {
                    const activityElement = $(this);
                    const $content: JQuery = activityElement.find('.activity-commands').clone().show();
                    const startButton = $content.find('.activity-start-action');
                    const isStart = activityElement.data('activity-start') === true;

                    startButton.attr('aria-pressed', activityElement.data('activity-start'));
                    startButton.toggleClass('active', isStart);

                    $content.on('click', '.activity-start-action', e => {
                        e.preventDefault();
                        const button = $(e.currentTarget);

                        button.button('toggle');

                        const isStart = button.is('.active');
                        activityElement.data('activity-start', isStart);
                        activityElement.toggleClass('activity-start', isStart);
                    });

                    $content.on('click', '.activity-delete-action', e => {
                        e.preventDefault();
                        if (!confirm(self.deleteActivityPrompt)) {
                            return;
                        }

                        plumber.remove(activityElement);
                        activityElement.popover('dispose');
                    });

                    $content.on('click', '[data-persist-workflow]', e => {
                        self.saveLocalState();
                    });

                    return $content.get(0);
                }
            });

            $(container).on('click', '.activity', e => {

                if (this.isDragging) {
                    this.isDragging = false;
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
                this.saveLocalState();
                sender.find('.activity-edit-action').get(0).click();
            });

            // Hide all popovers when clicking anywhere but on an activity.
            $('body').on('click', e => {
                activityElements.popover('hide');
                this.isPopoverVisible = false;
            });

            // Save local changes if the event target has the 'data-persist-workflow' attribute.
            $('body').on('click', '[data-persist-workflow]', e => {
                this.saveLocalState();
            })

            this.jsPlumbInstance = plumber;
        });
    }

    private jsPlumbInstance: jsPlumbInstance;

    private getActivity = function (id: number, activities: Array<Workflows.Activity> = null): Workflows.Activity {
        if (!activities) {
            activities = this.workflowDefinition.activities;
        }
        return $.grep(activities, (x: Workflows.Activity) => x.id === id)[0];
    }

    private getState = function (): Workflows.Workflow {
        const $allActivityElements = $(this.container).find('.activity');
        const workflow: Workflows.Workflow = {
            id: this.workflowDefinition.id,
            activities: [],
            transitions: []
        };

        // Collect activities.
        for (let i = 0; i < $allActivityElements.length; i++) {
            const $activityElement: JQuery = $($allActivityElements[i]);
            const activityId: number = $activityElement.data('activity-id');
            const activityIsStart: boolean = $activityElement.data('activity-start');
            const activityPosition: JQuery.Coordinates = $activityElement.position();
            const activity: Workflows.Activity = this.getActivity(activityId);

            workflow.activities.push({
                id: activityId,
                isStart: activityIsStart,
                outcomes: activity.outcomes,
                x: activityPosition.left,
                y: activityPosition.top
            });
        }

        // Collect connections.
        const allConnections = this.jsPlumbInstance.getConnections();

        for (let i = 0; i < allConnections.length; i++) {
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

        return workflow;
    }

    public serialize = function (): string {
        const workflow: Workflows.Workflow = this.getState();
        return JSON.stringify(workflow);
    }

    private saveLocalState = function (): void {
        const workflow: Workflows.Workflow = this.getState();
        sessionStorage[this.localId] = this.serialize(workflow);
    }

    private loadLocalState = function (): Workflows.Workflow {
        return JSON.parse(sessionStorage[this.localId]);
    }

    private updateCanvasHeight = function () {
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

$.fn.workflowEditor = function (this: JQuery): JQuery {
    this.each((index, element) => {
        var $element = $(element);
        var workflowDefinition: Workflows.Workflow = $element.data('workflow-definition');
        var deleteActivityPrompt: string = $element.data('workflow-delete-activity-prompt');
        var localId: string = $element.data('workflow-local-id');
        var loadLocalState: boolean = $element.data('workflow-load-local-state');

        $element.data('workflowEditor', new WorkflowEditor(element, workflowDefinition, deleteActivityPrompt, localId, loadLocalState));
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