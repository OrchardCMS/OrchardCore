$(function () {
    $('.disabledContent__wrapper input').prop('disabled', true);
    $('.disabledContent__wrapper textarea').prop('disabled', true);
    $('.disabledContent__wrapper button').prop('disabled', true);
    $('.disabledContent__wrapper .widget-editor .widget-editor-header .widget-editor-btn-toggle').prop('disabled', false);

    setTimeout(function () {
        $('.disabledContent__wrapper textarea').prop('disabled', true);
    });
});
