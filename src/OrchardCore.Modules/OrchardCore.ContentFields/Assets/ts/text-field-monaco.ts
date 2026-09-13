import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import syncMonacoTheme from "@orchardcore/bloom/helpers/monacoTheme";
import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";
import waitForMonaco from "@orchardcore/bloom/helpers/monaco";

const initTextFieldMonaco = (element: HTMLElement) => {
    const editorContainer = element.querySelector<HTMLElement>(".text-field-monaco-container");
    const textArea = element.querySelector<HTMLTextAreaElement>(".text-field-monaco-value");

    if (!editorContainer || !textArea) {
        return;
    }

    const options = getDatasetJson<Record<string, unknown>>(element, "options") ?? {};

    waitForMonaco()
        .then((monaco) => {
            if (options.automaticLayout === undefined) {
                options.automaticLayout = true;
            }

            syncMonacoTheme(monaco);

            const editor = monaco.editor.create(editorContainer, options);

            editor.getModel()?.setValue(textArea.value);

            window.addEventListener("submit", () => {
                textArea.value = editor.getValue();
            });
            editor.getModel()?.onDidChangeContent(() => {
                textArea.value = editor.getValue();
                document.dispatchEvent(new Event("contentpreview:render"));
            });
        })
        .catch((error: unknown) => console.error(error));
};

observeAndInit(".text-field-monaco", initTextFieldMonaco);
