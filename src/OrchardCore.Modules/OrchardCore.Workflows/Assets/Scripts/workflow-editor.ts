///<reference path="../Lib/jquery/typings.d.ts" />
///<reference path="../Lib/jsplumb/typings.d.ts" />
///<reference path="./workflow-models.ts" />

class WorkflowEditor {
    constructor(container: HTMLElement, workflowDefinitionData: Workflows.Workflow) {
        jsPlumb.ready(function () {
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

            var getSourceEndpointOptions = function (overlayLabel: string): Endpoint {
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
                            label: overlayLabel,
                            cssClass: "endpointSourceLabel",
                            visible: overlayLabel != null
                        }]
                    ]
                };
            };

            var init = function (connection: Connection) {
                connection.getOverlay("label").setLabel(connection.sourceId.substring(15) + "-" + connection.targetId.substring(15));
            };

            // Suspend drawing and initialize.
            plumber.batch(function () {
                // Listen for new connections; initialise them the same way we initialise the connections at startup.
                plumber.bind("connection", function (connInfo, originalEvent) {
                    init(connInfo.connection);
                });

                // Initialize activities, endpoints and connectors from model.
                var test = workflowDefinitionData;
                var workflowModel: Workflows.Workflow = {
                    activities: [{
                        id: 1,
                        left: 50,
                        top: 50,
                        outcomes: [{
                            name: "Done",
                            displayName: "Gereed"
                        }]
                    }, {
                        id: 2,
                        left: 500,
                        top: 150,
                        outcomes: [{
                            name: "True",
                            displayName: "True"
                        }, {
                            name: "False",
                            displayName: "False"
                        }]
                    }, {
                        id: 3,
                        left: 50,
                        top: 250,
                        outcomes: [{
                            name: "Done",
                            displayName: "Done"
                        }]
                    }, {
                        id: 4,
                        left: 350,
                        top: 250,
                        outcomes: [{
                            name: "Done",
                            displayName: "Done"
                        }]
                    }
                    ],
                    connections: [{
                        outcomeName: "Done",
                        sourceId: 1,
                        targetId: 2
                    }, {
                        outcomeName: "True",
                        sourceId: 2,
                        targetId: 3
                    }, {
                        outcomeName: "False",
                        sourceId: 2,
                        targetId: 4
                    }
                    ]
                };

                for (let activityModel of workflowModel.activities) {
                    // Generate activity HTML element.
                    let activityNode = $(`<div class="activity" style="left:${activityModel.left}px; top:${activityModel.top}px;"></div>`);
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
                        let sourceEndpointOptions = getSourceEndpointOptions(hasMultipleOutcomes ? outcome.displayName : null);
                        plumber.addEndpoint(activityElement, sourceEndpointOptions);
                    }
                }

                plumber.bind("click", function (conn, originalEvent) {
                    plumber.deleteConnection(conn);
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
}

$.fn.workflowEditor = function (this: JQuery): JQuery {
    this.each((index, element) => {
        var $element = $(element);
        var workflowDefinitionData = $element.data("workflow-definition");
        debugger;
        $element.data("workflowEditor", new WorkflowEditor(element, workflowDefinitionData));
    });

    return this;
};

$(document).ready(function () {
    $('.workflow-editor-canvas').workflowEditor();
});