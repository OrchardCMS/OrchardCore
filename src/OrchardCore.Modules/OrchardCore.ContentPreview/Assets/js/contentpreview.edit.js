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
    var previewButton, contentItemType, previewId, previewContentItemId, previewContentItemVersionId, draftUrl, form, channel, draftTimer, currentXHR, currentToken;

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

    // Announce that this editor is connected. An already-open preview window replies with
    // a 'ready' message, which triggers a draft send. This re-establishes the connection
    // after the editor page reloads (e.g. "Save and continue") without the user having to
    // edit a field first, and clears the "Preview Disconnected" banner automatically.
    channel.postMessage({ type: 'hello' });

    function sendDraft() {
        // Cancel any in-flight request so stale responses don't overwrite a newer preview.
        if (currentXHR) {
            currentXHR.abort();
            currentXHR = null;
        }

        var formData = form.serializeArray();
        formData.push({ name: 'ContentItemType', value: contentItemType });
        formData.push({ name: 'PreviewContentItemId', value: previewContentItemId });
        formData.push({ name: 'PreviewContentItemVersionId', value: previewContentItemVersionId });
        formData.push({ name: 'PreviewToken', value: currentToken });

        currentXHR = $.post(draftUrl, $.param(formData))
            .done(function (data) {
                currentXHR = null;
                if (data.previewUrl) {
                    currentToken = data.token;
                    channel.postMessage({ type: 'token', previewUrl: data.previewUrl });
                }
            })
            .fail(function (data) {
                currentXHR = null;
                if (data.statusText !== 'abort' && data.status === 422 && data.responseJSON && data.responseJSON.errors) {
                    channel.postMessage({ type: 'error', errors: data.responseJSON.errors });
                }
            });
    }

    // 500ms debounce: collapses rapid keystrokes (e.g. from a WYSIWYG editor) into a
    // single draft submission after the user pauses, preventing server spam.
    $(document).on('contentpreview:render', function () {
        clearTimeout(draftTimer);
        draftTimer = setTimeout(sendDraft, 500);
    });

    // 'pagehide' is the modern, reliable replacement for the deprecated 'unload' event;
    // it fires on navigation away (including when the page enters the bfcache), letting us
    // notify the preview window that the editor is disconnecting.
    $(window).on('pagehide', function () {
        channel.postMessage({ type: 'disconnected' });
        channel.close();
    });
});
