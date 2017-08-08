var editor;

function sendFormData(previewId) {
    var template = { "description": document.getElementById('Name').value, "content": editor.getValue() };
    localStorage.setItem('templates:' + previewId, JSON.stringify(template));
}

function initializeTemplatePreview(previewId, editorElement) {
    editor = CodeMirror.fromTextArea(editorElement, {
        lineNumbers: true,
        styleActiveLine: true,
        matchBrackets: true,
        mode: { name: "liquid" },
    });

    editor.on('change', function (cm) {
        sendFormData(previewId);
    });

    $(window).on('storage', function (ev) {
        if (ev.originalEvent.key != 'templates:ready:' + previewId) return; // ignore other keys

        // triggered by the preview window the first time it is loaded in order
        // to pre-render the view even if no contentpreview:render is already sent
        sendFormData(previewId);
    });

    $(document.getElementById('@Html.IdFor(x => x.Content)'))
        .on('input', sendFormData)
        .on('propertychange', sendFormData)
        .on('change', sendFormData)
        .on('keyup', function (event) {
            // handle backspace
            if (event.keyCode == 46 || event.ctrlKey) {
                sendFormData(previewId);
            }
        });
}