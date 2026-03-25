/// <reference path="../Lib/jsplumb/typings.d.ts" />
/// <reference path="etl-pipeline-models.ts" />

class EtlPipelineEditor {
    private jsPlumbInstance: jsPlumbInstance;
    private container: HTMLElement;
    private pipeline: EtlPipeline.Pipeline;
    private localId: string;

    constructor(
        container: HTMLElement,
        pipeline: EtlPipeline.Pipeline,
        deletePrompt: string,
        localId: string,
        loadLocalState: boolean
    ) {
        this.container = container;
        this.pipeline = pipeline;
        this.localId = localId;

        if (loadLocalState) {
            const saved = this.loadLocalState();
            if (saved) {
                this.pipeline = saved;
            }
        }

        jsPlumb.ready(() => {
            this.init();
        });
    }

    public saveCurrentState(): void {
        this.saveLocalState();
    }

    private init() {
        const plumber = jsPlumb.getInstance({
            DragOptions: { cursor: 'pointer', zIndex: 2000 },
            ConnectionOverlays: [
                ['Arrow', { width: 12, length: 12, location: -5 }]
            ],
            Container: this.container
        });

        this.jsPlumbInstance = plumber;

        const activityElements = this.container.querySelectorAll<HTMLElement>('.activity');
        activityElements.forEach((el) => {
            this.setupActivity(plumber, el);
        });

        this.updateConnections(plumber);
        this.updateCanvasHeight();
        requestAnimationFrame(() => plumber.repaintEverything());

        plumber.bind('connection', () => {
            this.saveLocalState();
            this.updateCanvasHeight();
        });

        plumber.bind('connectionDetached', () => {
            this.saveLocalState();
        });
    }

    private setupActivity(plumber: jsPlumbInstance, el: HTMLElement) {
        const activityId = el.getAttribute('data-activity-id');
        const activity = this.pipeline.activities.find(a => a.id === activityId);

        if (!activity) return;

        plumber.draggable(el, {
            grid: [10, 10],
            containment: true,
            stop: () => {
                this.updateCanvasHeight();
                this.saveLocalState();
            }
        });

        let color = '#7ab02c';
        if (activity.isTransform) color = '#3a8acd';
        if (activity.isLoad) color = '#6c757d';

        if (activity.outcomes) {
            activity.outcomes.forEach((outcome: EtlPipeline.Outcome) => {
                plumber.addEndpoint(el, {
                    endpoint: 'Dot',
                    anchor: 'Right' as any,
                    paintStyle: { fill: color, radius: 7 },
                    isSource: true,
                    connector: ['Flowchart', { stub: [40, 60], gap: 0, cornerRadius: 5, alwaysRespectStubs: true }],
                    connectorStyle: { strokeWidth: 2, stroke: '#999999', joinstyle: 'round', outlineStroke: 'white', outlineWidth: 2 },
                    hoverPaintStyle: { fill: '#216477', stroke: '#216477' },
                    connectorHoverStyle: { strokeWidth: 3, stroke: '#216477' },
                    uuid: `${activityId}-${outcome.name}`,
                    parameters: { outcome: outcome },
                    overlays: [
                        ['Label', {
                            location: [0.5, 1.5],
                            label: outcome.displayName,
                            cssClass: 'outcome-label',
                            visible: true
                        }]
                    ]
                } as any);
            });
        }

        plumber.makeTarget(el, {
            dropOptions: { hoverClass: 'hover' },
            anchor: 'Left' as any,
            endpoint: ['Blank', { radius: 8 }] as any
        });

        this.setupActivityActions(el, activity);
    }

    private setupActivityActions(el: HTMLElement, activity: EtlPipeline.Activity) {
        el.addEventListener('click', (e: MouseEvent) => {
            const target = e.target as HTMLElement;
            if (target.closest('.activity-commands')) return;

            const commands = el.querySelector('.activity-commands');
            if (commands) {
                commands.classList.toggle('d-none');
            }
        });

        const deleteBtn = el.querySelector('.activity-delete-action');
        if (deleteBtn) {
            deleteBtn.addEventListener('click', () => {
                if (!confirm('Are you sure you want to delete this activity?')) return;

                this.pipeline.removedActivities = this.pipeline.removedActivities || [];
                this.pipeline.removedActivities.push(activity.id);

                this.jsPlumbInstance.remove(el);
                this.saveLocalState();
            });
        }
    }

    private updateConnections(plumber: jsPlumbInstance) {
        plumber.batch(() => {
            this.pipeline.transitions.forEach((t: EtlPipeline.Transition) => {
                const sourceEndpoint = plumber.getEndpoint(`${t.sourceActivityId}-${t.sourceOutcomeName}`);
                const targetEl = this.container.querySelector(`[data-activity-id="${t.destinationActivityId}"]`);

                if (sourceEndpoint && targetEl) {
                    plumber.connect({
                        source: sourceEndpoint,
                        target: targetEl as any,
                        type: 'basic'
                    } as any);
                }
            });
        });
    }

    private updateCanvasHeight() {
        let maxY = 400;
        this.container.querySelectorAll<HTMLElement>('.activity').forEach((el) => {
            const bottom = el.offsetTop + el.offsetHeight + 50;
            if (bottom > maxY) maxY = bottom;
        });
        this.container.style.minHeight = maxY + 'px';
    }

    public getState(): EtlPipeline.Pipeline {
        const activities: EtlPipeline.Activity[] = [];

        this.container.querySelectorAll<HTMLElement>('.activity').forEach((el) => {
            const id = el.getAttribute('data-activity-id');
            const existing = this.pipeline.activities.find(a => a.id === id);
            if (existing) {
                activities.push({
                    ...existing,
                    x: el.offsetLeft,
                    y: el.offsetTop,
                    isStart: el.getAttribute('data-activity-start') === 'true'
                });
            }
        });

        const transitions: EtlPipeline.Transition[] = [];
        const connections = this.jsPlumbInstance.getConnections() as any[];
        connections.forEach((conn: any) => {
            const sourceEndpoint = conn.endpoints[0];
            const outcome = sourceEndpoint.getParameter('outcome');
            const targetEl = conn.target;
            transitions.push({
                sourceActivityId: conn.source.getAttribute('data-activity-id'),
                destinationActivityId: targetEl.getAttribute('data-activity-id'),
                sourceOutcomeName: outcome ? outcome.name : 'Done'
            });
        });

        return {
            ...this.pipeline,
            activities: activities,
            transitions: transitions,
            removedActivities: this.pipeline.removedActivities || []
        };
    }

    public serialize(): string {
        return JSON.stringify(this.getState());
    }

    private saveLocalState() {
        try {
            sessionStorage.setItem(`etl-pipeline-${this.localId}`, this.serialize());
        } catch {
            // Storage full or unavailable
        }
    }

    private loadLocalState(): EtlPipeline.Pipeline | null {
        try {
            const json = sessionStorage.getItem(`etl-pipeline-${this.localId}`);
            return json ? JSON.parse(json) : null;
        } catch {
            return null;
        }
    }
}

function initEtlPipelineEditor(): void {
    const canvas = document.querySelector<HTMLElement>('.etl-canvas');
    if (!canvas) return;

    const pipelineData = canvas.dataset.pipeline;
    if (!pipelineData) return;

    const pipeline: EtlPipeline.Pipeline = JSON.parse(pipelineData);
    pipeline.removedActivities = pipeline.removedActivities || [];

    const deletePrompt = canvas.dataset.deleteActivityPrompt || 'Are you sure?';
    const localId = canvas.dataset.localId || '';
    const loadLocalState = canvas.dataset.loadLocalState === 'true';

    const editor = new EtlPipelineEditor(
        canvas,
        pipeline,
        deletePrompt,
        localId,
        loadLocalState
    );

    // Serialize state into hidden input on form submit
    const form = document.getElementById('pipelineEditorForm') as HTMLFormElement | null;
    const stateInput = document.getElementById('pipelineStateInput') as HTMLInputElement | null;
    const persistPipeline = async (): Promise<boolean> => {
        if (!form || !stateInput) {
            return true;
        }

        stateInput.value = editor.serialize();
        editor.saveCurrentState();

        const formData = new FormData(form);

        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                credentials: 'same-origin',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            return response.ok;
        } catch {
            return false;
        }
    };

    if (form && stateInput) {
        form.addEventListener('submit', () => {
            stateInput.value = editor.serialize();
        });
    }

    const statePreservingLinks = document.querySelectorAll<HTMLElement>('.activity-add-action, .activity-edit-action');
    statePreservingLinks.forEach((link) => {
        link.addEventListener('click', async (event: Event) => {
            event.preventDefault();
            const succeeded = await persistPipeline();
            if (succeeded && link instanceof HTMLAnchorElement) {
                window.location.href = link.href;
            }
        });
    });

    // Activity picker modal category filtering
    const pickerModal = document.getElementById('activity-picker');
    if (pickerModal) {
        let activeActivityType = 'all';

        const applyActivityFilters = (): void => {
            const searchInput = pickerModal.querySelector<HTMLInputElement>('.activity-search');
            const query = (searchInput?.value || '').toLowerCase();
            const cards = pickerModal.querySelectorAll<HTMLElement>('.activity-card');

            cards.forEach((card) => {
                const cardTitle = card.querySelector('.card-title');
                const text = cardTitle ? cardTitle.textContent!.toLowerCase() : '';
                const cardType = card.dataset.activityType || 'all';
                const matchesType = activeActivityType === 'all' || cardType === activeActivityType;
                const matchesQuery = !query || text.indexOf(query) >= 0;

                card.style.display = matchesType && matchesQuery ? '' : 'none';
            });
        };

        const setActiveType = (activityType: string): void => {
            activeActivityType = activityType || 'all';
            applyActivityFilters();
        };

        pickerModal.addEventListener('show.bs.modal', (e: Event) => {
            const modalEvent = e as any;
            const button = modalEvent.relatedTarget as HTMLElement;
            const activityType = button?.dataset.activityType || 'all';
            const title = button?.dataset.pickerTitle || 'Available Activities';

            const modalTitle = pickerModal.querySelector('.modal-title');
            if (modalTitle) {
                modalTitle.textContent = title;
            }

            const searchInput = pickerModal.querySelector<HTMLInputElement>('.activity-search');
            if (searchInput) {
                searchInput.value = '';
            }

            setActiveType(activityType);
        });

        const searchInput = pickerModal.querySelector<HTMLInputElement>('.activity-search');
        if (searchInput) {
            searchInput.addEventListener('input', () => {
                applyActivityFilters();
            });
        }
    }
}

document.addEventListener('DOMContentLoaded', initEtlPipelineEditor);
