import type { Library, LibraryCategory } from "../api/types";

export type ActivityKind = "all" | "events" | "tasks";

/**
 * The MIME type of the drag data that carries an activity name from the toolbox to the canvas.
 */
export const ACTIVITY_DRAG_TYPE = "application/x-orchard-workflow-activity";

const normalize = (value: string) =>
    value
        .normalize("NFD")
        .replace(/[̀-ͯ]/g, "")
        .toLowerCase()
        .trim();

/**
 * Filters the toolbox: `query` matches the display text or the category (case and accent insensitive),
 * `kind` keeps only events or tasks. Empty categories are dropped.
 */
export const filterLibrary = (library: Library | null, query: string, kind: ActivityKind): LibraryCategory[] => {
    if (!library) {
        return [];
    }

    const terms = normalize(query).split(/\s+/).filter(Boolean);

    return library.categories
        .map((category) => {
            const categoryText = normalize(category.name);

            return {
                ...category,
                activities: category.activities.filter((activity) => {
                    if ((kind === "events" && !activity.isEvent) || (kind === "tasks" && activity.isEvent)) {
                        return false;
                    }

                    const text = `${normalize(activity.displayText)} ${categoryText}`;

                    return terms.every((term) => text.includes(term));
                }),
            };
        })
        .filter((category) => category.activities.length > 0);
};
