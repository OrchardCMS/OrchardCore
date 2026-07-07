import initCodeMirrorQueryEditor from "@orchardcore/bloom/components/codemirror-query-editor";
import syncMonacoTheme from "@orchardcore/bloom/helpers/monacoTheme";

document.querySelectorAll<HTMLElement>(".codemirror-query-editor").forEach(initCodeMirrorQueryEditor);

// Monaco's own AMD/RequireJS loader, not a CommonJS import.
// eslint-disable-next-line @typescript-eslint/no-require-imports
require(["vs/editor/editor.main"], () => {
    syncMonacoTheme();

    const modelUri = monaco.Uri.parse("json://grid/settings.json");
    const editorContainer = document.getElementById("monaco_editor");
    const textArea = document.getElementById("monaco_textarea") as HTMLTextAreaElement | null;

    if (!editorContainer || !textArea) {
        return;
    }

    // Mirrors the original exactly: the model is first created from JSON.stringify(textArea, ...)
    // (a DOM element, not its value - stringifies to "{}") and immediately overwritten below via
    // setValue(textArea.value). A pre-existing no-op preserved rather than silently simplified.
    const jsonModel = monaco.editor.createModel(JSON.stringify(textArea, null, 4), "json", modelUri);

    const editor = monaco.editor.create(editorContainer, {
        automaticLayout: true,
        language: "json",
        lineNumbers: false,
        minimap: { enabled: false },
        model: jsonModel,
    });

    editor.getModel().setValue(textArea.value);

    setTimeout(() => {
        editor.getAction("editor.action.formatDocument")?.run();
    }, 300);

    window.addEventListener("submit", () => {
        textArea.value = editor.getValue();
    });
});
