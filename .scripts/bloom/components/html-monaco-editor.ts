import syncMonacoTheme from "../helpers/monacoTheme";
import { getDatasetJson } from "../helpers/dataset";

// The "Monaco-based HTML source editor with an Insert Shortcode action" pattern is shared by
// every HTML field/part editor that offers Monaco instead of CodeMirror/Trumbowyg/Wysiwyg
// (identical between HtmlField-Monaco and HtmlBodyPart-Monaco) - reads its container/value
// elements and its per-view Monaco options (as JSON) off data-* attributes.
const initHtmlMonacoEditor = (element: HTMLElement) => {
    const editorContainer = element.querySelector<HTMLElement>(".html-monaco-editor-container");
    const textArea = element.querySelector<HTMLTextAreaElement>(".html-monaco-editor-value");

    if (!editorContainer || !textArea) {
        return;
    }

    const options = getDatasetJson<Record<string, unknown>>(element, "options") ?? {};

    // Monaco's own AMD/RequireJS loader, not a CommonJS import.
    // eslint-disable-next-line @typescript-eslint/no-require-imports
    require(["vs/editor/editor.main"], () => {
        if (options.automaticLayout === undefined) {
            options.automaticLayout = true;
        }

        options.language = "html";

        syncMonacoTheme();

        const editor = monaco.editor.create(editorContainer, options);

        editor.getModel().onDidChangeContent(() => {
            textArea.value = editor.getValue();
            document.dispatchEvent(new Event("contentpreview:render"));
        });

        editor.addAction({
            id: "shortcodes",
            label: "Add Shortcode",
            run: () => {
                shortcodesApp.init((value) => {
                    if (value) {
                        const selection = editor.getSelection();

                        editor.executeEdits("shortcodes", [{ range: selection, text: value, forceMoveMarkers: true }]);
                    }

                    editor.focus();
                });
            },
            contextMenuGroupId: "orchardcore",
            contextMenuOrder: 0,
            keybindings: [monaco.KeyMod.Alt | monaco.KeyCode.KeyS],
        });

        editor.getModel().setValue(textArea.value);

        window.addEventListener("submit", () => {
            textArea.value = editor.getValue();
        });
    });
};

export default initHtmlMonacoEditor;
