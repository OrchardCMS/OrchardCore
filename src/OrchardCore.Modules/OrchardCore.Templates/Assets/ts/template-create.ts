import syncMonacoTheme from "@orchardcore/bloom/helpers/monacoTheme";
import { initializeTemplatePreview } from "@orchardcore/bloom/components/template-preview-sync";

const editorElement = document.querySelector<HTMLElement>(".template-editor");
const textArea = document.querySelector<HTMLTextAreaElement>(".template-editor-value");

if (editorElement && textArea) {
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- Monaco's own AMD/RequireJS loader, not a CommonJS import.
    require(["vs/editor/editor.main"], () => {
        ConfigureLiquidIntellisense(monaco);
        syncMonacoTheme();

        const editor = monaco.editor.create(editorElement, {
            automaticLayout: true,
            language: "liquid",
        });

        editor.getModel().onDidChangeContent(() => {
            textArea.value = editor.getValue();
        });

        editor.getModel().setValue(textArea.value);

        window.addEventListener("submit", () => {
            textArea.value = editor.getValue();
        });
    });

    initializeTemplatePreview(textArea, "change");
}
