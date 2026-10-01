// A plain CodeMirror instance in "liquid" mode with no default-value seeding - shared by every
// *Settings.Edit.cshtml pattern/template editor (Alias, Autoroute, ContentPreview, Contents
// FullTextAspect, ContentPicker/TextField TitlePattern, etc). Distinct from
// codemirror-js-options-editor.ts (javascript mode, seeds a default value from data-*).
const initLiquidPatternEditor = (textArea: HTMLTextAreaElement): CodeMirrorEditor =>
    CodeMirror.fromTextArea(textArea, {
        autoRefresh: true,
        lineNumbers: true,
        styleActiveLine: true,
        matchBrackets: true,
        mode: { name: "liquid" },
    });

export default initLiquidPatternEditor;
