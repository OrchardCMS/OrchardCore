import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";
import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { bindCodeMirrorToTextArea } from "@orchardcore/bloom/helpers/editorLifecycle";

// Initializes every HttpResponseTask editor, including editors injected after the page loaded (the
// workflow designer panel).
const initHttpResponseTaskEditor = (element: HTMLElement) => {
    for (const id of [element.dataset.headersId, element.dataset.contentId]) {
        const textArea = element.querySelector<HTMLTextAreaElement>(`#${CSS.escape(id ?? "")}`);

        if (textArea) {
            bindCodeMirrorToTextArea(initLiquidPatternEditor(textArea), textArea);
        }
    }
};

observeAndInit('[data-task-editor="http-response"]', initHttpResponseTaskEditor);
