// Shared by NotifyContentOwnerTask.Fields.Edit.cshtml and NotifyUserTaskActivity.Fields.Edit.cshtml
// (byte-identical scripts, same view model, referenced from both views).
const initializeEditor = (textArea: HTMLTextAreaElement | null) => {
    if (!textArea) {
        return;
    }

    CodeMirror.fromTextArea(textArea, {
        autoCloseTags: true,
        autoRefresh: true,
        lineNumbers: true,
        lineWrapping: true,
        matchBrackets: true,
        styleActiveLine: true,
        mode: { name: "htmlmixed" },
    });
};

initializeEditor(document.getElementById("Summary") as HTMLTextAreaElement | null);
initializeEditor(document.getElementById("HtmlBody") as HTMLTextAreaElement | null);
