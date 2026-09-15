const textArea = document.getElementById("Expression") as HTMLTextAreaElement | null;

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
