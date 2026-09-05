import syncMonacoTheme from "../helpers/monacoTheme";
import { getDatasetJson } from "../helpers/dataset";
import waitForMonaco from "../helpers/monaco";

// A Monaco JSON editor backed by a named model + JSON-schema validation (monaco.editor.createModel
// + monaco.Uri.parse), as opposed to the simpler monaco.editor.create(el, options) overload used
// by monaco-json-settings-editor.ts. Shared by every "edit this JSON document against a schema"
// view (deployment JSON recipes, tenant feature profile rules) - each just supplies its own
// model/schema URIs and schema object via data-* attributes.
const initMonacoJsonModelEditor = (element: HTMLElement) => {
    const editorContainer = element.querySelector<HTMLElement>(".monaco-json-model-editor-container");
    const textArea = element.querySelector<HTMLTextAreaElement>(".monaco-json-model-editor-value");

    if (!editorContainer || !textArea) {
        return;
    }

    const modelUri = element.dataset.modelUri;
    const schemaUri = element.dataset.schemaUri;
    const schema = getDatasetJson<Record<string, unknown>>(element, "schema");

    if (!modelUri) {
        return;
    }

    waitForMonaco()
        .then((monaco) => {
            syncMonacoTheme(monaco);

            const uri = monaco.Uri.parse(modelUri);
            const model = monaco.editor.createModel(textArea.value, "json", uri);

            if (schema && schemaUri) {
                monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
                    validate: true,
                    schemas: [{ uri: schemaUri, fileMatch: [uri.toString()], schema }],
                });
            }

            const editor = monaco.editor.create(editorContainer, { model });

            window.addEventListener("submit", () => {
                textArea.value = editor.getValue();
            });
        })
        .catch((error: unknown) => console.error(error));
};

export default initMonacoJsonModelEditor;
