import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const headers = document.getElementById("Headers") as HTMLTextAreaElement | null;
const body = document.getElementById("Body") as HTMLTextAreaElement | null;

if (headers) {
    initLiquidPatternEditor(headers);
}

if (body) {
    initLiquidPatternEditor(body);
}
