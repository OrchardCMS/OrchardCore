import { reactive } from "vue";
import type { DesignIssue, DesignerDefinition, DesignerNode, DesignerTransition, SavePayload, SaveResult, WorkflowSettings } from "../api/types";
import { History, type Command } from "./history";
import type { Graph } from "./commands";

export type SaveStatus = "saved" | "saving" | "unsaved" | "offline" | "conflict";

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
    runningInstanceCount: number;
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
    runningInstanceCount: 0,
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
            state.hasDraft = true;
        },

        /**
         * Replaces a node with the server's version after an editor apply. Its properties changed on the
         * server and can't be reverted locally, so the undo history is cleared.
         */
        replaceNode(node: DesignerNode) {
            const index = state.nodes.findIndex((existing) => existing.id === node.id);

            if (index >= 0) {
                state.nodes.splice(index, 1, { ...node, x: state.nodes[index].x, y: state.nodes[index].y, isStart: state.nodes[index].isStart });
            }

            history.markBoundary();
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
            state.hasDraft = true;
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
