const editor = document.querySelector<HTMLElement>('[data-task-editor="liquid"]');
const textArea = document.getElementById(editor?.dataset.expressionId ?? "") as HTMLTextAreaElement | null;

if (textArea) {
    CodeMirror.fromTextArea(textArea, {
        autoRefresh: true,
        lineNumbers: true,
        lineWrapping: true,
        matchBrackets: true,
        styleActiveLine: true,
        mode: { name: "liquid" },
    });
}

export {};
