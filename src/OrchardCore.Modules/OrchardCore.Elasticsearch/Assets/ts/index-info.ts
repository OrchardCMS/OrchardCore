import initMonacoJsonModelEditor from "@orchardcore/bloom/components/monaco-json-model-editor";

const element = document.querySelector<HTMLElement>(".monaco-json-model-editor");

if (element) {
    initMonacoJsonModelEditor(element);
}
