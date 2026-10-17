import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";
import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { bindCodeMirrorToTextArea } from "@orchardcore/bloom/helpers/editorLifecycle";

// Initializes every EmailTask editor ([data-task-editor="email"]), including editors injected after the
// page loaded (the workflow designer panel). The DisplayDriver prefixes generated ids, so the textareas
// are matched by their id suffix; every lookup is scoped to the editor so other fields on the page are
// left alone.
const initEmailTaskEditor = (editor: HTMLElement) => {
    editor.querySelectorAll<HTMLTextAreaElement>("textarea[id$='HtmlBody'], textarea[id$='TextBody']").forEach((textArea) => {
        bindCodeMirrorToTextArea(initLiquidPatternEditor(textArea), textArea);
    });

    const select = editor.querySelector<HTMLSelectElement>("select[data-all-value]");

    if (!select) {
        return;
    }

    const textBodyDiv = editor.querySelector<HTMLElement>("#textBodyDiv");
    const htmlBodyDiv = editor.querySelector<HTMLElement>("#htmlBodyDiv");

    const changeBodyFormat = () => {
        const value = select.value;

        textBodyDiv?.classList.toggle("d-none", value === select.dataset.htmlValue);
        htmlBodyDiv?.classList.toggle("d-none", value === select.dataset.textValue);
    };

    select.addEventListener("change", changeBodyFormat);
    changeBodyFormat();
};

observeAndInit('[data-task-editor="email"]', initEmailTaskEditor);
