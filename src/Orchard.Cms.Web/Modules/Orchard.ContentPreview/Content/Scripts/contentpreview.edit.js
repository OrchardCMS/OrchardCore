//$(function () {
//    $('.content-preview-text')
//    .on('input', function () {
//        $(document).trigger('contentpreview:render');
//    })
//    .on('keyup', function (event) {
//        // handle backspace
//        if (event.keyCode == 46) {
//            $(document).trigger('contentpreview:render');
//        }
//    });
//});

$(function () {
    $(document)
    .on('input', '.content-preview-text', function () {
        $(document).trigger('contentpreview:render');
    })
    .on('keyup', '.content-preview-text', function (event) {
        // handle backspace
        if (event.keyCode == 46) {
            $(document).trigger('contentpreview:render');
        }
    })
    .on('change', '.content-preview-select', function () {
        $(document).trigger('contentpreview:render');
    });
});