import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { bindCodeMirrorToTextArea } from "@orchardcore/bloom/helpers/editorLifecycle";

// Initializes every LiquidTask editor, including editors injected after the page loaded (the
// workflow designer panel).
const initLiquidTaskEditor = (element: HTMLElement) => {
    const textArea = element.querySelector<HTMLTextAreaElement>(`#${CSS.escape(element.dataset.expressionId ?? "")}`);

    if (!textArea) {
        return;
    }

    bindCodeMirrorToTextArea(
        CodeMirror.fromTextArea(textArea, {
            autoRefresh: true,
            lineNumbers: true,
            lineWrapping: true,
            matchBrackets: true,
            styleActiveLine: true,
            mode: { name: "liquid" },
        }),
        textArea,
    );
};

observeAndInit('[data-task-editor="liquid"]', initLiquidTaskEditor);
