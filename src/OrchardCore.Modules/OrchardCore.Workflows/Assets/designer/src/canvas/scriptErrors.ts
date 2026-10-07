import type { DesignerInstance } from "../api/types";

/**
 * The script errors of the activities an instance ran without faulting, by activity, from its journal: a record that
 * completed or halted with an error is an activity whose script failed and used its fallback value.
 */
export const scriptErrorsByActivity = (instance: DesignerInstance | null | undefined): Map<string, string[]> => {
    const errors = new Map<string, string[]>();

    for (const record of instance?.journal ?? []) {
        if (record.status === "Faulted" || !record.error) {
            continue;
        }

        const messages = errors.get(record.activityId) ?? [];

        if (!messages.includes(record.error)) {
            messages.push(record.error);
        }

        errors.set(record.activityId, messages);
    }

    return errors;
};
