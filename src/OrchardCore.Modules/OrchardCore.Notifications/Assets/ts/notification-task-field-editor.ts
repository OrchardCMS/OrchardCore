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

// The DisplayDriver prefixes generated ids, so hardcoded getElementById("Summary")/
// getElementById("HtmlBody") never match - use attribute selectors against the
// field-name suffix instead.
initializeEditor(document.querySelector<HTMLTextAreaElement>("textarea[id$='Summary']"));
initializeEditor(document.querySelector<HTMLTextAreaElement>("textarea[id$='HtmlBody']"));
