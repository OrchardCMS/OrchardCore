document.addEventListener('DOMContentLoaded', function () {
    function renderPreview() {
        document.dispatchEvent(new CustomEvent('contentpreview:render'));
    }

    document.addEventListener('input', function (e) {
        if (e.target.closest('.content-preview-text')) {
            renderPreview();
        }
    });
    document.addEventListener('propertychange', function (e) {
        if (e.target.closest('.content-preview-text')) {
            renderPreview();
        }
    });
    document.addEventListener('keyup', function (e) {
        // handle backspace
        if ((e.key === 'Backspace' || e.ctrlKey) && e.target.closest('.content-preview-text')) {
            renderPreview();
        }
    });
    document.addEventListener('change', function (e) {
        if (e.target.closest('.content-preview-select')) {
            renderPreview();
        }
    });
});

document.addEventListener('DOMContentLoaded', function () {
    var previewButton, contentItemType, previewId, previewContentItemId, previewContentItemVersionId, draftUrl, form, channel, draftTimer, currentAbortController, currentToken;

    previewButton = document.getElementById('previewButton');
    contentItemType = document.getElementById('contentItemType').dataset.value;
    previewId = document.getElementById('previewId').dataset.value;
    previewContentItemId = document.getElementById('previewContentItemId').dataset.value;
    previewContentItemVersionId = document.getElementById('previewContentItemVersionId').dataset.value;
    draftUrl = document.getElementById('draftUrl').dataset.value;
    form = previewButton.closest('form');
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

    function serializeFormArray(formElement) {
        var result = [];
        new FormData(formElement).forEach(function (value, name) {
            // Matches jQuery's serializeArray(), which skips file inputs (it can't serialize a File
            // into a name/value string pair) - FormData yields File objects for those.
            if (typeof value === 'string') {
                result.push({ name: name, value: value });
            }
        });
        return result;
    }

    function sendDraft() {
        // Cancel any in-flight request so stale responses don't overwrite a newer preview.
        if (currentAbortController) {
            currentAbortController.abort();
            currentAbortController = null;
        }

        var formData = serializeFormArray(form);
        formData.push({ name: 'ContentItemType', value: contentItemType });
        formData.push({ name: 'PreviewContentItemId', value: previewContentItemId });
        formData.push({ name: 'PreviewContentItemVersionId', value: previewContentItemVersionId });
        formData.push({ name: 'PreviewToken', value: currentToken });

        var abortController = new AbortController();
        currentAbortController = abortController;

        fetch(draftUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: new URLSearchParams(formData.map(function (item) { return [item.name, item.value]; })).toString(),
            signal: abortController.signal
        }).then(function (response) {
            currentAbortController = null;
            if (!response.ok) {
                if (response.status !== 422) {
                    console.error('Preview draft request failed', response.status, response.statusText);
                }
                return;
            }
            return response.json().then(function (data) {
                if (data.previewUrl) {
                    currentToken = data.token;
                    channel.postMessage({ type: 'token', previewUrl: data.previewUrl });
                }
            });
        }).catch(function (err) {
            currentAbortController = null;
            if (err.name !== 'AbortError') {
                console.error('Preview draft request failed', err);
            }
        });
    }

    // 500ms debounce: collapses rapid keystrokes (e.g. from a WYSIWYG editor) into a
    // single draft submission after the user pauses, preventing server spam.
    document.addEventListener('contentpreview:render', function () {
        clearTimeout(draftTimer);
        draftTimer = setTimeout(sendDraft, 500);
    });

    // 'pagehide' is the modern, reliable replacement for the deprecated 'unload' event;
    // it fires on navigation away (including when the page enters the bfcache), letting us
    // notify the preview window that the editor is disconnecting.
    window.addEventListener('pagehide', function () {
        channel.postMessage({ type: 'disconnected' });
        channel.close();
    });
});
