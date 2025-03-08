document.addEventListener('DOMContentLoaded', function () {
    const elements = document.querySelectorAll('.disabledContent__wrapper input, .disabledContent__wrapper textarea, .disabledContent__wrapper button');
    elements.forEach(function (element) {
        element.disabled = true;
    });

    const toggleButtons = document.querySelectorAll('.disabledContent__wrapper .widget-editor .widget-editor-header .widget-editor-btn-toggle');
    toggleButtons.forEach(function (element) {
        element.disabled = false;
    });

    setTimeout(function () {
        const textareas = document.querySelectorAll('.disabledContent__wrapper textarea');
        textareas.forEach(function (element) {
            element.disabled = true;
        });
    });
});

