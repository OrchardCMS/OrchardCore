window.initializeShortcodeCodeMirrorEditors = function (contentElement, usageElement, previewElement, nameElement, hintElement) {
    if (contentElement) {
        CodeMirror.fromTextArea(contentElement, {
            autoCloseTags: true,
            autoRefresh: true,
            lineNumbers: true,
            lineWrapping: true,
            matchBrackets: true,
            styleActiveLine: true,
            mode: { name: "liquid" }
        });
    }

    if (usageElement) {
        var editor = CodeMirror.fromTextArea(usageElement, {
            autoCloseTags: true,
            autoRefresh: true,
            lineNumbers: true,
            lineWrapping: true,
            matchBrackets: true,
            styleActiveLine: true,
            mode: { name: "htmlmixed" }
        });
        if (previewElement) {
            editor.on('change', function (e) {
                previewElement.style.display = '';
                previewElement.querySelector('.shortcode-usage').innerHTML = e.doc.getValue();
            });
        }
    }

    if (nameElement && previewElement) {
        var updateNamePreview = function () {
            previewElement.style.display = '';
            previewElement.querySelector('.shortcode-name').innerHTML = nameElement.value;
        };
        nameElement.addEventListener('keyup', updateNamePreview);
        nameElement.addEventListener('paste', updateNamePreview);
    }

    if (hintElement && previewElement) {
        var updateHintPreview = function () {
            previewElement.style.display = '';
            previewElement.querySelector('.shortcode-hint').innerHTML = hintElement.value;
        };
        hintElement.addEventListener('keyup', updateHintPreview);
        hintElement.addEventListener('paste', updateHintPreview);
    }
};
