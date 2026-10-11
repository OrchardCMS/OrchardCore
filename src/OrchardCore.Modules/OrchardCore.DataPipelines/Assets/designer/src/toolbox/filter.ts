import type { Library, LibraryCategory, LibraryStep, PortKind, StepCategory } from "../api/types";
import { acceptsKind } from "../state/ports";

/**
 * The MIME type of the drag data that carries a step type name from the toolbox to the canvas.
 */
export const STEP_DRAG_TYPE = "application/x-orchard-data-pipeline-step";

/**
 * The order of the toolbox groups: where the rows come from, what changes them, the files made of them, where they go.
 */
export const CATEGORY_ORDER: StepCategory[] = ["Source", "Transform", "File", "Destination"];

/**
 * The default icon of each category, for a step type without one.
 */
export const CATEGORY_ICONS: Record<StepCategory, string> = {
    Source: "fa-solid fa-database",
    Transform: "fa-solid fa-shuffle",
    File: "fa-solid fa-file-lines",
    Destination: "fa-solid fa-paper-plane",
};

export const stepIcon = (step: { icon?: string | null; category: StepCategory }) => step.icon || CATEGORY_ICONS[step.category] || "fa-solid fa-cube";

const normalize = (value: string) =>
    value
        .normalize("NFD")
        .replace(/[̀-ͯ]/g, "")
        .toLowerCase()
        .trim();

const categoryRank = (category: StepCategory) => {
    const index = CATEGORY_ORDER.indexOf(category);

    return index < 0 ? CATEGORY_ORDER.length : index;
};

/**
 * Filters the toolbox: every term of `query` matches the display text, the description or the category (case and accent
 * insensitive). The categories come in CATEGORY_ORDER, and empty ones are dropped.
 */
export const filterLibrary = (library: Library | null, query: string): LibraryCategory[] => {
    if (!library) {
        return [];
    }

    const terms = normalize(query).split(/\s+/).filter(Boolean);

    return [...library.categories]
        .sort((a, b) => categoryRank(a.category) - categoryRank(b.category))
        .map((category) => {
            const categoryText = `${normalize(category.displayName)} ${normalize(category.category)}`;

            return {
                ...category,
                steps: category.steps.filter((step) => {
                    const text = `${normalize(step.displayText)} ${normalize(step.description ?? "")} ${categoryText}`;

                    return terms.every((term) => text.includes(term));
                }),
            };
        })
        .filter((category) => category.steps.length > 0);
};

/**
 * The steps that can come after an output of `kind` (their first input receives it), matching `query`, in toolbox order.
 */
export const compatibleSteps = (library: Library | null, kind: PortKind, query: string): { step: LibraryStep; category: LibraryCategory }[] =>
    filterLibrary(library, query).flatMap((category) => category.steps.filter((step) => acceptsKind(step, kind)).map((step) => ({ step, category })));

/**
 * The steps an empty pipeline starts with: its sources.
 */
export const sourceSteps = (library: Library | null): LibraryStep[] =>
    (library?.categories ?? []).flatMap((category) => category.steps).filter((step) => step.category === "Source");
