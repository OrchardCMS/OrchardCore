$(function () {
    $(document)
        .on('input', '.content-preview-text', function () {
            $(document).trigger('contentpreview:render');
        })
        .on('propertychange', '.content-preview-text', function () {
            $(document).trigger('contentpreview:render');
        })
        .on('keyup', '.content-preview-text', function (event) {
            // handle backspace
            if (event.keyCode == 46 || event.ctrlKey) {
                $(document).trigger('contentpreview:render');
            }
        })
        .on('change', '.content-preview-select', function () {
            $(document).trigger('contentpreview:render');
        });
});


$(function () {
    
    var previewButton, contentItemType, previewId, previewContentItemId, previewContentItemVersionId, form, formData;

    previewButton = document.getElementById('previewButton');
    contentItemType = $(document.getElementById('contentItemType')).data('value');
    previewId = $(document.getElementById('previewId')).data('value');
    previewContentItemId = $(document.getElementById('previewContentItemId')).data('value');
    previewContentItemVersionId = $(document.getElementById('previewContentItemVersionId')).data('value');
    form = $(previewButton).closest('form');

    sendFormData = function () {

        formData = form.serializeArray(); // convert form to array
        formData.push({ name: "ContentItemType", value: contentItemType });
        formData.push({ name: "PreviewId", value: previewId });
        formData.push({ name: "PreviewContentItemId", value: previewContentItemId });
        formData.push({ name: "PreviewContentItemVersionId", value: previewContentItemVersionId });

        // store the form data to pass it in the event handler
        localStorage.setItem('contentpreview:' + previewId, JSON.stringify($.param(formData)));
    }

    $(document).on('contentpreview:render', function () {
        sendFormData();
    });


    $(window).on('storage', function (ev) {
        if (ev.originalEvent.key != 'contentpreview:ready:' + previewId) return; // ignore other keys

        // triggered by the preview window the first time it is loaded in order
        // to pre-render the view even if no contentpreview:render is already sent
        sendFormData();
    });    

    $(window).on('unload', function () {
        localStorage.removeItem('contentpreview:' + previewId);
        // this will raise an event in the preview window to notify that the live preview is no longer active.
        localStorage.setItem('contentpreview:not-connected:' + previewId, '');
        localStorage.removeItem('contentpreview:not-connected:' + previewId);
    });
});
