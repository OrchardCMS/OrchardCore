import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import syncMonacoTheme from "@orchardcore/bloom/helpers/monacoTheme";
import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

const initHtmlFieldMonaco = (element: HTMLElement) => {
    const editorContainer = element.querySelector<HTMLElement>(".html-field-monaco-container");
    const textArea = element.querySelector<HTMLTextAreaElement>(".html-field-monaco-value");

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

observeAndInit(".html-field-monaco", initHtmlFieldMonaco);
