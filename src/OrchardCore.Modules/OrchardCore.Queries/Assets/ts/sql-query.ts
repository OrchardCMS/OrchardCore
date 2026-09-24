import initCodeMirrorQueryEditor from "@orchardcore/bloom/components/codemirror-query-editor";

const element = document.querySelector<HTMLElement>(".codemirror-query-editor");

if (element) {
    initCodeMirrorQueryEditor(element);
}
