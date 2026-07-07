import syncMonacoTheme from "../helpers/monacoTheme";

// The "enter editor options as JSON" Monaco settings editors (HtmlField/TextField Monaco editor
// settings) are functionally identical, differing only in id/label text - one shared component
// reads its container/value elements and an optional JSON-schema URI off data-* attributes.
const initMonacoJsonSettingsEditor = (element: HTMLElement) => {
    const editorContainer = element.querySelector<HTMLElement>(".monaco-json-settings-editor-container");
    const textArea = element.querySelector<HTMLTextAreaElement>(".monaco-json-settings-editor-value");

    if (!editorContainer || !textArea) {
        return;
    }

    const schemaUri = element.dataset.schemaUri;

    // Monaco's own AMD/RequireJS loader, not a CommonJS import.
    // eslint-disable-next-line @typescript-eslint/no-require-imports
    require(["vs/editor/editor.main"], () => {
        syncMonacoTheme();

        if (schemaUri) {
            monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
                validate: true,
                enableSchemaRequest: true,
                allowComments: true,
                schemas: [{ uri: schemaUri, fileMatch: ["*"] }],
            });
        }

        const editor = monaco.editor.create(editorContainer, {
            automaticLayout: true,
            language: "json",
            lineNumbers: false,
            minimap: { enabled: false },
        });

        editor.getModel().setValue(textArea.value);

        window.addEventListener("submit", () => {
            textArea.value = editor.getValue();
        });
    });
};

export default initMonacoJsonSettingsEditor;
