import syncMonacoTheme from "@orchardcore/bloom/helpers/monacoTheme";
import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

const editorContainer = document.querySelector<HTMLElement>(".json-recipe-editor");
const textArea = document.querySelector<HTMLTextAreaElement>(".json-recipe-editor-value");

if (editorContainer && textArea) {
    const schema = getDatasetJson<Record<string, unknown>>(editorContainer, "schema");

    // Monaco's own AMD/RequireJS loader, not a CommonJS import.
    // eslint-disable-next-line @typescript-eslint/no-require-imports
    require(["vs/editor/editor.main"], () => {
        syncMonacoTheme();

        const modelUri = monaco.Uri.parse("x://orchardcore.deployments.steps.jsonrecipe.json");
        const model = monaco.editor.createModel(textArea.value, "json", modelUri);

        if (schema) {
            monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
                validate: true,
                schemas: [
                    {
                        uri: "x://orchardcore.deployments.steps.jsonrecipe.schema.json",
                        fileMatch: [modelUri.toString()],
                        schema,
                    },
                ],
            });
        }

        const editor = monaco.editor.create(editorContainer, { model });

        window.addEventListener("submit", () => {
            textArea.value = editor.getValue();
        });
    });
}
