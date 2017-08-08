var editor;

function sendFormData(nameElement) {
    var template = { "description": nameElement.value, "content": editor.getValue() };
    localStorage.setItem('orchard.templates', JSON.stringify(template));
}

function initializeTemplatePreview(nameElement, editorElement) {
    editor = CodeMirror.fromTextArea(editorElement, {
        lineNumbers: true,
        styleActiveLine: true,
        matchBrackets: true,
        mode: { name: "liquid" },
    });

    editor.on('change', function (cm) {
        sendFormData(nameElement);
    });

    window.addEventListener('storage', function (ev) {
        if (ev.key != 'orchard.templates:ready') return; // ignore other keys

        // triggered by the preview window the first time it is loaded in order
        // to pre-render the view even if no contentpreview:render is already sent
        sendFormData(nameElement);
    }, false);

    $(nameElement)
        .on('input', sendFormData(nameElement))
        .on('propertychange', sendFormData(nameElement))
        .on('change', sendFormData(nameElement))
        .on('keyup', function (event) {
            // handle backspace
            if (event.keyCode == 46 || event.ctrlKey) {
                sendFormData(nameElement);
            }
        });
}