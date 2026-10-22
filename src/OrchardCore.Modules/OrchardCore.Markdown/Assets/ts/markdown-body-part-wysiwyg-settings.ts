import initCodeMirrorJsOptionsEditor from "@orchardcore/bloom/components/codemirror-js-options-editor";

const element = document.querySelector<HTMLElement>(".codemirror-js-options-editor");

if (element) {
    initCodeMirrorJsOptionsEditor(element);
}
