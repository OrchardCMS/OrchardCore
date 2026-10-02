import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { bindCodeMirrorToTextArea } from "@orchardcore/bloom/helpers/editorLifecycle";

// Initializes every CorrelateTask editor, including editors injected after the page loaded (the
// workflow designer panel). Lookups are scoped to the editor so several editors can coexist.
const initCorrelateTaskEditor = (element: HTMLElement) => {
    const valueTextArea = element.querySelector<HTMLTextAreaElement>(`#${CSS.escape(element.dataset.valueId ?? "")}`);
    const syntaxSelect = element.querySelector<HTMLSelectElement>(`#${CSS.escape(element.dataset.syntaxId ?? "")}`);

    if (!valueTextArea || !syntaxSelect) {
        return;
    }

    const editor = bindCodeMirrorToTextArea(
        CodeMirror.fromTextArea(valueTextArea, {
            autoCloseTags: true,
            autoRefresh: true,
            lineNumbers: true,
            lineWrapping: true,
            matchBrackets: true,
            styleActiveLine: true,
            mode: { name: valueTextArea.dataset.mode ?? "liquid" },
        }),
        valueTextArea,
    );

    syntaxSelect.addEventListener("change", () => {
        const syntax = syntaxSelect.value.toLowerCase();
        editor.setOption("mode", syntax);

        element.querySelector("#liquid_hint")?.classList.toggle("d-none", syntax === "javascript");
        element.querySelector("#javascript_hint")?.classList.toggle("d-none", syntax !== "javascript");
    });
};

observeAndInit('[data-task-editor="correlate"]', initCorrelateTaskEditor);
