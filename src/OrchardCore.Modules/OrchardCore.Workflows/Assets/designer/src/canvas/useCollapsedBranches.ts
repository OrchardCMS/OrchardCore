import { computed, ref, watch } from "vue";
import type { DesignerStore } from "../state/designerStore";
import { transitionKey } from "../state/commands";
import { readPreference, writePreference } from "../ui/preferences";
import { collapsedHiding, hiddenActivityIds, hiddenAfter } from "./branches";

// The collapsed activities are a view preference: they are remembered per browser and per workflow type,
// and never saved with the workflow. Hidden activities stay in the graph and are saved as usual.
export const collapsedPreferenceKey = (workflowTypeId: string) => `collapsed:${workflowTypeId}`;

const parse = (value: string | null): string[] => {
    try {
        const ids: unknown = JSON.parse(value ?? "[]");

        return Array.isArray(ids) ? ids.filter((id): id is string => typeof id === "string") : [];
    } catch {
        return [];
    }
};

export const useCollapsedBranches = (store: DesignerStore) => {
    const state = store.state;
    const collapsedIds = ref<string[]>([]);

    watch(
        () => state.workflowTypeId,
        (workflowTypeId) => {
            collapsedIds.value = workflowTypeId ? parse(readPreference(collapsedPreferenceKey(workflowTypeId))) : [];
        },
        { immediate: true },
    );

    const activityIds = computed(() => state.nodes.map((node) => node.id));
    const startIds = computed(() => state.nodes.filter((node) => node.isStart).map((node) => node.id));
    const collapsed = computed(() => new Set(collapsedIds.value.filter((id) => !!store.getNode(id))));
    const hidden = computed(() => hiddenActivityIds(activityIds.value, state.transitions, collapsed.value, startIds.value));

    // The number of activities each visible collapsed activity hides, nested collapsed ones included.
    const hiddenCounts = computed(
        () => new Map([...collapsed.value].filter((id) => !hidden.value.has(id)).map((id) => [id, hiddenAfter(id, state.transitions, hidden.value).size])),
    );

    const update = (ids: string[]) => {
        // Activities that no longer exist are dropped when the preference is written.
        collapsedIds.value = ids.filter((id) => !!store.getNode(id));

        if (state.workflowTypeId) {
            writePreference(collapsedPreferenceKey(state.workflowTypeId), JSON.stringify(collapsedIds.value));
        }
    };

    const hiddenIfCollapsed = (activityId: string) =>
        hiddenActivityIds(activityIds.value, state.transitions, [...collapsed.value, activityId], startIds.value);

    /**
     * Returns the number of activities that collapsing `activityId` would hide.
     */
    const countHiddenBy = (activityId: string) => (collapsed.value.has(activityId) ? 0 : hiddenIfCollapsed(activityId).size - hidden.value.size);

    /**
     * Hides the activities that come after `activityId`. Hidden activities are no longer selected.
     */
    const collapse = (activityId: string) => {
        const next = hiddenIfCollapsed(activityId);

        if (state.selectedNodeIds.some((id) => next.has(id))) {
            state.selectedNodeIds = state.selectedNodeIds.filter((id) => !next.has(id));
        }

        const transition = state.transitions.find((item) => transitionKey(item) === state.selectedTransitionKey);

        if (transition && (next.has(transition.sourceActivityId) || next.has(transition.destinationActivityId))) {
            state.selectedTransitionKey = null;
        }

        update([...collapsed.value, activityId]);
    };

    const expand = (activityId: string) => update([...collapsed.value].filter((id) => id !== activityId));

    const expandAll = () => update([]);

    /**
     * Expands the collapsed activities that hide `ids`. Returns whether anything was expanded.
     */
    const reveal = (ids: readonly string[]) => {
        const opening = collapsedHiding(ids, activityIds.value, state.transitions, [...collapsed.value], startIds.value);

        if (opening.length > 0) {
            update([...collapsed.value].filter((id) => !opening.includes(id)));
        }

        return opening.length > 0;
    };

    return { collapsed, hidden, hiddenCounts, countHiddenBy, collapse, expand, expandAll, reveal };
};
