// Shared by the "enter the editor options as a JS object literal" CodeMirror settings editors
// (Trumbowyg field/part settings, Markdown Wysiwyg field/part settings) - all four wrap a
// textarea in CodeMirror and, only if the textarea starts out empty, seed it with a literal
// default-options text computed server-side and passed through as data-default-options.
const initCodeMirrorJsOptionsEditor = (root: HTMLElement) => {
    const optionsTextArea = root.querySelector<HTMLTextAreaElement>("textarea");

    if (!optionsTextArea) {
        return;
    }

    const editor = CodeMirror.fromTextArea(optionsTextArea, {
        autoRefresh: true,
        lineNumbers: true,
        lineWrapping: true,
        matchBrackets: true,
        mode: { name: "javascript" },
    });

    if (!optionsTextArea.value && root.dataset.defaultOptions) {
        editor.setValue(root.dataset.defaultOptions);
    }
};

export default initCodeMirrorJsOptionsEditor;
