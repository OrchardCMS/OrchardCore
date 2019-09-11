var charCountInput = $('[data-counter]');

charCountInput.each(function () {
    var maxLength = $(this).data('counter-max-length');
    if (maxLength !== 0) {
        var currLength = $(this).val().length;
        var remainLength = 0;
        if (maxLength && currLength <= maxLength) {
            remainLength = maxLength - currLength;
        }
        var label = $('label[for="' + $(this).attr('id') + '"]');
        var span = $('<span />').addClass('counter-display').html(remainLength + " characters remaining of " + maxLength);
        if (maxLength && remainLength === 0) {
            span.addClass('is-invalid');
        }
        $(this).after(span);
    }
});

charCountInput.keyup(function () {
    var maxLength = $(this).data('counter-max-length');
    if (maxLength !== 0) {
        var currLength = $(this).val().length;
        var display = $(this).next('.counter-display');
        var remainLength = 0;
        if (maxLength && currLength <= maxLength) {
            remainLength = maxLength - currLength;
        }
        if (maxLength) {
            if (remainLength === 0) {
                display.addClass('is-invalid');
            }
            else {
                display.removeClass('is-invalid');
            }
        }
        display.text(remainLength + " characters remaining of " + maxLength);
    }
});