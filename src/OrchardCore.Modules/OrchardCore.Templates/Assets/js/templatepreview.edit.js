var editor;

function initializeTemplatePreview(nameElement, editorElement) {

    var antiforgerytoken = $("[name='__RequestVerificationToken']").val();

    sendFormData = function (nameElement) {

        var formData = {
            'Name': nameElement.value,
            'Content': editor.getValue(),
            '__RequestVerificationToken': antiforgerytoken
        };

        // store the form data to pass it in the event handler
        localStorage.setItem('OrchardCore.templates', JSON.stringify($.param(formData)));
    }

    editor = CodeMirror.fromTextArea(editorElement, {
        autoRefresh: true,
        lineNumbers: true,
        lineWrapping: true,
        matchBrackets: true,
        styleActiveLine: true,
        mode: { name: "liquid" },
        extraKeys: {
            "F11": function (cm) {
                cm.setOption("fullScreen", !cm.getOption("fullScreen"));
            },
            "Esc": function (cm) {
                if (cm.getOption("fullScreen")) cm.setOption("fullScreen", false);
            }
        }
    });

    editor.on('change', function (cm) {
        sendFormData(nameElement);
    });

    window.addEventListener('storage', function (ev) {
        if (ev.key != 'OrchardCore.templates:ready') return; // ignore other keys

        // triggered by the preview window the first time it is loaded in order
        // to pre-render the view even if no contentpreview:render is already sent
        sendFormData(nameElement);
    }, false);

    $(nameElement)
        .on('input', function () { sendFormData(nameElement); })
        .on('propertychange', function () { sendFormData(nameElement); })
        .on('change', function () { sendFormData(nameElement); })
        .on('keyup', function (event) {
            // handle backspace
            if (event.keyCode == 46 || event.ctrlKey) {
                sendFormData(nameElement);
            }
        });

    $(window).on('unload', function () {
        localStorage.removeItem('OrchardCore.templates');
        // this will raise an event in the preview window to notify that the live preview is no longer active.
        localStorage.setItem('OrchardCore.templates:not-connected', '');
        localStorage.removeItem('OrchardCore.templates:not-connected');
   });
}
