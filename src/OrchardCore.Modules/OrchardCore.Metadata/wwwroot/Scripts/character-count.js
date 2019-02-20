var charCountInput = $('[data-counter]');

charCountInput.each(function () {
    var currLength = $(this).val().length;
    var label = $('label[for="' + $(this).attr('id') + '"]');
    var span = $('<span />').addClass('counter-display').html(currLength);
    var maxLength = $(this).data('counter-max-length');
    if (maxLength) {
        if (currLength > maxLength) {
            span.addClass('is-invalid');
        }
    }
    label.append(span);
});

charCountInput.keyup(function () {
    var length = $(this).val().length;
    var display = $('label[for="' + $(this).attr('id') + '"] > .counter-display');
    var maxLength = $(this).data('counter-max-length');
    if (maxLength) {
        if (length > maxLength) {
            display.addClass('is-invalid');
        }
        else {
            display.removeClass('is-invalid');
        }
    }
    display.text(length);
});