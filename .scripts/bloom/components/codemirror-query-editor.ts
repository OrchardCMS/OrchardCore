import { getDatasetJson } from "../helpers/dataset";

// A single CodeMirror.fromTextArea() call reading its mode/extra-options off the textarea's own
// data-mode/data-options attributes - shared by every "one query, one CodeMirror box" admin view
// (SQL/Elasticsearch/Lucene query editors) instead of each duplicating the same call.
const initCodeMirrorQueryEditor = (element: HTMLElement) => {
    const textArea = element as HTMLTextAreaElement;
    const mode = textArea.dataset.mode ?? "javascript";
    const options = getDatasetJson<Record<string, unknown>>(textArea, "options") ?? {};

    CodeMirror.fromTextArea(textArea, {
        mode,
        lineNumbers: true,
        viewportMargin: Infinity,
        ...options,
    });
};

export default initCodeMirrorQueryEditor;
