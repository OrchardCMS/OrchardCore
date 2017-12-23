///<reference path="../Lib/jquery/typings.d.ts" />
///<reference path="../Lib/jsplumb/typings.d.ts" />
///<reference path="./workflow-models.ts" />

class WorkflowEditor {
    constructor(private container: HTMLElement, workflowDefinitionData: Workflows.Workflow) {

        jsPlumb.ready(() => {
            var plumber = jsPlumb.getInstance({
                DragOptions: { cursor: 'pointer', zIndex: 2000 },
                ConnectionOverlays: [
                    ["Arrow", {
                        location: 1,
                        visible: true,
                        width: 11,
                        length: 11
                    }],
                    ["Label", {
                        location: 0.5,
                        id: "label",
                        cssClass: "connection-label"
                    }]
                ],
                Container: container
            });

            var getSourceEndpointOptions = function (outcome: Workflows.Outcome): EndpointOptions {
                // The definition of source endpoints.
                return {
                    endpoint: "Dot",
                    anchor: "Continuous",
                    paintStyle: {
                        stroke: "#7AB02C",
                        fill: "#7AB02C",
                        radius: 7,
                        strokeWidth: 1
                    },
                    isSource: true,
                    connector: ["Flowchart", { stub: [40, 60], gap: 0, cornerRadius: 5, alwaysRespectStubs: true }],
                    connectorStyle: {
                        strokeWidth: 2,
                        stroke: "#999999",
                        joinstyle: "round",
                        outlineStroke: "white",
                        outlineWidth: 2
                    },
                    hoverPaintStyle: {
                        fill: "#216477",
                        stroke: "#216477"
                    },
                    connectorHoverStyle: {
                        strokeWidth: 3,
                        stroke: "#216477",
                        outlineWidth: 5,
                        outlineStroke: "white"
                    },
                    dragOptions: {},
                    overlays: [
                        ["Label", {
                            location: [0.5, 1.5],
                            //label: outcome.displayName,
                            cssClass: "endpointSourceLabel",
                            visible: true
                        }]
                    ],
                    parameters: {
                        outcome: outcome
                    }
                };
            };

            // Suspend drawing and initialize.
            plumber.batch(function () {
                // Listen for new connections; initialise them the same way we initialise the connections at startup.
                plumber.bind("connection", function (connInfo, originalEvent) {
                    const connection: Connection = connInfo.connection;
                    const outcome: Workflows.Outcome = connection.getParameters().outcome;

                    const label: any = connection.getOverlay("label");
                    label.setLabel(outcome.displayName);
                });

                // Initialize activities, endpoints and connectors from model.
                var test = workflowDefinitionData;
                var workflowModel: Workflows.Workflow = workflowDefinitionData;

                for (let activityModel of workflowModel.activities) {
                    // Generate activity HTML element.
                    let activityNode = $(`<div class="activity" style="left:${activityModel.x}px; top:${activityModel.y}px;"></div>`);
                    let activityElement = activityNode[0];

                    // Add activity HTML element to the canvas.
                    $(container).append(activityNode);

                    // Make the activity draggable.
                    plumber.draggable(activityElement, { grid: [20, 20] });

                    // Configure the activity as a target.
                    plumber.makeTarget(activityElement, {
                        dropOptions: { hoverClass: "hover" },
                        anchor: "Continuous",
                        endpoint: ["Blank", { radius: 8 }]
                    });

                    // Add source endpoints.
                    let hasMultipleOutcomes = activityModel.outcomes.length > 1;
                    for (let outcome of activityModel.outcomes) {
                        let sourceEndpointOptions = getSourceEndpointOptions(outcome);
                        var endpoint = plumber.addEndpoint(activityElement, sourceEndpointOptions);
                    }

                    $(activityElement).data("activity-model", activityModel);
                }

                plumber.bind("click", function (conn, originalEvent) {
                    //plumber.deleteConnection(conn);
                });

                plumber.bind("connectionDrag", function (connection) {
                    console.log("connection " + connection.id + " is being dragged. suspendedElement is ", connection.suspendedElement, " of type ", connection.suspendedElementType);
                });

                plumber.bind("connectionDragStop", function (connection) {
                    console.log("connection " + connection.id + " was dragged");
                });

                plumber.bind("connectionMoved", function (params) {
                    console.log("connection " + params.connection.id + " was moved");
                });
            });

            this.jsPlumbInstance = plumber;
        });
    }

    private jsPlumbInstance: jsPlumbInstance;

    public serialize = function (): string {
        const allActivities = $(this.container).find(".activity");
        const workflow: any = {
            activities: [],
            connections: []
        };

        for (var i = 0; i < allActivities.length; i++) {
            var activity = $(allActivities[i]);
            var activityModel: Workflows.Activity = activity.data("activity-model");
            var activityPosition = activity.position();

            workflow.activities.push({
                id: activityModel.id,
                x: activityPosition.left,
                y: activityPosition.top
            });
        }

        const allConnections = this.jsPlumbInstance.getConnections();
        for (var i = 0; i < allConnections.length; i++) {
            var connection = allConnections[i];
            var sourceEndpoint: Endpoint = connection.endpoints[0];
            var sourceOutcomeName = sourceEndpoint.getParameters().outcome.name;
            var sourceActivity: Workflows.Activity = $(connection.source).data("activity-model");
            var destinationActivity: Workflows.Activity = $(connection.target).data("activity-model");

            workflow.connections.push({
                sourceActivityId: sourceActivity.id,
                destinationActivityId: destinationActivity.id,
                sourceOutcomeName: sourceOutcomeName
            });
        }
        return JSON.stringify(workflow);
    }
}

$.fn.workflowEditor = function (this: JQuery): JQuery {
    this.each((index, element) => {
        var $element = $(element);
        var workflowDefinitionData: Workflows.Workflow = $element.data("workflow-definition");

        $element.data("workflowEditor", new WorkflowEditor(element, workflowDefinitionData));
    });

    return this;
};

$(document).ready(function () {
    const workflowEditor: WorkflowEditor = $(".workflow-editor-canvas").workflowEditor().data("workflowEditor");

    $("#workflowEditorForm").on("submit", (s, e) => {
        const state = workflowEditor.serialize();
        $("#workflowStateInput").val(state);
    });
});