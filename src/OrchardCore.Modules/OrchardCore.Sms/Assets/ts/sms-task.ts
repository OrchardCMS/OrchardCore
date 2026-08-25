import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

// The DisplayDriver prefixes generated ids, so a hardcoded getElementById("Body")
// never matches - use an attribute selector against the field-name suffix instead.
const textArea = document.querySelector<HTMLTextAreaElement>("textarea[id$='Body']");

if (textArea) {
    initLiquidPatternEditor(textArea);
}
