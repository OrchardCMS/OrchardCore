$('.oc-json-code').each(function () {
    CodeMirror.fromTextArea(this, {
        mode: { name: "javascript", json: true },
        lineNumbers: true,
        lineWrapping: true,
        readOnly: true,
        styleActiveLine: true,
        matchBrackets: true,
        extraKeys: { "Ctrl-Q": function (cm) { cm.foldCode(cm.getCursor()); } },
        foldGutter: true,
        gutters: ["CodeMirror-linenumbers", "CodeMirror-foldgutter"]
    });
});