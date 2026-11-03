import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";
import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { bindCodeMirrorToTextArea } from "@orchardcore/bloom/helpers/editorLifecycle";

// The DisplayDriver prefixes generated ids, so the "Body" textarea is matched by its id suffix,
// within the [data-task-editor="sms"] wrapper (other editors have "...Body" fields too).
// 'observeAndInit' also covers editors injected after the page loaded (the workflow designer panel).
observeAndInit('[data-task-editor="sms"]', (editor) => {
    editor.querySelectorAll<HTMLTextAreaElement>("textarea[id$='Body']").forEach((textArea) => {
        bindCodeMirrorToTextArea(initLiquidPatternEditor(textArea), textArea);
    });
});
