var monacoEditorUrl = document.currentScript.dataset.monacoEditorUrl;
require.config({ paths: { 'vs': monacoEditorUrl + 'min/vs' } });

window.MonacoEnvironment = {
    getWorkerUrl: function (workerId, label) {
        return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
        self.MonacoEnvironment = {
          baseUrl: '${monacoEditorUrl}min/'
        };
        importScripts('${monacoEditorUrl}min/vs/base/worker/workerMain.min.js');`
        )}`;
    }
};