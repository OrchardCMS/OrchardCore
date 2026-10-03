const initializeEditor = (textArea: HTMLTextAreaElement) =>
    CodeMirror.fromTextArea(textArea, {
        autoCloseTags: true,
        autoRefresh: true,
        lineNumbers: true,
        lineWrapping: true,
        matchBrackets: true,
        styleActiveLine: true,
        mode: { name: textArea.dataset.mode ?? "liquid" },
    });

const editorElement = document.querySelector<HTMLElement>('[data-task-editor="correlate"]');
const valueTextArea = document.getElementById(editorElement?.dataset.valueId ?? "") as HTMLTextAreaElement | null;
const syntaxSelect = document.getElementById(editorElement?.dataset.syntaxId ?? "") as HTMLSelectElement | null;

if (valueTextArea && syntaxSelect) {
    const editor = initializeEditor(valueTextArea);

    syntaxSelect.addEventListener("change", (e) => {
        const syntax = (e.target as HTMLSelectElement).value.toLowerCase();
        editor.setOption("mode", syntax);

        const liquidHint = document.getElementById("liquid_hint");
        const javascriptHint = document.getElementById("javascript_hint");

        if (syntax === "javascript") {
            liquidHint?.classList.add("d-none");
            javascriptHint?.classList.remove("d-none");
        } else {
            javascriptHint?.classList.add("d-none");
            liquidHint?.classList.remove("d-none");
        }
    });
}
