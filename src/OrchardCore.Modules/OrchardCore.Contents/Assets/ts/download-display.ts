const textArea = document.getElementById("ContentItemJson") as HTMLTextAreaElement | null;

if (textArea) {
    CodeMirror.fromTextArea(textArea, {
        autoRefresh: true,
        lineNumbers: true,
        styleActiveLine: true,
        matchBrackets: true,
        mode: { name: "javascript" },
    });

    document.getElementById("json-copy-to-clipboard")?.addEventListener("click", () => {
        const temp = document.createElement("textarea");
        document.body.appendChild(temp);
        temp.value = textArea.textContent || "";
        temp.select();
        document.execCommand("copy");
        temp.remove();
    });
}

export {};
