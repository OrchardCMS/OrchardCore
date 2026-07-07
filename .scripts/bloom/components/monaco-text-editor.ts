import syncMonacoTheme from "../helpers/monacoTheme";

// A plain Monaco source editor with no shortcodes action and no live-preview hook - just create,
// seed from the hidden textarea, and serialize back on submit. Shared by workflow task/event
// editors (ScriptTask, WorkflowFaultEvent) that are otherwise identical apart from language.
const initMonacoTextEditor = (element: HTMLElement) => {
    const editorContainer = element.querySelector<HTMLElement>(".monaco-text-editor-container");
    const textArea = element.querySelector<HTMLTextAreaElement>(".monaco-text-editor-value");

    if (!editorContainer || !textArea) {
        return;
    }

    const language = element.dataset.language ?? "javascript";

    // Monaco's own AMD/RequireJS loader, not a CommonJS import.
    // eslint-disable-next-line @typescript-eslint/no-require-imports
    require(["vs/editor/editor.main"], () => {
        syncMonacoTheme();

        const editor = monaco.editor.create(editorContainer, { automaticLayout: true, language });

        editor.getModel().setValue(textArea.value);

        window.addEventListener("submit", () => {
            textArea.value = editor.getValue();
        });
    });
};

export default initMonacoTextEditor;
