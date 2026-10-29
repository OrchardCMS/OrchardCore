import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { bindCodeMirrorToTextArea } from "@orchardcore/bloom/helpers/editorLifecycle";

// Shared by NotifyContentOwnerTask.Fields.Edit.cshtml and NotifyUserTaskActivity.Fields.Edit.cshtml
// (same view model, both wrapped in [data-task-editor="notification"]).
const initializeEditor = (textArea: HTMLTextAreaElement | null) => {
    if (!textArea) {
        return;
    }

    bindCodeMirrorToTextArea(
        CodeMirror.fromTextArea(textArea, {
            autoCloseTags: true,
            autoRefresh: true,
            lineNumbers: true,
            lineWrapping: true,
            matchBrackets: true,
            styleActiveLine: true,
            mode: { name: "htmlmixed" },
        }),
        textArea,
    );
};

// The DisplayDriver prefixes generated ids, so the textareas are matched by their id suffix, within
// the wrapper. 'observeAndInit' also covers editors injected after the page loaded (the workflow
// designer panel).
observeAndInit('[data-task-editor="notification"]', (editor) => {
    initializeEditor(editor.querySelector<HTMLTextAreaElement>("textarea[id$='Summary']"));
    initializeEditor(editor.querySelector<HTMLTextAreaElement>("textarea[id$='HtmlBody']"));
});
