import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

const initJavascriptCondition = (element: HTMLElement) => {
    const textArea = element as HTMLTextAreaElement;

    CodeMirror.fromTextArea(textArea, {
        autoRefresh: true,
        lineNumbers: true,
        styleActiveLine: true,
        matchBrackets: true,
        mode: { name: "javascript" },
    });
};

observeAndInit(".javascript-condition-editor", initJavascriptCondition);
