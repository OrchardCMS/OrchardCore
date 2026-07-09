import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

// Shared by CreateContentTask.Fields.Edit.cshtml and UpdateContentTask.Fields.Edit.cshtml -
// both bind a "ContentProperties" textarea, so the element id is the same in both views.
const textArea = document.getElementById("ContentProperties") as HTMLTextAreaElement | null;

if (textArea) {
    initLiquidPatternEditor(textArea);
}
