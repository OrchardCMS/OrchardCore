import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

// Shared by CreateContentTask.Fields.Edit.cshtml and UpdateContentTask.Fields.Edit.cshtml -
// both bind a "ContentProperties" textarea. The DisplayDriver prefixes generated ids
// (e.g. "IActivity_ContentProperties" - ActivityDisplayDriver's default prefix is its
// TModel type name, IActivity, not the concrete activity type), so a hardcoded
// getElementById("ContentProperties") never matches - use an attribute selector against
// the field-name suffix instead.
const textArea = document.querySelector<HTMLTextAreaElement>("textarea[id$='ContentProperties']");

if (textArea) {
    initLiquidPatternEditor(textArea);
}
