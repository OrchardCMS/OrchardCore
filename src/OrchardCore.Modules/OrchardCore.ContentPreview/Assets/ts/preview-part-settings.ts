import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const textArea = document.getElementById("Pattern") as HTMLTextAreaElement | null;

if (textArea) {
    initLiquidPatternEditor(textArea);
}
