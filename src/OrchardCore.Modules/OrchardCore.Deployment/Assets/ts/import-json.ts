const textArea = document.getElementById("Json") as HTMLTextAreaElement | null;

if (textArea) {
    CodeMirror.fromTextArea(textArea, {
        autoRefresh: true,
        lineNumbers: true,
        styleActiveLine: true,
        matchBrackets: true,
        mode: { name: "javascript" },
    });
}

export {};
