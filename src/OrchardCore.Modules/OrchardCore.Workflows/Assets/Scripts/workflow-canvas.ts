///<reference path='../Lib/jquery/typings.d.ts' />
///<reference path='../Lib/jsplumb/typings.d.ts' />

import './workflow-models';

abstract class WorkflowCanvas {
    private minCanvasHeight: number = 400;
    protected endpointMap: Array<{ endpoint: any, activityElement: HTMLElement }> = [];

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
        const displayName = outcome.displayName || '';
        
        return {
            endpoint: 'Dot',
            anchor: 'ContinuousRight',
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
            overlays: displayName ? [['Label', { label: displayName, cssClass: 'outcome-label', id: 'outcome-label', location: [1.6, 0] }]] : [],
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

    protected orientOutcomeLabels = () => {
        for (const { endpoint, activityElement } of this.endpointMap) {
            const overlay: any = endpoint.getOverlay ? endpoint.getOverlay('outcome-label') : null;
            if (!overlay) continue;

            const overlayEl = overlay.getElement ? overlay.getElement() : overlay.canvas;
            if (!overlayEl) continue;

            // Hide empty labels
            const labelText = $(overlayEl).text().trim();
            if (!labelText) {
                $(overlayEl).hide();
                continue;
            }

            const epCanvas = endpoint.canvas;
            if (!epCanvas) continue;

            const activityRect = activityElement.getBoundingClientRect();
            const epRect = epCanvas.getBoundingClientRect();

            const activityCenterX = activityRect.left + activityRect.width / 2;
            const activityCenterY = activityRect.top + activityRect.height / 2;
            const epCenterX = epRect.left + epRect.width / 2;
            const epCenterY = epRect.top + epRect.height / 2;

            const dx = epCenterX - activityCenterX;
            const dy = epCenterY - activityCenterY;
            const normDx = activityRect.width > 0 ? Math.abs(dx) / (activityRect.width / 2) : 0;
            const normDy = activityRect.height > 0 ? Math.abs(dy) / (activityRect.height / 2) : 0;

            let face: string;
            if (normDx > normDy) {
                face = dx > 0 ? 'right' : 'left';
            } else {
                face = dy > 0 ? 'bottom' : 'top';
            }

            const $ol = $(overlayEl);
            $ol.show();

            // Measure label width, temporarily showing if hidden.
            let halfWidth = $ol.outerWidth() / 2;
            if (halfWidth === 0) {
                const prevDisplay = overlayEl.style.display;
                overlayEl.style.display = 'block';
                halfWidth = $ol.outerWidth() / 2;
                overlayEl.style.display = prevDisplay;
            }
            const labelOffset = Math.max(halfWidth - 6, 0);

            const overlayHtmlElement = overlayEl as HTMLElement;
            const storedBaseTransform = overlayHtmlElement.dataset.outcomeLabelBaseTransform;
            const currentTransform = overlayHtmlElement.style.transform || '';
            const normalizedCurrentTransform = !currentTransform || currentTransform === 'none' ? '' : currentTransform;
            const baseTransform = storedBaseTransform ?? normalizedCurrentTransform;
            if (!storedBaseTransform) {
                overlayHtmlElement.dataset.outcomeLabelBaseTransform = baseTransform;
            }

            let orientedTransform = '';

            switch (face) {
                case 'bottom':
                    orientedTransform = `translateY(${labelOffset}px) rotate(90deg)`;
                    break;
                case 'top':
                    orientedTransform = `translateY(-${labelOffset}px) rotate(-90deg)`;
                    break;
                case 'left':
                    orientedTransform = `translateX(-${labelOffset}px)`;
                    break;
                case 'right':
                    orientedTransform = `translateX(${labelOffset}px)`;
                    break;
            }

            const transformPrefix = baseTransform ? `${baseTransform} ` : '';
            $ol.css({
                'transform': `${transformPrefix}${orientedTransform}`.trim(),
                'transform-origin': 'center center'
            });

            // Ensure left/right outcome badges are vertically centered with the endpoint dot.
            if (face === 'left' || face === 'right') {
                const overlayRect = overlayEl.getBoundingClientRect();
                const overlayCenterY = overlayRect.top + overlayRect.height / 2;
                const verticalOffset = Math.round((epCenterY - overlayCenterY) * 10) / 10;

                if (Math.abs(verticalOffset) >= 0.5) {
                    $ol.css('transform', `${transformPrefix}${orientedTransform} translateY(${verticalOffset}px)`.trim());
                }
            }
        }
    };
}

export default WorkflowCanvas;
