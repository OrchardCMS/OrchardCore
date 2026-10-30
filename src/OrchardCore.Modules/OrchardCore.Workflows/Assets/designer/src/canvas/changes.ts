// How an activity or a transition differs from the definition it is compared with (the compare page).
export type ChangeKind = "added" | "removed" | "changed" | "moved";

export interface CanvasChanges {
    // By activity id.
    nodes: Record<string, ChangeKind>;
    // By transition key ("source:outcome:destination").
    edges: Record<string, ChangeKind>;
}
