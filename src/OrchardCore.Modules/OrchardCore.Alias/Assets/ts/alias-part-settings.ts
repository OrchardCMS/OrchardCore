import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

// The DisplayDriver prefixes generated ids (e.g. "MyType_MyPart_MyDriver_Pattern"), so a
// hardcoded getElementById("Pattern") never matches - use an attribute selector against
// the field-name suffix instead, matching how @Html.IdFor(x => x.Pattern) resolved it
// before this script was extracted from an inline @Html.IdFor-based Razor block.
const textArea = document.querySelector<HTMLTextAreaElement>("textarea[id$='Pattern']");

if (textArea) {
    initLiquidPatternEditor(textArea);
}
