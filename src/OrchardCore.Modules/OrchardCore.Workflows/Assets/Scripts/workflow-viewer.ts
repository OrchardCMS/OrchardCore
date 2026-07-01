// typings.d.ts is a pure ambient declaration file (global interfaces, no exports), so there's
// nothing to `import` - a path reference is the only way to bring it into scope.
// eslint-disable-next-line @typescript-eslint/triple-slash-reference
///<reference path='../Lib/jsplumb/typings.d.ts' />

import { WorkflowType, Activity, Outcome } from './workflow-models';
import WorkflowCanvas from './workflow-canvas';

class WorkflowViewer extends WorkflowCanvas {
    private jsPlumbInstance!: jsPlumbInstance;

    constructor(protected container: HTMLElement, protected workflowType: WorkflowType) {
        super(container, workflowType);

        jsPlumb.ready(() => {
            jsPlumb.importDefaults(this.getDefaults());

            const plumber = this.createJsPlumbInstance();

            // Listen for new connections.
            plumber.bind('connection', (connInfo: { connection: Connection; sourceEndpoint: Endpoint }) => {
                const connection = connInfo.connection;
                const outcome: Outcome = connection.getParameters().outcome;

                const label: Overlay = connection.getOverlay('label');
                label.setLabel(outcome.displayName);

                // Change anchor to Continuous for better routing when connected
                const sourceEndpoint = connInfo.sourceEndpoint;
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
                    this.orientOutcomeLabels();
                });
            });

            const activityElements = this.getActivityElements();

            // Suspend drawing and initialize.
            plumber.batch(() => {
                activityElements.forEach((activityElement) => {
                    // Configure the activity as a target.
                    plumber.makeTarget(activityElement, {
                        dropOptions: { hoverClass: 'hover' },
                        anchor: 'Continuous',
                        endpoint: ['Blank', { radius: 8 }]
                    });
                });

                // Make all activity elements visible. The ".activity" class defaults to "display: none"
                // so newly added activities stay hidden until positioned here; clearing the inline style
                // would just fall back to that CSS default instead of showing the element.
                activityElements.forEach((activityElement) => activityElement.style.display = 'block');

                this.updateCanvasHeight();
            });

            // Wait for layout to complete before adding endpoints and connections
            setTimeout(() => {
                plumber.batch(() => {
                    activityElements.forEach((activityElement) => {
                        const activityId = activityElement.dataset.activityId as string;
                        const activity = this.getActivity(activityId);

                        // Add source endpoints after layout is complete
                        for (const outcome of activity.outcomes) {
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

    protected getEndpointColor = (activity: Activity) => {
        return activity.isBlocking ? '#7ab02c' : activity.isEvent ? '#3a8acd' : '#7ab02c';
    }
}

const workflowViewerInstances = new WeakMap<HTMLElement, WorkflowViewer>();

function initWorkflowViewer(element: HTMLElement): WorkflowViewer {
    const workflowType: WorkflowType = JSON.parse(element.dataset.workflowType ?? '{}');
    const instance = new WorkflowViewer(element, workflowType);
    workflowViewerInstances.set(element, instance);
    return instance;
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll<HTMLElement>('.workflow-canvas').forEach((element) => initWorkflowViewer(element));
});
