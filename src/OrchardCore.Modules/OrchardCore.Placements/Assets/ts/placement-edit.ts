const editorTextArea = document.querySelector<HTMLTextAreaElement>(".placement-nodes-editor");

if (editorTextArea) {
    CodeMirror.fromTextArea(editorTextArea, {
        autoRefresh: true,
        lineNumbers: true,
        lineWrapping: true,
        matchBrackets: true,
        styleActiveLine: true,
        mode: { name: "javascript" },
    });
}
