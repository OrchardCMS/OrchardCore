import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";
import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { bindCodeMirrorToTextArea } from "@orchardcore/bloom/helpers/editorLifecycle";

// Initializes every HttpRequestTask editor, including editors injected after the page loaded (the
// workflow designer panel).
const initHttpRequestTaskEditor = (element: HTMLElement) => {
    for (const id of [element.dataset.headersId, element.dataset.bodyId]) {
        const textArea = element.querySelector<HTMLTextAreaElement>(`#${CSS.escape(id ?? "")}`);

        if (textArea) {
            bindCodeMirrorToTextArea(initLiquidPatternEditor(textArea), textArea);
        }
    }
};

observeAndInit('[data-task-editor="http-request"]', initHttpRequestTaskEditor);
