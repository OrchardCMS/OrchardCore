import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const editor = document.querySelector<HTMLElement>('[data-task-editor="http-response"]');
const headers = document.getElementById(editor?.dataset.headersId ?? "") as HTMLTextAreaElement | null;
const content = document.getElementById(editor?.dataset.contentId ?? "") as HTMLTextAreaElement | null;

if (headers) {
    initLiquidPatternEditor(headers);
}

if (content) {
    initLiquidPatternEditor(content);
}
