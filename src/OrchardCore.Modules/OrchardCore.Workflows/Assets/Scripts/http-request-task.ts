import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const editor = document.querySelector<HTMLElement>('[data-task-editor="http-request"]');
const headers = document.getElementById(editor?.dataset.headersId ?? "") as HTMLTextAreaElement | null;
const body = document.getElementById(editor?.dataset.bodyId ?? "") as HTMLTextAreaElement | null;

if (headers) {
    initLiquidPatternEditor(headers);
}

if (body) {
    initLiquidPatternEditor(body);
}
