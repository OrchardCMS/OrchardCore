/// <reference path="../Lib/jsplumb/typings.d.ts" />
/// <reference path="etl-pipeline-models.ts" />

class EtlPipelineEditor {
    private jsPlumbInstance: jsPlumbInstance;
    private container: HTMLElement;
    private pipeline: EtlPipeline.Pipeline;
    private localId: string;
    private deletePrompt: string;

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
        this.deletePrompt = deletePrompt;

        if (loadLocalState) {
            const saved = this.loadLocalState();
            if (saved) {
                this.pipeline = this.mergeLocalState(pipeline, saved);
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
                ['Arrow', { width: 12, length: 12, location: -5 }],
                ['Label', {
                    location: 0.5,
                    id: 'label',
                    cssClass: 'connection-label'
                }]
            ],
            Container: this.container
        });

        this.jsPlumbInstance = plumber;
        this.applyPipelineStateToCanvas();

        const activityElements = this.container.querySelectorAll<HTMLElement>('.activity');
        activityElements.forEach((el) => {
            this.setupActivity(plumber, el);
        });

        this.updateConnections(plumber);
        this.updateCanvasHeight();
        requestAnimationFrame(() => plumber.repaintEverything());

        plumber.bind('connection', (connInfo: any) => {
            const connection = connInfo.connection;
            const outcome = connection.getParameters().outcome;
            const label = connection.getOverlay('label');
            if (label && outcome) {
                label.setLabel(outcome.displayName);
            }

            this.saveLocalState();
            this.updateCanvasHeight();
        });

        plumber.bind('connectionDetached', () => {
            this.saveLocalState();
        });
    }

    private mergeLocalState(serverPipeline: EtlPipeline.Pipeline, savedPipeline: EtlPipeline.Pipeline): EtlPipeline.Pipeline {
        const removedActivityIds = new Set(savedPipeline.removedActivities || []);
        const savedActivities = new Map<string, EtlPipeline.Activity>();

        (savedPipeline.activities || []).forEach((activity) => {
            savedActivities.set(activity.id, activity);
        });

        const activities = (serverPipeline.activities || [])
            .filter((activity) => !removedActivityIds.has(activity.id))
            .map((activity) => {
                const saved = savedActivities.get(activity.id);

                return saved
                    ? {
                        ...activity,
                        x: saved.x,
                        y: saved.y,
                        isStart: saved.isStart
                    }
                    : activity;
            });

        this.ensureStartActivity(activities);

        const activityIds = new Set(activities.map((activity) => activity.id));
        const transitions = (savedPipeline.transitions || []).filter((transition) =>
            activityIds.has(transition.sourceActivityId) &&
            activityIds.has(transition.destinationActivityId)
        );

        return {
            ...serverPipeline,
            activities: activities,
            transitions: transitions,
            removedActivities: savedPipeline.removedActivities || []
        };
    }

    private applyPipelineStateToCanvas(): void {
        const activities = new Map<string, EtlPipeline.Activity>();
        const removedActivityIds = new Set(this.pipeline.removedActivities || []);

        this.pipeline.activities.forEach((activity) => {
            activities.set(activity.id, activity);
        });

        this.container.querySelectorAll<HTMLElement>('.activity').forEach((el) => {
            const activityId = el.getAttribute('data-activity-id') || '';
            const activity = activities.get(activityId);

            if (!activity || removedActivityIds.has(activityId)) {
                el.remove();
                return;
            }

            el.style.left = `${activity.x || 0}px`;
            el.style.top = `${activity.y || 0}px`;
            el.setAttribute('data-activity-start', activity.isStart ? 'true' : 'false');
            el.classList.toggle('activity-start', activity.isStart);
        });
    }

    private ensureStartActivity(activities: EtlPipeline.Activity[]): void {
        if (activities.length === 0 || activities.some((activity) => activity.isStart)) {
            return;
        }

        activities[0].isStart = true;
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
                    anchor: 'ContinuousRight' as any,
                    paintStyle: { stroke: color, fill: color, radius: 7, strokeWidth: 1 },
                    isSource: true,
                    connector: ['Flowchart', { stub: [40, 60], gap: 0, cornerRadius: 5, alwaysRespectStubs: true }],
                    connectorStyle: { strokeWidth: 2, stroke: '#999999', joinstyle: 'round', outlineStroke: 'white', outlineWidth: 2 },
                    hoverPaintStyle: { fill: '#216477', stroke: '#216477' },
                    connectorHoverStyle: { strokeWidth: 3, stroke: '#216477', outlineWidth: 5, outlineStroke: 'white' },
                    uuid: `${activityId}-${outcome.name}`,
                    parameters: { outcome: outcome },
                    connectorOverlays: [['Label', { label: outcome.displayName, cssClass: 'connection-label' }]],
                    overlays: [
                        ['Label', {
                            location: [1.6, 0],
                            label: outcome.displayName,
                            cssClass: 'outcome-label',
                            id: 'outcome-label',
                            visible: true
                        }]
                    ]
                } as any);
            });
        }

        plumber.makeTarget(el, {
            dropOptions: { hoverClass: 'hover' },
            anchor: 'Continuous' as any,
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
            deleteBtn.addEventListener('click', (event: Event) => {
                event.preventDefault();

                if (!confirm(this.deletePrompt)) return;

                this.pipeline.removedActivities = this.pipeline.removedActivities || [];
                this.pipeline.removedActivities.push(activity.id);

                this.jsPlumbInstance.remove(el);
                this.promoteStartActivityIfNeeded(activity);
                this.saveLocalState();
            });
        }
    }

    private promoteStartActivityIfNeeded(removedActivity: EtlPipeline.Activity): void {
        if (!removedActivity.isStart) {
            return;
        }

        const nextActivityElement = this.container.querySelector<HTMLElement>('.activity');

        if (!nextActivityElement) {
            return;
        }

        const nextActivityId = nextActivityElement.getAttribute('data-activity-id');
        const nextActivity = this.pipeline.activities.find((activity) => activity.id === nextActivityId);

        if (!nextActivity) {
            return;
        }

        nextActivity.isStart = true;
        nextActivityElement.setAttribute('data-activity-start', 'true');
        nextActivityElement.classList.add('activity-start');
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

        this.ensureStartActivity(activities);

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
            await persistPipeline();
            if (link instanceof HTMLAnchorElement) {
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
            const activeCategory = pickerModal.querySelector<HTMLAnchorElement>('.activity-picker-categories .nav-link.active')?.getAttribute('href')?.substring(1) || 'all';

            cards.forEach((card) => {
                const cardTitle = card.querySelector('.card-title');
                const text = cardTitle ? cardTitle.textContent!.toLowerCase() : '';
                const cardCategory = card.dataset.category || 'all';
                const cardType = card.dataset.activityType || 'all';
                const matchesType = activeActivityType === 'all' || cardType === activeActivityType;
                const matchesQuery = !query || text.indexOf(query) >= 0;
                const matchesCategory = query || activeCategory === 'all' || cardCategory.toLowerCase() === activeCategory.toLowerCase();

                card.style.display = matchesType && matchesQuery && matchesCategory ? '' : 'none';
            });

            pickerModal.querySelectorAll<HTMLElement>('.activity-picker-categories [data-category]').forEach((item) => {
                const category = item.dataset.category || '';
                const hasActivities = !!pickerModal.querySelector(`.activity-card[data-category='${category}'][data-activity-type='${activeActivityType}']`);
                item.style.display = hasActivities ? '' : 'none';
            });
        };

        const setActiveType = (activityType: string): void => {
            activeActivityType = activityType || 'all';
            const allLink = pickerModal.querySelector<HTMLAnchorElement>('.activity-picker-categories [href="#all"]');
            if (allLink) {
                pickerModal.querySelectorAll('.activity-picker-categories .nav-link').forEach((link) => link.classList.remove('active'));
                allLink.classList.add('active');
            }

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

        pickerModal.querySelectorAll<HTMLAnchorElement>('.activity-picker-categories .nav-link').forEach((link) => {
            link.addEventListener('click', (event: Event) => {
                event.preventDefault();
                pickerModal.querySelectorAll('.activity-picker-categories .nav-link').forEach((navLink) => navLink.classList.remove('active'));
                link.classList.add('active');
                applyActivityFilters();
            });
        });
    }
}

document.addEventListener('DOMContentLoaded', initEtlPipelineEditor);
