import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";
import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { bindCodeMirrorToTextArea } from "@orchardcore/bloom/helpers/editorLifecycle";

// Shared by CreateContentTask.Fields.Edit.cshtml and UpdateContentTask.Fields.Edit.cshtml -
// both bind a "ContentProperties" textarea inside a [data-task-editor="content"] wrapper. The
// DisplayDriver prefixes generated ids, so the textarea is matched by its id suffix, within the
// wrapper. 'observeAndInit' also covers editors injected after the page loaded (the workflow
// designer panel).
observeAndInit('[data-task-editor="content"]', (editor) => {
    editor.querySelectorAll<HTMLTextAreaElement>("textarea[id$='ContentProperties']").forEach((textArea) => {
        bindCodeMirrorToTextArea(initLiquidPatternEditor(textArea), textArea);
    });
});
