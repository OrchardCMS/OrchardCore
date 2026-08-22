import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const textArea = document.getElementById("FullTextTemplate") as HTMLTextAreaElement | null;

if (textArea) {
    initLiquidPatternEditor(textArea);
}
