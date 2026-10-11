import { reactive } from "vue";
import type {
    DesignIssue,
    DesignerConnection,
    DesignerDefinition,
    DesignerNode,
    DesignerRun,
    DesignerVersion,
    PipelineSettings,
    SavePayload,
    SaveResult,
} from "../api/types";
import { History, type Command } from "./history";
import type { Graph } from "./commands";
import { connectionKey, danglingConnections } from "./ports";

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
    pipelineId: string;
    revision: number;
    hasDraft: boolean;
    draftModifiedBy: string | null;
    draftModifiedByUserId: string | null;
    draftModifiedUtc: string | null;
    /**
     * The signed-in user, so the draft's last editor becomes them when they change it.
     */
    currentUserId: string | null;
    settings: PipelineSettings | null;
    /**
     * The version runs use, and the version a version page shows.
     */
    publishedVersion: DesignerVersion | null;
    version: DesignerVersion | null;
    canRun: boolean;
    lastRun: DesignerRun | null;
    nodes: DesignerNode[];
    connections: DesignerConnection[];
    issues: DesignIssue[];
    selectedNodeIds: string[];
    selectedConnectionKey: string | null;
    viewport: Viewport;
    saveStatus: SaveStatus;
    /**
     * Increments on every graph change; autosave compares it with the version it last sent.
     */
    changeVersion: number;
    canUndo: boolean;
    canRedo: boolean;
    /**
     * Whether the selected step's panel was asked for: by a double-click, Enter, its Edit button, or the issues list.
     * Moving steps around doesn't open it, so it doesn't cover them.
     */
    stepPanelRequested: boolean;
    /**
     * The run whose step statuses and counts the canvas shows, selected in the Runs tab.
     */
    overlayRun: DesignerRun | null;
}

const createInitialState = (): DesignerState => ({
    loaded: false,
    pipelineId: "",
    revision: 0,
    hasDraft: false,
    draftModifiedBy: null,
    draftModifiedByUserId: null,
    draftModifiedUtc: null,
    currentUserId: null,
    settings: null,
    publishedVersion: null,
    version: null,
    canRun: false,
    lastRun: null,
    nodes: [],
    connections: [],
    issues: [],
    selectedNodeIds: [],
    selectedConnectionKey: null,
    viewport: { panX: 0, panY: 0, zoom: 1 },
    saveStatus: "saved",
    changeVersion: 0,
    canUndo: false,
    canRedo: false,
    stepPanelRequested: false,
    overlayRun: null,
});

/**
 * The designer state: graph, selection, viewport, revision, save status and issues. A module-level instance
 * (`designerStore`) is shared by the components; tests create their own.
 */
export const createDesignerStore = () => {
    const state = reactive(createInitialState());
    const history = new History();

    // The steps the draft on the server has, so a save can tell which steps were removed, and which removed ones
    // were brought back (by undo).
    let serverStepIds = new Set<string>();

    const graph: Graph = {
        get nodes() {
            return state.nodes;
        },
        get connections() {
            return state.connections;
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
                pipelineId: definition.pipelineId,
                revision: definition.revision,
                hasDraft: definition.hasDraft,
                draftModifiedBy: definition.draftModifiedBy ?? null,
                draftModifiedByUserId: definition.draftModifiedByUserId ?? null,
                draftModifiedUtc: definition.draftModifiedUtc ?? null,
                settings: definition.settings,
                publishedVersion: definition.publishedVersion ?? null,
                version: definition.version ?? null,
                canRun: definition.canRun,
                lastRun: definition.lastRun ?? null,
                nodes: definition.nodes,
                connections: definition.connections,
                issues: definition.issues,
                selectedNodeIds: [],
                selectedConnectionKey: null,
                saveStatus: "saved" satisfies SaveStatus,
            });

            serverStepIds = new Set(definition.nodes.map((node) => node.id));
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
         * Records a command whose effect is already applied.
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
         * Records a step that the server just created (AddStep), with the connection it made to it.
         */
        registerServerNode(node: DesignerNode, revision: number, issues: DesignIssue[]) {
            serverStepIds.add(node.id);
            state.revision = revision;
            state.issues = issues;
            touchDraft();
        },

        /**
         * Replaces a step with the server's version after an editor apply (keeping its position), and removes the
         * connections of the ports it no longer has (`removed` from the server, plus unsaved local ones). Its settings
         * changed on the server and can't be reverted locally, so the undo history is cleared. Returns the number of
         * removed connections.
         */
        replaceNode(node: DesignerNode, removed: DesignerConnection[] = []): number {
            const index = state.nodes.findIndex((existing) => existing.id === node.id);

            if (index >= 0) {
                state.nodes.splice(index, 1, { ...node, x: state.nodes[index].x, y: state.nodes[index].y });
            }

            const removedKeys = new Set(removed.map(connectionKey));
            const dangling = new Set(danglingConnections(node, state.connections));
            const before = state.connections.length;

            state.connections = state.connections.filter((connection) => !dangling.has(connection) && !removedKeys.has(connectionKey(connection)));

            const count = before - state.connections.length;

            if (count > 0) {
                markChanged();
            }

            history.markBoundary();

            return count;
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
                nodes: state.nodes.map((node) => ({ id: node.id, x: Math.round(node.x), y: Math.round(node.y) })),
                connections: state.connections.map((connection) => ({
                    sourceStepId: connection.sourceStepId,
                    sourcePort: connection.sourcePort,
                    targetStepId: connection.targetStepId,
                    targetPort: connection.targetPort,
                })),
                removedStepIds: [...serverStepIds].filter((id) => !currentIds.has(id)),
                restoredStepIds: [...currentIds].filter((id) => !serverStepIds.has(id)),
            };
        },

        /**
         * Records a successful save of `payload`.
         */
        markSaved(payload: SavePayload, result: SaveResult) {
            serverStepIds = new Set(payload.nodes.map((node) => node.id));
            state.revision = result.revision;
            state.issues = result.issues;
            touchDraft();
        },

        /**
         * Records a successful publish: the draft is gone, so the next change starts a new one from the published
         * pipeline (revision 0), and undo can't bring back what the draft removed.
         */
        markPublished(issues: DesignIssue[], version: DesignerVersion | null = null) {
            Object.assign(state, {
                publishedVersion: version ?? state.publishedVersion,
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
            serverStepIds = new Set();
            history.clear();
        },
    };
};

export type DesignerStore = ReturnType<typeof createDesignerStore>;

export const designerStore = createDesignerStore();
