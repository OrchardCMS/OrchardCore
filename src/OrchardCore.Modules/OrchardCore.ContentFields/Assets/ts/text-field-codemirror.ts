import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

const initTextFieldCodeMirror = (element: HTMLElement) => {
    const textArea = element as HTMLTextAreaElement;
    const editor = CodeMirror.fromTextArea(textArea, {
        autoCloseTags: true,
        autoRefresh: true,
        lineNumbers: true,
        lineWrapping: true,
        matchBrackets: true,
        styleActiveLine: true,
        mode: { name: "htmlmixed" },
    });

    editor.on("change", (cmEditor) => {
        cmEditor.save();
        document.dispatchEvent(new Event("contentpreview:render"));
    });
};

observeAndInit(".text-field-codemirror", initTextFieldCodeMirror);
