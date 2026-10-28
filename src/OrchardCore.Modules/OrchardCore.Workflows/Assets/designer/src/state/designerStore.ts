import { reactive } from "vue";
import type {
    DesignIssue,
    DesignerDefinition,
    DesignerInstance,
    DesignerNode,
    DesignerTransition,
    DesignerVersion,
    GlobalValue,
    SavePayload,
    SaveResult,
    VariableDefinition,
    VariableType,
    WorkflowSettings,
} from "../api/types";
import type { Presence, WorkflowTypeChangedMessage } from "../realtime/realtime";
import { createGraphHistory, createServerNodeIds } from "@bloom/designer/graph/graphHistory";
import { danglingEdges } from "@bloom/designer/graph/rules";
import { createWorkflowModel, type Graph } from "./commands";

export type { SaveStatus } from "@bloom/designer/core/types";
import type { SaveStatus } from "@bloom/designer/core/types";

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
    /**
     * The version new instances start on.
     */
    publishedVersion: DesignerVersion | null;
    /**
     * The version a version page shows, or the one the viewed instance runs on.
     */
    version: DesignerVersion | null;
    settings: WorkflowSettings | null;
    /**
     * The declared variables, and the types a declaration can use.
     */
    variables: VariableDefinition[];
    variableTypes: VariableType[];
    // The global values and functions every expression can use.
    globalValues: GlobalValue[];
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
    /**
     * The others who have the workflow open, and the last change someone else made to it (real time).
     */
    presence: Presence[];
    remoteChange: WorkflowTypeChangedMessage | null;
    /**
     * The journal record the instance viewer shows in the activity's Runs tab, after it was selected in the journal.
     */
    focusedRunSequence: number | null;
    /**
     * Whether the selected activity's panel was asked for: by a click on the activity (not a drag), a double-click,
     * Enter, or the issues and journal lists. Moving activities around doesn't open it, so it doesn't cover them.
     */
    activityPanelRequested: boolean;
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
    publishedVersion: null,
    version: null,
    settings: null,
    variables: [],
    variableTypes: [],
    globalValues: [],
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
    presence: [],
    remoteChange: null,
    focusedRunSequence: null,
    activityPanelRequested: false,
});

/**
 * The designer state: graph, selection, viewport, revision, save status and issues. A module-level
 * instance (`designerStore`) is shared by the components; tests create their own.
 */
export const createDesignerStore = () => {
    const state = reactive(createInitialState());
    const { history, markChanged, execute, record, undo, redo } = createGraphHistory(state);

    // The activities the draft (or live type) on the server has, so a save can tell which activities were
    // removed and which removed ones were brought back (for example by undo).
    const serverActivityIds = createServerNodeIds();

    const graph: Graph = {
        get nodes() {
            return state.nodes;
        },
        get transitions() {
            return state.transitions;
        },
        get edges() {
            return state.transitions;
        },
        model: createWorkflowModel(() => state.settings?.branchingMode === "All"),
    };

    // The server changed the draft on behalf of the signed-in user, who is now its last editor.
    const touchDraft = () => {
        state.hasDraft = true;
        state.draftModifiedBy = null;
        state.draftModifiedByUserId = state.currentUserId;
        state.draftModifiedUtc = new Date().toISOString();
    };

    /**
     * Records the revision and issues returned by a successful server change.
     */
    const applyServerChange = (revision: number, issues: DesignIssue[]) => {
        state.revision = revision;
        state.issues = issues;
        touchDraft();
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
                publishedVersion: definition.publishedVersion ?? null,
                version: definition.version ?? null,
                settings: definition.settings,
                variables: definition.variables ?? [],
                variableTypes: definition.variableTypes ?? [],
                globalValues: definition.globalValues ?? [],
                nodes: definition.nodes,
                transitions: definition.transitions,
                issues: definition.issues,
                selectedNodeIds: [],
                selectedTransitionKey: null,
                saveStatus: "saved" satisfies SaveStatus,
                remoteChange: null,
            });

            serverActivityIds.reset(definition.nodes.map((node) => node.id));
            history.clear();
        },

        getNode(id: string) {
            return state.nodes.find((node) => node.id === id);
        },

        /**
         * Applies a command and records it in the history.
         */
        execute,

        /**
         * Records a command whose effect is already applied, for example a drag.
         */
        record,

        undo,
        redo,

        /**
         * Records an activity that the server just created (AddActivity).
         */
        registerServerNode(node: DesignerNode, revision: number, issues: DesignIssue[]) {
            serverActivityIds.add(node.id);
            applyServerChange(revision, issues);
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

            const removed = danglingEdges(graph, node);

            for (const transition of removed) {
                state.transitions.splice(state.transitions.indexOf(transition), 1);
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
        applyServerChange,

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
                removedActivityIds: serverActivityIds.removed(currentIds),
                restoredActivityIds: serverActivityIds.restored(currentIds),
            };
        },

        /**
         * Records a successful save of `payload`.
         */
        markSaved(payload: SavePayload, result: SaveResult) {
            serverActivityIds.reset(payload.nodes.map((node) => node.id));
            state.revision = result.revision;
            state.issues = result.issues;
            touchDraft();
        },

        /**
         * Records a successful publish: the draft is gone, so the next change starts a new one from the live
         * type (at the revision the publish returned), and undo can't restore what the draft's trash held.
         */
        markPublished(issues: DesignIssue[], revision: number, version: DesignerVersion | null = null) {
            Object.assign(state, {
                publishedVersion: version ?? state.publishedVersion,
                revision,
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
            serverActivityIds.reset([]);
            history.clear();
        },
    };
};

export type DesignerStore = ReturnType<typeof createDesignerStore>;

export const designerStore = createDesignerStore();
