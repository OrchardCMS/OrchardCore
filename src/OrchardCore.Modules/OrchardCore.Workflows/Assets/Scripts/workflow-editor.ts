// typings.d.ts is a pure ambient declaration file (global interfaces, no exports), so there's
// nothing to `import` - a path reference is the only way to bring it into scope.
// eslint-disable-next-line @typescript-eslint/triple-slash-reference
///<reference path='../Lib/jsplumb/typings.d.ts' />

import { WorkflowType, Activity, Outcome } from "./workflow-models";
import "./activity-picker";
import "./workflow-url-generator";
import WorkflowCanvas from "./workflow-canvas";

// Bootstrap is loaded as a shared global script resource (see ResourceManagementOptionsConfiguration.cs),
// not bundled here — this brings in its types only, with no runtime import/bundling.
declare const bootstrap: typeof import("bootstrap");

// TODO: Re-implement this using a MVVM approach.
class WorkflowEditor extends WorkflowCanvas {
    private jsPlumbInstance!: jsPlumbInstance;
    private isPopoverVisible!: boolean;
    private hasDragged!: boolean;
    private dragStart!: { left: number; top: number };

    constructor(
        protected container: HTMLElement,
        protected workflowType: WorkflowType,
        private deleteActivityPrompt: string,
        private localId: string,
        loadLocalState: boolean,
    ) {
        super(container, workflowType);

        jsPlumb.ready(() => {
            jsPlumb.importDefaults(this.getDefaults());

            const plumber = this.createJsPlumbInstance();

            // Listen for new connections.
            plumber.bind("connection", (connInfo: { connection: Connection; sourceEndpoint: Endpoint }) => {
                const connection = connInfo.connection;
                const outcome: Outcome = connection.getParameters().outcome;

                const label: Overlay = connection.getOverlay("label");
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
                        sourceEndpoint.hideOverlay("outcome-label");
                    }
                }

                // Re-orient labels after connection
                requestAnimationFrame(() => {
                    this.orientOutcomeLabels();
                });
            });

            // Listen for detached connections.
            plumber.bind("connectionDetached", (connInfo: { sourceEndpoint: Endpoint }) => {
                const sourceEndpoint = connInfo.sourceEndpoint;

                // Change anchor back to ContinuousRight when no connections remain
                if (sourceEndpoint && sourceEndpoint.connections && sourceEndpoint.connections.length === 0) {
                    if (sourceEndpoint.setAnchor) {
                        sourceEndpoint.setAnchor('ContinuousRight');
                    }

                    // Show the outcome label if no connections remain on this endpoint, but only if it has content.
                    if (sourceEndpoint.showOverlay) {
                        const overlay = sourceEndpoint.getOverlay('outcome-label');
                        if (overlay) {
                            const outcome: Outcome = sourceEndpoint.getParameters().outcome;
                            if (outcome && outcome.displayName) {
                                sourceEndpoint.showOverlay("outcome-label");
                            }
                        }
                    }
                }

                // Re-orient labels after disconnection
                requestAnimationFrame(() => {
                    this.orientOutcomeLabels();
                });
            });

            const activityElements = this.getActivityElements();

            const areEqualOutcomes = function (outcomes1: Outcome[], outcomes2: Outcome[]): boolean {
                if (outcomes1.length != outcomes2.length) {
                    return false;
                }

                for (let i = 0; i < outcomes1.length; i++) {
                    const outcome1 = outcomes1[i];
                    const outcome2 = outcomes2[i];

                    if (outcome1.name != outcome2.name || outcome1.displayName != outcome2.displayName) {
                        return false;
                    }
                }

                return true;
            };

            // Suspend drawing and initialize.
            plumber.batch(() => {
                const serverworkflowType: WorkflowType = this.workflowType;

                if (loadLocalState) {
                    const localState: WorkflowType = this.loadLocalState();

                    if (localState) {
                        this.workflowType = localState;
                    }
                }

                activityElements.forEach((activityElement) => {
                    const activityId = activityElement.dataset.activityId as string;
                    const isDeleted = this.workflowType.removedActivities.indexOf(activityId) > -1;

                    if (isDeleted) {
                        activityElement.remove();
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
                        } else {
                            // The available outcomes might have changed when editing an activity,
                            // so we need to check for that and update the client's activity outcomes if so.
                            const sameOutcomes = areEqualOutcomes(activity.outcomes, serverActivity.outcomes);

                            if (!sameOutcomes) {
                                activity.outcomes = serverActivity.outcomes;
                            }

                            activityElement.style.left = `${activity.x}px`;
                            activityElement.style.top = `${activity.y}px`;
                            activityElement.classList.toggle("activity-start", activity.isStart);
                            activityElement.dataset.activityStart = String(activity.isStart);
                        }
                    }

                    // Make the activity draggable.
                    plumber.draggable(activityElement, {
                        grid: [10, 10],
                        containment: true,
                        start: (args: { e: MouseEvent }) => {
                            this.dragStart = { left: args.e.screenX, top: args.e.screenY };
                        },
                        stop: (args: { e: MouseEvent }) => {
                            this.hasDragged = this.dragStart.left != args.e.screenX || this.dragStart.top != args.e.screenY;
                            this.updateCanvasHeight();
                            // Re-orient labels after dragging (connections may have repositioned)
                            requestAnimationFrame(() => {
                                this.orientOutcomeLabels();
                            });
                        },
                    });

                    // Configure the activity as a target.
                    plumber.makeTarget(activityElement, {
                        dropOptions: { hoverClass: "hover" },
                        anchor: "Continuous",
                        endpoint: ["Blank", { radius: 8 }],
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
                        const isDeleted = this.workflowType.removedActivities.indexOf(activityId) > -1;

                        if (isDeleted) {
                            return;
                        }

                        const activity = this.getActivity(activityId);

                        // Add source endpoints after layout is complete
                        for (const outcome of activity.outcomes) {
                            const sourceEndpointOptions = this.getSourceEndpointOptions(activity, outcome);
                            const endpoint = plumber.addEndpoint(
                                activityElement,
                                { connectorOverlays: [["Label", { label: outcome.displayName, cssClass: "connection-label" }]] },
                                sourceEndpointOptions,
                            );

                            this.endpointMap.push({ endpoint, activityElement });

                            // Add Title for each dot, only if outcome has a display name.
                            if (endpoint.canvas && outcome.displayName) {
                                endpoint.canvas.setAttribute("title", outcome.displayName);
                            }
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

            // Use requestAnimationFrame to ensure DOM has been updated before orienting labels
            requestAnimationFrame(() => {
                this.orientOutcomeLabels();
            });

            // Initialize popovers.
            activityElements.forEach((activityElement) => {
                new bootstrap.Popover(activityElement, {
                    trigger: "manual",
                    html: true,
                    content: () => {
                        const content = activityElement.querySelector(".activity-commands")?.cloneNode(true) as HTMLElement;
                        const startButton = content.querySelector<HTMLElement>(".activity-start-action");
                        const isStart = activityElement.dataset.activityStart === "true";
                        const activityId = activityElement.dataset.activityId as string;
                        startButton?.setAttribute("aria-pressed", activityElement.dataset.activityStart ?? "false");
                        startButton?.classList.toggle("active", isStart);

                        content.addEventListener("click", (e) => {
                            const button = (e.target as Element)?.closest<HTMLElement>(".activity-start-action");
                            if (!button) {
                                return;
                            }
                            e.preventDefault();

                            const isStart = button.classList.contains("active");
                            activityElement.dataset.activityStart = String(isStart);
                            activityElement.classList.toggle("activity-start", isStart);
                        });

                        content.addEventListener("click", (e) => {
                            const deleteButton = (e.target as Element)?.closest(".activity-delete-action");
                            if (!deleteButton) {
                                return;
                            }
                            e.preventDefault();

                            // TODO: The prompts are really annoying. Consider showing some sort of small message balloon somewhere to undo the action instead.
                            //if (!confirm(self.deleteActivityPrompt)) {
                            //    return;
                            //}

                            this.workflowType.removedActivities.push(activityId);
                            plumber.remove(activityElement);
                            bootstrap.Popover.getInstance(activityElement)?.dispose();
                        });

                        content.addEventListener("click", (e) => {
                            const persistTrigger = (e.target as Element)?.closest("[data-persist-workflow]");
                            if (!persistTrigger) {
                                return;
                            }
                            this.saveLocalState();
                        });

                        return content;
                    },
                });
            });

            container.addEventListener("click", (e) => {
                const sender = (e.target as Element)?.closest<HTMLElement>(".activity");
                if (!sender) {
                    return;
                }

                if (this.hasDragged) {
                    this.hasDragged = false;
                    return;
                }

                // if any other popovers are visible, hide them
                if (this.isPopoverVisible) {
                    activityElements.forEach((el) => bootstrap.Popover.getInstance(el)?.hide());
                }

                bootstrap.Popover.getOrCreateInstance(sender).show();

                // handle clicking on the popover itself.
                document.querySelectorAll(".popover").forEach((popoverEl) => {
                    popoverEl.addEventListener("click", (e2) => {
                        e2.stopPropagation();
                    });
                });

                e.stopPropagation();
                this.isPopoverVisible = true;
            });

            container.addEventListener("dblclick", (e) => {
                const sender = (e.target as Element)?.closest<HTMLElement>(".activity");
                if (!sender) {
                    return;
                }

                const hasEditor = sender.dataset.activityHasEditor === "true";

                if (hasEditor) {
                    this.saveLocalState();
                    sender.querySelector<HTMLElement>(".activity-edit-action")?.click();
                }
            });

            // Hide all popovers when clicking anywhere but on an activity.
            document.body.addEventListener("click", () => {
                activityElements.forEach((el) => bootstrap.Popover.getInstance(el)?.hide());
                this.isPopoverVisible = false;
            });

            // Save local changes if the event target has the 'data-persist-workflow' attribute.
            document.body.addEventListener("click", (e) => {
                const trigger = (e.target as Element)?.closest("[data-persist-workflow]");
                if (trigger) {
                    this.saveLocalState();
                }
            });

            this.jsPlumbInstance = plumber;
        });
    }

    private getState = (): WorkflowType => {
        const allActivityElements = this.container.querySelectorAll<HTMLElement>(".activity");
        const workflow: WorkflowType = {
            id: this.workflowType.id,
            activities: [],
            transitions: [],
            removedActivities: this.workflowType.removedActivities,
        };

        // Collect activities.
        allActivityElements.forEach((activityElement) => {
            const activityId = activityElement.dataset.activityId as string;
            const activityIsStart = activityElement.dataset.activityStart === "true";
            const activityIsEvent = activityElement.dataset.activityType === "Event";
            const activity: Activity = this.getActivity(activityId);

            workflow.activities.push({
                id: activityId,
                isStart: activityIsStart,
                isEvent: activityIsEvent,
                outcomes: activity.outcomes,
                x: activityElement.offsetLeft,
                y: activityElement.offsetTop,
            });
        });

        // Collect connections.
        const allConnections = this.jsPlumbInstance.getConnections();

        for (let i = 0; i < allConnections.length; i++) {
            const connection = allConnections[i];
            const sourceEndpoint: Endpoint = connection.endpoints[0];
            const sourceOutcomeName = sourceEndpoint.getParameters().outcome.name;
            const sourceActivityId = (connection.source as HTMLElement).dataset.activityId as string;
            const destinationActivityId = (connection.target as HTMLElement).dataset.activityId as string;

            workflow.transitions.push({
                sourceActivityId: sourceActivityId,
                destinationActivityId: destinationActivityId,
                sourceOutcomeName: sourceOutcomeName,
            });
        }

        return workflow;
    };

    public serialize = (): string => {
        const workflow: WorkflowType = this.getState();
        return JSON.stringify(workflow);
    };

    private saveLocalState = (): void => {
        sessionStorage[this.localId] = this.serialize();
    };

    private loadLocalState = (): WorkflowType => {
        return JSON.parse(sessionStorage[this.localId]);
    };
}

const workflowEditorInstances = new WeakMap<HTMLElement, WorkflowEditor>();

function initWorkflowEditor(element: HTMLElement): WorkflowEditor {
    const workflowType: WorkflowType = JSON.parse(element.dataset.workflowType ?? '{}');
    const deleteActivityPrompt = element.dataset.workflowDeleteActivityPrompt ?? '';
    const localId = element.dataset.workflowLocalId ?? '';
    const loadLocalState = element.dataset.workflowLoadLocalState === 'true';

    workflowType.removedActivities = workflowType.removedActivities || [];
    const instance = new WorkflowEditor(element, workflowType, deleteActivityPrompt, localId, loadLocalState);
    workflowEditorInstances.set(element, instance);
    return instance;
}

document.addEventListener("DOMContentLoaded", () => {
    let workflowEditor: WorkflowEditor | undefined;

    document.querySelectorAll<HTMLElement>(".workflow-canvas").forEach((element) => {
        workflowEditor = initWorkflowEditor(element);
    });

    document.getElementById("workflowEditorForm")?.addEventListener("submit", () => {
        const state = workflowEditor?.serialize();
        const stateInput = document.getElementById("workflowStateInput") as HTMLInputElement | null;
        if (stateInput && state !== undefined) {
            stateInput.value = state;
        }
    });
});
