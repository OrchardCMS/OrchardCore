import { reactive } from "vue";
import type { DesignIssue, DesignerDefinition, DesignerInstance, DesignerNode, DesignerTransition, SavePayload, SaveResult, WorkflowSettings } from "../api/types";
import { History, type Command } from "./history";
import type { Graph } from "./commands";

/**
 * The autosave status: `offline` is retrying after a network or server error, `failed` was rejected by
 * the server and waits for a retry, and `conflict` waits for the user to reload or overwrite.
 */
export type SaveStatus = "saved" | "saving" | "unsaved" | "offline" | "failed" | "conflict";

export interface Viewport {
    panX: number;
    panY: number;
    zoom: number;
}

export interface DesignerState {
    loaded: boolean;
    id: number;
    workflowTypeId: string;
    revision: number;
    hasDraft: boolean;
    draftModifiedBy: string | null;
    draftModifiedByUserId: string | null;
    draftModifiedUtc: string | null;
    /**
     * The signed-in user, so the draft's last editor becomes them when they change it.
     */
    currentUserId: string | null;
    runningInstanceCount: number;
    /**
     * The workflow instance shown by the read-only instance viewer.
     */
    instance: DesignerInstance | null;
    settings: WorkflowSettings | null;
    nodes: DesignerNode[];
    transitions: DesignerTransition[];
    issues: DesignIssue[];
    selectedNodeIds: string[];
    selectedTransitionKey: string | null;
    viewport: Viewport;
    saveStatus: SaveStatus;
    /**
     * Increments on every graph change; autosave compares it with the version it last sent.
     */
    changeVersion: number;
    canUndo: boolean;
    canRedo: boolean;
}

const createInitialState = (): DesignerState => ({
    loaded: false,
    id: 0,
    workflowTypeId: "",
    revision: 0,
    hasDraft: false,
    draftModifiedBy: null,
    draftModifiedByUserId: null,
    draftModifiedUtc: null,
    currentUserId: null,
    runningInstanceCount: 0,
    instance: null,
    settings: null,
    nodes: [],
    transitions: [],
    issues: [],
    selectedNodeIds: [],
    selectedTransitionKey: null,
    viewport: { panX: 0, panY: 0, zoom: 1 },
    saveStatus: "saved",
    changeVersion: 0,
    canUndo: false,
    canRedo: false,
});

/**
 * The designer state: graph, selection, viewport, revision, save status and issues. A module-level
 * instance (`designerStore`) is shared by the components; tests create their own.
 */
export const createDesignerStore = () => {
    const state = reactive(createInitialState());
    const history = new History();

    // The activities the draft (or live type) on the server has, so a save can tell which activities were
    // removed and which removed ones were brought back (for example by undo).
    let serverActivityIds = new Set<string>();

    const graph: Graph = {
        get nodes() {
            return state.nodes;
        },
        get transitions() {
            return state.transitions;
        },
    };

    history.subscribe(() => {
        state.canUndo = history.canUndo;
        state.canRedo = history.canRedo;
    });

    const markChanged = () => {
        state.changeVersion++;
    };

    // The server changed the draft on behalf of the signed-in user, who is now its last editor.
    const touchDraft = () => {
        state.hasDraft = true;
        state.draftModifiedBy = null;
        state.draftModifiedByUserId = state.currentUserId;
        state.draftModifiedUtc = new Date().toISOString();
    };

    return {
        state,
        graph,
        history,

        loadDefinition(definition: DesignerDefinition) {
            Object.assign(state, {
                loaded: true,
                id: definition.id,
                workflowTypeId: definition.workflowTypeId,
                revision: definition.revision,
                hasDraft: definition.hasDraft,
                draftModifiedBy: definition.draftModifiedBy ?? null,
                draftModifiedByUserId: definition.draftModifiedByUserId ?? null,
                draftModifiedUtc: definition.draftModifiedUtc ?? null,
                runningInstanceCount: definition.runningInstanceCount,
                instance: definition.instance ?? null,
                settings: definition.settings,
                nodes: definition.nodes,
                transitions: definition.transitions,
                issues: definition.issues,
                selectedNodeIds: [],
                selectedTransitionKey: null,
                saveStatus: "saved" satisfies SaveStatus,
            });

            serverActivityIds = new Set(definition.nodes.map((node) => node.id));
            history.clear();
        },

        getNode(id: string) {
            return state.nodes.find((node) => node.id === id);
        },

        /**
         * Applies a command and records it in the history.
         */
        execute(command: Command) {
            history.execute(command);
            markChanged();
        },

        /**
         * Records a command whose effect is already applied, for example a drag.
         */
        record(command: Command) {
            history.record(command);
            markChanged();
        },

        undo() {
            if (history.undo()) {
                markChanged();
            }
        },

        redo() {
            if (history.redo()) {
                markChanged();
            }
        },

        /**
         * Records an activity that the server just created (AddActivity).
         */
        registerServerNode(node: DesignerNode, revision: number, issues: DesignIssue[]) {
            serverActivityIds.add(node.id);
            state.revision = revision;
            state.issues = issues;
            touchDraft();
        },

        /**
         * Replaces a node with the server's version after an editor apply, and removes the transitions of the
         * outcomes it no longer has (including unsaved local ones, which the server doesn't know about).
         * Its properties changed on the server and can't be reverted locally, so the undo history is cleared.
         * Returns the removed transitions.
         */
        replaceNode(node: DesignerNode): DesignerTransition[] {
            const index = state.nodes.findIndex((existing) => existing.id === node.id);

            if (index >= 0) {
                state.nodes.splice(index, 1, { ...node, x: state.nodes[index].x, y: state.nodes[index].y, isStart: state.nodes[index].isStart });
            }

            const outcomes = new Set(node.outcomes.map((outcome) => outcome.name));
            const removed: DesignerTransition[] = [];

            for (let i = state.transitions.length - 1; i >= 0; i--) {
                const transition = state.transitions[i];

                if (transition.sourceActivityId === node.id && !outcomes.has(transition.sourceOutcomeName)) {
                    removed.unshift(transition);
                    state.transitions.splice(i, 1);
                }
            }

            if (removed.length > 0) {
                markChanged();
            }

            history.markBoundary();

            return removed;
        },

        /**
         * Records the revision and issues returned by a successful server change.
         */
        applyServerChange(revision: number, issues: DesignIssue[]) {
            state.revision = revision;
            state.issues = issues;
            touchDraft();
        },

        /**
         * The body of a Save request for the current graph.
         */
        toSavePayload(): SavePayload {
            const currentIds = new Set(state.nodes.map((node) => node.id));

            return {
                revision: state.revision,
                nodes: state.nodes.map((node) => ({ id: node.id, x: Math.round(node.x), y: Math.round(node.y), isStart: node.isStart })),
                transitions: state.transitions.map((transition) => ({
                    sourceActivityId: transition.sourceActivityId,
                    sourceOutcomeName: transition.sourceOutcomeName,
                    destinationActivityId: transition.destinationActivityId,
                })),
                removedActivityIds: [...serverActivityIds].filter((id) => !currentIds.has(id)),
                restoredActivityIds: [...currentIds].filter((id) => !serverActivityIds.has(id)),
            };
        },

        /**
         * Records a successful save of `payload`.
         */
        markSaved(payload: SavePayload, result: SaveResult) {
            serverActivityIds = new Set(payload.nodes.map((node) => node.id));
            state.revision = result.revision;
            state.issues = result.issues;
            touchDraft();
        },

        /**
         * Records a successful publish: the draft is gone, so the next change starts a new one from the live
         * type (revision 0), and undo can't restore what the draft's trash held.
         */
        markPublished(issues: DesignIssue[]) {
            Object.assign(state, {
                revision: 0,
                hasDraft: false,
                draftModifiedBy: null,
                draftModifiedByUserId: null,
                draftModifiedUtc: null,
                issues,
            });

            history.clear();
        },

        reset() {
            Object.assign(state, createInitialState());
            serverActivityIds = new Set();
            history.clear();
        },
    };
};

export type DesignerStore = ReturnType<typeof createDesignerStore>;

export const designerStore = createDesignerStore();
