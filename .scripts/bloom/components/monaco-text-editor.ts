import syncMonacoTheme from "../helpers/monacoTheme";
import waitForMonaco from "../helpers/monaco";
import { dispatchFieldChange, onEditorUnmounting } from "../helpers/editorLifecycle";

// A plain Monaco source editor with no shortcodes action and no live-preview hook - just create,
// seed from the hidden textarea, and keep the textarea in sync. Shared by workflow task/event
// editors (ScriptTask, WorkflowFaultEvent) that are otherwise identical apart from language.
// It is safe to inject: the textarea follows every edit (hosts can post the form with FormData),
// edits are reported as a "change" event when the editor loses focus, and the editor is disposed
// when its container dispatches "oc:editor-unmounting".
const initMonacoTextEditor = (element: HTMLElement) => {
    const editorContainer = element.querySelector<HTMLElement>(".monaco-text-editor-container");
    const textArea = element.querySelector<HTMLTextAreaElement>(".monaco-text-editor-value");

    if (!editorContainer || !textArea) {
        return;
    }

    const language = element.dataset.language ?? "javascript";

    waitForMonaco()
        .then((monaco) => {
            if (!element.isConnected) {
                return;
            }

            syncMonacoTheme(monaco);

            const editor = monaco.editor.create(editorContainer, { automaticLayout: true, language });
            let changed = false;

            editor.getModel()?.setValue(textArea.value);

            editor.onDidChangeModelContent(() => {
                textArea.value = editor.getValue();
                changed = true;
            });

            editor.onDidBlurEditorText(() => {
                if (changed) {
                    changed = false;
                    dispatchFieldChange(textArea);
                }
            });

            const onSubmit = () => {
                textArea.value = editor.getValue();
            };

            window.addEventListener("submit", onSubmit);

            onEditorUnmounting(element, () => {
                window.removeEventListener("submit", onSubmit);

                const model = editor.getModel();
                editor.dispose();

                if (model && !model.isDisposed()) {
                    model.dispose();
                }
            });
        })
        .catch((error: unknown) => console.error(error));
};

export default initMonacoTextEditor;
