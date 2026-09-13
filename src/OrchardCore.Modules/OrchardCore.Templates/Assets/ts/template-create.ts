import syncMonacoTheme from "@orchardcore/bloom/helpers/monacoTheme";
import { initializeTemplatePreview } from "@orchardcore/bloom/components/template-preview-sync";
import waitForMonaco from "@orchardcore/bloom/helpers/monaco";

const editorElement = document.querySelector<HTMLElement>(".template-editor");
const textArea = document.querySelector<HTMLTextAreaElement>(".template-editor-value");

if (editorElement && textArea) {
    waitForMonaco()
        .then((monaco) => {
            ConfigureLiquidIntellisense(monaco);
            syncMonacoTheme(monaco);

            const editor = monaco.editor.create(editorElement, {
                automaticLayout: true,
                language: "liquid",
            });

            editor.getModel()?.onDidChangeContent(() => {
                textArea.value = editor.getValue();
            });

            editor.getModel()?.setValue(textArea.value);

            window.addEventListener("submit", () => {
                textArea.value = editor.getValue();
            });
        })
        .catch((error: unknown) => console.error(error));

    initializeTemplatePreview(textArea, "change");
}
