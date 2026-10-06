import type { DesignerTransition } from "../api/types";

// Collapsing an activity hides what comes after it: the activities that can only be reached through it.
// An activity that is also reached another way (a branch joining back, a loop to an earlier activity)
// stays visible, so a visible activity never has a connection from a hidden one, and the start
// activities are never hidden.

const successorsOf = (transitions: readonly DesignerTransition[]) => {
    const map = new Map<string, string[]>();

    for (const transition of transitions) {
        map.set(transition.sourceActivityId, [...(map.get(transition.sourceActivityId) ?? []), transition.destinationActivityId]);
    }

    return map;
};

const walk = (from: Iterable<string>, next: (id: string) => Iterable<string>, into = new Set<string>()) => {
    const queue = [...from];

    while (queue.length > 0) {
        const id = queue.pop()!;

        for (const successor of next(id)) {
            if (!into.has(successor)) {
                into.add(successor);
                queue.push(successor);
            }
        }
    }

    return into;
};

/**
 * Returns the ids of the activities hidden by collapsing `collapsedIds`.
 */
export const hiddenActivityIds = (
    activityIds: readonly string[],
    transitions: readonly DesignerTransition[],
    collapsedIds: Iterable<string>,
    startIds: Iterable<string> = [],
): Set<string> => {
    const ids = new Set(activityIds);
    const collapsed = new Set([...collapsedIds].filter((id) => ids.has(id)));

    if (collapsed.size === 0) {
        return new Set();
    }

    const successors = successorsOf(transitions);
    const next = (id: string) => (successors.get(id) ?? []).filter((successor) => ids.has(successor));

    // Everything after a collapsed activity may be hidden; everything else is visible.
    const candidates = new Set<string>();

    for (const id of collapsed) {
        for (const descendant of walk([id], next)) {
            if (descendant !== id) {
                candidates.add(descendant);
            }
        }
    }

    const visible = new Set<string>();
    const reveal = (seeds: Iterable<string>) => {
        const added = [...seeds].filter((id) => !visible.has(id));
        added.forEach((id) => visible.add(id));
        walk(added, (id) => (collapsed.has(id) ? [] : next(id)), visible);
    };

    reveal([...ids].filter((id) => !candidates.has(id)));
    reveal([...startIds].filter((id) => ids.has(id)));

    // Collapsed activities that only reach each other (a loop with no way in) would hide each other with
    // nothing left to expand them from: show the first one of them.
    for (;;) {
        const reachable = new Set<string>();
        const hiddenNext = (id: string) => next(id).filter((successor) => !visible.has(successor));

        for (const id of collapsed) {
            if (visible.has(id)) {
                walk([id], hiddenNext, reachable);
            }
        }

        const orphan = activityIds.find((id) => !visible.has(id) && !reachable.has(id));

        if (!orphan) {
            break;
        }

        reveal([orphan]);
    }

    return new Set(activityIds.filter((id) => !visible.has(id)));
};

/**
 * Returns the hidden activities that expanding `activityId` would show first: those reached from it
 * through hidden activities only.
 */
export const hiddenAfter = (activityId: string, transitions: readonly DesignerTransition[], hiddenIds: ReadonlySet<string>): Set<string> => {
    const successors = successorsOf(transitions);
    const next = (id: string) => (successors.get(id) ?? []).filter((successor) => hiddenIds.has(successor));

    return walk([activityId], next);
};

/**
 * Returns the collapsed activities to expand so that `activityIds` are shown, nested ones included.
 */
export const collapsedHiding = (
    activityIds: readonly string[],
    allIds: readonly string[],
    transitions: readonly DesignerTransition[],
    collapsedIds: readonly string[],
    startIds: Iterable<string> = [],
): string[] => {
    const remaining = [...collapsedIds];
    const expanded: string[] = [];
    const starts = [...startIds];

    for (;;) {
        const hidden = hiddenActivityIds(allIds, transitions, remaining, starts);
        const target = activityIds.filter((id) => hidden.has(id));

        if (target.length === 0) {
            return expanded;
        }

        // Expand the visible collapsed activities the targets are behind; nested ones become visible then.
        const opening = remaining.filter((id) => !hidden.has(id) && target.some((hiddenId) => hiddenAfter(id, transitions, hidden).has(hiddenId)));

        if (opening.length === 0) {
            return expanded;
        }

        expanded.push(...opening);
        opening.forEach((id) => remaining.splice(remaining.indexOf(id), 1));
    }
};
