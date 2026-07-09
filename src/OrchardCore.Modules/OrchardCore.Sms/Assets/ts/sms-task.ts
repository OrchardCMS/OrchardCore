import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const textArea = document.getElementById("Body") as HTMLTextAreaElement | null;

if (textArea) {
    initLiquidPatternEditor(textArea);
}
