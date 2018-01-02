var editor;

function storeTemplate(nameElement) {
    var template = { "description": nameElement.value, "content": editor.getValue() };
    localStorage.setItem('OrchardCore.templates', JSON.stringify(template));
}

function initializeTemplatePreview(nameElement, editorElement) {
    editor = CodeMirror.fromTextArea(editorElement, {
        lineNumbers: true,
        styleActiveLine: true,
        matchBrackets: true,
        mode: { name: "liquid" },
    });

    editor.on('change', function (cm) {
        storeTemplate(nameElement);
    });

    window.addEventListener('storage', function (ev) {
        if (ev.key != 'OrchardCore.templates:ready') return; // ignore other keys

        // triggered by the preview window the first time it is loaded in order
        // to pre-render the view even if no contentpreview:render is already sent
        storeTemplate(nameElement);
    }, false);

    $(nameElement)
        .on('input', function () { storeTemplate(nameElement); })
        .on('propertychange', function () { storeTemplate(nameElement); })
        .on('change', function () { storeTemplate(nameElement); })
        .on('keyup', function (event) {
            // handle backspace
            if (event.keyCode == 46 || event.ctrlKey) {
                storeTemplate(nameElement);
            }
        });

    $(window).on('unload', function () {
        localStorage.removeItem('OrchardCore.templates');
        // this will raise an event in the preview window to notify that the live preview is no longer active.
        localStorage.setItem('OrchardCore.templates:not-connected', '');
        localStorage.removeItem('OrchardCore.templates:not-connected');
   });
}