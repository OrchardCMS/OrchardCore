///<reference path='../Lib/jsplumb/typings.d.ts' />

import './workflow-models';
import WorkflowCanvas from './workflow-canvas';

class WorkflowViewer extends WorkflowCanvas {
    private jsPlumbInstance!: jsPlumbInstance;

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

                // Change anchor to Continuous for better routing when connected
                const sourceEndpoint: any = connInfo.sourceEndpoint;
                if (sourceEndpoint && sourceEndpoint.setAnchor) {
                    sourceEndpoint.setAnchor('Continuous');
                }

                // Hide the outcome label on the source endpoint since it's now connected, but only if it has content.
                if (sourceEndpoint && sourceEndpoint.hideOverlay && outcome.displayName) {
                    const overlay = sourceEndpoint.getOverlay('outcome-label');
                    if (overlay) {
                        sourceEndpoint.hideOverlay('outcome-label');
                    }
                }

                // Re-orient labels after connection
                requestAnimationFrame(() => {
                    self.orientOutcomeLabels();
                });
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

                activityElements.forEach((activityElement) => {
                    const activityId = activityElement.dataset.activityId as string;
                    const activity = this.getActivity(activityId);

                    // Configure the activity as a target.
                    plumber.makeTarget(activityElement, {
                        dropOptions: { hoverClass: 'hover' },
                        anchor: 'Continuous',
                        endpoint: ['Blank', { radius: 8 }]
                    });
                });

                // Make all activity elements visible
                activityElements.forEach((activityElement) => activityElement.style.display = '');

                this.updateCanvasHeight();
            });

            // Wait for layout to complete before adding endpoints and connections
            setTimeout(() => {
                plumber.batch(() => {
                    activityElements.forEach((activityElement) => {
                        const activityId = activityElement.dataset.activityId as string;
                        const activity = this.getActivity(activityId);

                        // Add source endpoints after layout is complete
                        for (let outcome of activity.outcomes) {
                            const sourceEndpointOptions = this.getSourceEndpointOptions(activity, outcome);
                            const endpoint = plumber.addEndpoint(activityElement, { connectorOverlays: [['Label', { label: outcome.displayName, cssClass: 'connection-label' }]] }, sourceEndpointOptions);
                            this.endpointMap.push({ endpoint, activityElement });
                        }
                    });

                    // Connect activities after endpoints are created
                    this.updateConnections(plumber);
                });

                // Orient labels after everything is set up
                requestAnimationFrame(() => {
                    this.orientOutcomeLabels();
                });
            }, 0);

            this.jsPlumbInstance = plumber;
        });
    }

    protected getEndpointColor = (activity: Workflows.Activity) => {
        return activity.isBlocking ? '#7ab02c' : activity.isEvent ? '#3a8acd' : '#7ab02c';
    }
}

const workflowViewerInstances = new WeakMap<HTMLElement, WorkflowViewer>();

function initWorkflowViewer(element: HTMLElement): WorkflowViewer {
    const workflowType: Workflows.WorkflowType = JSON.parse(element.dataset.workflowType ?? '{}');
    const instance = new WorkflowViewer(element, workflowType);
    workflowViewerInstances.set(element, instance);
    return instance;
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll<HTMLElement>('.workflow-canvas').forEach((element) => initWorkflowViewer(element));
});
