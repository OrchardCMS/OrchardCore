import type * as Monaco from "monaco-editor";
import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import waitForMonaco from "@orchardcore/bloom/helpers/monaco";
import syncMonacoTheme from "@orchardcore/bloom/helpers/monacoTheme";
import { dispatchFieldChange, onEditorUnmounting } from "@orchardcore/bloom/helpers/editorLifecycle";

// The WorkflowExpressionEditor shape: a syntax select and the expression, in a one-line input or a Monaco editor
// whose language follows the syntax. It is safe to inject (the workflow designer loads activity editors into
// its panel): the form fields follow every edit, edits are reported as "change" events, and the editor is
// disposed when its container dispatches "oc:editor-unmounting".

// The "monaco" resource may load after this script, so wait for it to announce itself.
const whenMonacoReady = (timeout = 15000): Promise<typeof Monaco> =>
    new Promise((resolve, reject) => {
        const started = Date.now();

        const check = () => {
            if (window.__orchardCoreMonacoReady) {
                waitForMonaco().then(resolve, reject);
            } else if (Date.now() - started > timeout) {
                reject(new Error("Monaco isn't loaded on this page."));
            } else {
                setTimeout(check, 50);
            }
        };

        check();
    });

const initExpressionEditor = (element: HTMLElement) => {
    const select = element.querySelector<HTMLSelectElement>("[data-expression-syntax]");
    const field = element.querySelector<HTMLInputElement | HTMLTextAreaElement>("[data-expression-value]");

    if (!select || !field) {
        return;
    }

    const option = () => select.selectedOptions[0];
    const language = () => option()?.dataset.language || "plaintext";

    const showExample = () => {
        if (field instanceof HTMLInputElement) {
            field.placeholder = option()?.dataset.placeholder ?? "";
        }
    };

    select.addEventListener("change", showExample);

    const host = element.querySelector<HTMLElement>("[data-expression-monaco]");

    if (!host) {
        onEditorUnmounting(element, () => select.removeEventListener("change", showExample));

        return;
    }

    whenMonacoReady()
        .then((monaco) => {
            if (!element.isConnected) {
                return;
            }

            syncMonacoTheme(monaco);

            const editor = monaco.editor.create(host, {
                value: field.value,
                language: language(),
                automaticLayout: true,
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                wordWrap: "on",
            });
            let changed = false;

            editor.onDidChangeModelContent(() => {
                field.value = editor.getValue();
                changed = true;
            });

            editor.onDidBlurEditorText(() => {
                if (changed) {
                    changed = false;
                    dispatchFieldChange(field);
                }
            });

            const changeLanguage = () => {
                const model = editor.getModel();

                if (model) {
                    monaco.editor.setModelLanguage(model, language());
                }
            };

            select.addEventListener("change", changeLanguage);

            onEditorUnmounting(element, () => {
                select.removeEventListener("change", showExample);
                select.removeEventListener("change", changeLanguage);

                const model = editor.getModel();
                editor.dispose();

                if (model && !model.isDisposed()) {
                    model.dispose();
                }
            });
        })
        .catch((error: unknown) => console.error(error));
};

observeAndInit("[data-workflow-expression]", initExpressionEditor);
