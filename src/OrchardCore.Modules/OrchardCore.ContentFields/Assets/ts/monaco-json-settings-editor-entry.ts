import initMonacoJsonSettingsEditor from "@orchardcore/bloom/components/monaco-json-settings-editor";

const element = document.querySelector<HTMLElement>(".monaco-json-settings-editor");

if (element) {
    initMonacoJsonSettingsEditor(element);
}
