import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const headers = document.getElementById("Headers") as HTMLTextAreaElement | null;
const content = document.getElementById("Content") as HTMLTextAreaElement | null;

if (headers) {
    initLiquidPatternEditor(headers);
}

if (content) {
    initLiquidPatternEditor(content);
}
