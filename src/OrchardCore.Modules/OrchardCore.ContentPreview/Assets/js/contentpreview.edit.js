$(function () {
    $(document)
        .on('input', '.content-preview-text', function () {
            $(document).trigger('contentpreview:render');
        })
        .on('propertychange', '.content-preview-text', function () {
            $(document).trigger('contentpreview:render');
        })
        .on('keyup', '.content-preview-text', function (e) {
            // handle backspace
            if (e.key === 'Backspace' || e.ctrlKey) {
                $(document).trigger('contentpreview:render');
            }
        })
        .on('change', '.content-preview-select', function () {
            $(document).trigger('contentpreview:render');
        });
});

$(function () {
    var previewButton, contentItemType, previewId, previewContentItemId, previewContentItemVersionId, draftUrl, form, channel, draftTimer;

    previewButton = document.getElementById('previewButton');
    contentItemType = $(document.getElementById('contentItemType')).data('value');
    previewId = $(document.getElementById('previewId')).data('value');
    previewContentItemId = $(document.getElementById('previewContentItemId')).data('value');
    previewContentItemVersionId = $(document.getElementById('previewContentItemVersionId')).data('value');
    draftUrl = $(document.getElementById('draftUrl')).data('value');
    form = $(previewButton).closest('form');
    channel = new BroadcastChannel('contentpreview-' + previewId);

    // When the preview window signals it is ready, send the current draft immediately.
    channel.onmessage = function (ev) {
        if (ev.data && ev.data.type === 'ready') {
            sendDraft();
        }
    };

    function sendDraft() {
        var formData = form.serializeArray();
        formData.push({ name: 'ContentItemType', value: contentItemType });
        formData.push({ name: 'PreviewContentItemId', value: previewContentItemId });
        formData.push({ name: 'PreviewContentItemVersionId', value: previewContentItemVersionId });

        $.post(draftUrl, $.param(formData))
            .done(function (data) {
                channel.postMessage({ type: 'token', previewUrl: data.previewUrl });
            })
            .fail(function (data) {
                if (data.responseJSON && data.responseJSON.errors) {
                    channel.postMessage({ type: 'error', errors: data.responseJSON.errors });
                }
            });
    }

    $(document).on('contentpreview:render', function () {
        clearTimeout(draftTimer);
        draftTimer = setTimeout(sendDraft, 150);
    });

    $(window).on('unload', function () {
        channel.postMessage({ type: 'disconnected' });
        channel.close();
    });
});
