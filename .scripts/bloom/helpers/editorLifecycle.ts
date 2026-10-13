// Helpers for editors that can be injected into a page after it has loaded, for example the activity
// editors that the workflow designer shows in its properties panel. Such a host replaces the editor
// markup without reloading the page, so the initializers must release what they created, and rich
// editors must keep their underlying <textarea> up to date for hosts that post the form with FormData.

// Dispatched (bubbling) on a container right before the host removes or replaces its content.
export const EDITOR_UNMOUNTING_EVENT = "oc:editor-unmounting";

// Notifies every editor inside `container` that it is about to be removed.
export const dispatchEditorUnmounting = (container: Element) => {
    container.dispatchEvent(new CustomEvent(EDITOR_UNMOUNTING_EVENT, { bubbles: true }));
};

// Calls `dispose` once, when a container that holds `element` dispatches EDITOR_UNMOUNTING_EVENT.
// Returns a function that stops listening without disposing.
export const onEditorUnmounting = (element: Element, dispose: () => void): (() => void) => {
    const listener = (event: Event) => {
        if (event.target instanceof Node && event.target.contains(element)) {
            document.removeEventListener(EDITOR_UNMOUNTING_EVENT, listener);
            dispose();
        }
    };

    document.addEventListener(EDITOR_UNMOUNTING_EVENT, listener);

    return () => document.removeEventListener(EDITOR_UNMOUNTING_EVENT, listener);
};

// Fires the same "change" event a native field fires when it loses focus after being edited, so hosts
// that react to form changes see edits made in a rich editor.
export const dispatchFieldChange = (field: HTMLElement) => {
    field.dispatchEvent(new Event("change", { bubbles: true }));
};

// Keeps the <textarea> behind a CodeMirror 5 instance in sync, reports edits as a "change" event on the
// textarea when the editor loses focus, and restores the textarea when the editor is unmounted.
export const bindCodeMirrorToTextArea = (editor: CodeMirrorEditor, textArea: HTMLTextAreaElement) => {
    let changed = false;

    editor.on("change", () => {
        editor.save();
        changed = true;
    });

    editor.on("blur", () => {
        if (changed) {
            changed = false;
            dispatchFieldChange(textArea);
        }
    });

    onEditorUnmounting(textArea, () => editor.toTextArea());

    return editor;
};
