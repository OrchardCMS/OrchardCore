// Inserts an expression where the cursor was in a server-rendered activity editor: a Monaco editor, a CodeMirror
// editor, an input or a text area. The editors' scripts aren't changed: Monaco lists its editors, and CodeMirror
// hangs on its element. A "change" event then tells the form host that the form changed.

interface MonacoEditorLike {
    getContainerDomNode(): HTMLElement;
    getSelection(): unknown;
    executeEdits(source: string, edits: { range: unknown; text: string; forceMoveMarkers?: boolean }[]): boolean;
    focus(): void;
}

interface MonacoWithEditors {
    editor: { getEditors(): MonacoEditorLike[] };
}

interface CodeMirrorLike {
    replaceSelection(text: string): void;
    focus(): void;
}

const TEXT_INPUT_TYPES = new Set(["", "text", "search", "url", "email", "tel"]);

const notifyChange = (element: Element) => element.dispatchEvent(new Event("change", { bubbles: true }));

const insertInMonaco = async (target: Element, text: string) => {
    const ready = window.__orchardCoreMonacoReady;

    if (!ready) {
        return false;
    }

    const monaco = (await ready) as Partial<MonacoWithEditors>;
    const editor = monaco.editor?.getEditors?.().find((candidate) => candidate.getContainerDomNode().contains(target));

    if (!editor) {
        return false;
    }

    editor.executeEdits("workflow-designer", [{ range: editor.getSelection(), text, forceMoveMarkers: true }]);
    editor.focus();
    notifyChange(editor.getContainerDomNode());

    return true;
};

/**
 * Whether text can be inserted in the field `target` belongs to: a code editor, or a text field that can be edited.
 */
export const canInsertAt = (target: Element | null | undefined): target is Element => {
    if (!target || !target.isConnected) {
        return false;
    }

    if (target.closest(".monaco-editor, .CodeMirror")) {
        return true;
    }

    const isTextField = target instanceof HTMLTextAreaElement || (target instanceof HTMLInputElement && TEXT_INPUT_TYPES.has(target.type));

    return isTextField && !target.readOnly && !target.disabled;
};

/**
 * Inserts `text` at the cursor of the field `target` belongs to. Resolves to false when it isn't a field the
 * text can be inserted in.
 */
export const insertAtCursor = async (target: Element | null | undefined, text: string): Promise<boolean> => {
    if (!target || !target.isConnected) {
        return false;
    }

    if (target.closest(".monaco-editor")) {
        return insertInMonaco(target, text);
    }

    const codeMirror = (target.closest(".CodeMirror") as (Element & { CodeMirror?: CodeMirrorLike }) | null)?.CodeMirror;

    if (codeMirror) {
        codeMirror.replaceSelection(text);
        codeMirror.focus();
        notifyChange(target.closest(".CodeMirror")!);

        return true;
    }

    const field =
        target instanceof HTMLTextAreaElement || (target instanceof HTMLInputElement && TEXT_INPUT_TYPES.has(target.type))
            ? target
            : null;

    if (!field || field.readOnly || field.disabled) {
        return false;
    }

    const start = field.selectionStart ?? field.value.length;
    const end = field.selectionEnd ?? start;

    field.setRangeText(text, start, end, "end");
    field.focus();
    field.dispatchEvent(new Event("input", { bubbles: true }));
    notifyChange(field);

    return true;
};
