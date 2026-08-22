import initLiquidPatternEditor from "./liquid-pattern-editor";

// Shared by LiquidPart.Edit.cshtml, FacebookPluginPart.Edit.cshtml and
// FacebookPluginPartSettings.Edit.cshtml - a liquid CodeMirror editor that saves back to the
// textarea and pings the content-preview iframe on every change. Widget-attachable (repeatable),
// so callers wire this up via observeAndInit.
const initContentPreviewLiquidEditor = (element: HTMLElement) => {
    const editor = initLiquidPatternEditor(element as HTMLTextAreaElement);

    editor.on("change", (cmEditor) => {
        cmEditor.save();
        document.dispatchEvent(new Event("contentpreview:render"));
    });
};

export default initContentPreviewLiquidEditor;
