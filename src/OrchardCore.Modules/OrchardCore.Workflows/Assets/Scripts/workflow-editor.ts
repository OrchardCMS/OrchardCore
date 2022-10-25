///<reference path='../Lib/jquery/typings.d.ts' />
///<reference path='../Lib/jsplumb/typings.d.ts' />
///<reference path='./workflow-models.ts' />
///<reference path='./workflow-canvas.ts' />

// TODO: Re-implement this using a MVVM approach.
class WorkflowEditor extends WorkflowCanvas {
    private jsPlumbInstance: jsPlumbInstance;
    private isPopoverVisible: boolean;
    private hasDragged: boolean;
    private dragStart: JQuery.Coordinates;

    constructor(protected container: HTMLElement, protected workflowType: Workflows.WorkflowType, private deleteActivityPrompt: string, private localId: string, loadLocalState: boolean) {
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
                var serverworkflowType: Workflows.WorkflowType = this.workflowType;
                var workflowId: number = this.workflowType.id;

                if (loadLocalState) {
                    const localState: Workflows.WorkflowType = this.loadLocalState();

                    if (localState) {
                        this.workflowType = localState;
                    }
                }

                activityElements.each((index, activityElement) => {
                    const $activityElement = $(activityElement);
                    const activityId = $activityElement.data('activity-id');
                    const isDeleted = this.workflowType.removedActivities.indexOf(activityId) > -1;

                    if (isDeleted) {
                        $activityElement.remove();
                        return;
                    }

                    let activity = this.getActivity(activityId);
                    const serverActivity = this.getActivity(activityId, serverworkflowType.activities);

                    // Update the activity's visual state.
                    if (loadLocalState) {
                        if (activity == null) {
                            // This is a newly added activity not yet added to local state.
                            activity = serverActivity;
                            this.workflowType.activities.push(activity);

                            activity.x = 50;
                            activity.y = 50;
                        }
                        else {
                            // The available outcomes might have changed when editing an activity,
                            // so we need to check for that and update the client's activity outcomes if so.
                            const sameOutcomes = areEqualOutcomes(activity.outcomes, serverActivity.outcomes);

                            if (!sameOutcomes) {
                                activity.outcomes = serverActivity.outcomes;
                            }

                            $activityElement
                                .css({ left: activity.x, top: activity.y })
                                .toggleClass('activity-start', activity.isStart)
                                .data('activity-start', activity.isStart);
                        }
                    }

                    // Make the activity draggable.
                    plumber.draggable(activityElement, {
                        grid: [10, 10],
                        containment: true,
                        start: (args: any) => {
                            this.dragStart = { left: args.e.screenX, top: args.e.screenY };
                        },
                        stop: (args: any) => {
                            this.hasDragged = this.dragStart.left != args.e.screenX || this.dragStart.top != args.e.screenY;
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

            // Initialize popovers.
            activityElements.popover({
                trigger: 'manual',
                html: true,
                content: function () {
                    const activityElement = $(this);
                    const $content: JQuery<Element> = activityElement.find('.activity-commands').clone().show();
                    const startButton = $content.find('.activity-start-action');
                    const isStart = activityElement.data('activity-start') === true;
                    const activityId: number = activityElement.data('activity-id');

                    startButton.attr('aria-pressed', activityElement.data('activity-start'));
                    startButton.toggleClass('active', isStart);

                    $content.on('click', '.activity-start-action', e => {
                        e.preventDefault();
                        const button = $(e.currentTarget);

                        const isStart = button.is('.active');
                        activityElement.data('activity-start', isStart);
                        activityElement.toggleClass('activity-start', isStart);
                    });

                    $content.on('click', '.activity-delete-action', e => {
                        e.preventDefault();

                        // TODO: The prompts are really annoying. Consider showing some sort of small message balloon somewhere to undo the action instead.
                        //if (!confirm(self.deleteActivityPrompt)) {
                        //    return;
                        //}

                        self.workflowType.removedActivities.push(activityId);
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
                if (this.hasDragged) {
                    this.hasDragged = false;
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
                const hasEditor = sender.data('activity-has-editor');

                if (hasEditor) {
                    this.saveLocalState();
                    sender.find('.activity-edit-action').get(0).click();
                }
            });

            // Hide all popovers when clicking anywhere but on an activity.
            $('body').on('click', e => {
                activityElements.popover('hide');
                this.isPopoverVisible = false;
            });

            // Save local changes if the event target has the 'data-persist-workflow' attribute.
            $('body').on('click', '[data-persist-workflow]', e => {
                this.saveLocalState();
            });

            this.jsPlumbInstance = plumber;
        });
    }

    private getState = (): Workflows.WorkflowType => {
        const $allActivityElements = $(this.container).find('.activity');
        const workflow: Workflows.WorkflowType = {
            id: this.workflowType.id,
            activities: [],
            transitions: [],
            removedActivities: this.workflowType.removedActivities
        };

        // Collect activities.
        for (let i = 0; i < $allActivityElements.length; i++) {
            const $activityElement: JQuery = $($allActivityElements[i]);
            const activityId: string = $activityElement.data('activity-id');
            const activityIsStart: boolean = $activityElement.data('activity-start');
            const activityIsEvent: boolean = $activityElement.data('activity-type') === 'Event';
            const activityPosition: JQuery.Coordinates = $activityElement.position();
            const activity: Workflows.Activity = this.getActivity(activityId);

            workflow.activities.push({
                id: activityId,
                isStart: activityIsStart,
                isEvent: activityIsEvent,
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
            var sourceActivityId: string = $(connection.source).data('activity-id');
            var destinationActivityId: string = $(connection.target).data('activity-id');

            workflow.transitions.push({
                sourceActivityId: sourceActivityId,
                destinationActivityId: destinationActivityId,
                sourceOutcomeName: sourceOutcomeName
            });
        }

        return workflow;
    }

    public serialize = (): string => {
        const workflow: Workflows.WorkflowType = this.getState();
        return JSON.stringify(workflow);
    }

    private saveLocalState = (): void => {
        sessionStorage[this.localId] = this.serialize();
    }

    private loadLocalState = (): Workflows.WorkflowType => {
        return JSON.parse(sessionStorage[this.localId]);
    }
}

$.fn.workflowEditor = function (this: JQuery): JQuery {
    this.each((index, element) => {
        var $element = $(element);
        var workflowType: Workflows.WorkflowType = $element.data('workflow-type');
        var deleteActivityPrompt: string = $element.data('workflow-delete-activity-prompt');
        var localId: string = $element.data('workflow-local-id');
        var loadLocalState: boolean = $element.data('workflow-load-local-state');

        workflowType.removedActivities = workflowType.removedActivities || [];
        $element.data('workflowEditor', new WorkflowEditor(element, workflowType, deleteActivityPrompt, localId, loadLocalState));
    });

    return this;
};

$(document).ready(function () {
    const workflowEditor: WorkflowEditor = $('.workflow-canvas').workflowEditor().data('workflowEditor');

    $('#workflowEditorForm').on('submit', (s, e) => {
        const state = workflowEditor.serialize();
        $('#workflowStateInput').val(state);
    });
});
