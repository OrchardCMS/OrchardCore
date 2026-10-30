import type { DesignerTransition } from "../api/types";

// Collapsing an activity hides what comes after it: every activity reached through it, except those a start
// activity also reaches without going through it (a branch from the start joining back, a loop to an earlier
// activity). Activities that no start activity reaches, such as one left unconnected, don't keep anything
// visible. Start activities are never hidden.

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

    // What isn't after a collapsed activity is visible. Of what is, only the activities a start activity
    // reaches without going through a collapsed one stay visible.
    const visible = new Set([...ids].filter((id) => !candidates.has(id)));
    const reveal = (seeds: Iterable<string>) => {
        // Its own set: the walk goes on through activities that are already visible.
        const reached = new Set(seeds);
        walk(reached, (id) => (collapsed.has(id) ? [] : next(id)), reached);
        reached.forEach((id) => visible.add(id));
    };

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
