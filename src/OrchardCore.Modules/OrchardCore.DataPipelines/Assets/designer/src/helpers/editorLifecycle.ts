// A copy of the event part of .scripts/bloom/helpers/editorLifecycle.ts (added by the Workflows designer), so editors
// that listen with `onEditorUnmounting` release what they created when the designer replaces its form.
// TODO: import it from @bloom/helpers/editorLifecycle once that helper is on main.

// Dispatched (bubbling) on a container right before the host removes or replaces its content.
export const EDITOR_UNMOUNTING_EVENT = "oc:editor-unmounting";

// Notifies every editor inside `container` that it is about to be removed.
export const dispatchEditorUnmounting = (container: Element) => {
    container.dispatchEvent(new CustomEvent(EDITOR_UNMOUNTING_EVENT, { bubbles: true }));
};
