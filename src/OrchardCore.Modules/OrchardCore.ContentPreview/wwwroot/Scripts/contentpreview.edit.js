document.addEventListener('DOMContentLoaded', function () {

    var previewButton, contentItemType, previewId, previewContentItemId, previewContentItemVersionId, form, formData;

    previewButton = document.getElementById('previewButton');
    contentItemType = document.getElementById('contentItemType').dataset.value;
    previewId = document.getElementById('previewId').dataset.value;
    previewContentItemId = document.getElementById('previewContentItemId').dataset.value;
    previewContentItemVersionId = document.getElementById('previewContentItemVersionId').dataset.value;
    form = previewButton.closest('form');

    function sendFormData() {

        formData = new FormData(form);
        formData.append('ContentItemType', contentItemType);
        formData.append('PreviewId', previewId);
        formData.append('PreviewContentItemId', previewContentItemId);
        formData.append('PreviewContentItemVersionId', previewContentItemVersionId);

        // store the form data to pass it in the event handler
        localStorage.setItem('contentpreview:' + previewId, new URLSearchParams(formData).toString());
    }

    document.addEventListener('contentpreview:render', function () {
        sendFormData();
    });


    window.addEventListener('storage', function (ev) {
        if (ev.key !== 'contentpreview:ready:' + previewId) return; // ignore other keys

        // triggered by the preview window the first time it is loaded in order
        // to pre-render the view even if no contentpreview:render is already sent
        sendFormData();
    });    

    window.addEventListener('beforeunload', function () {
        localStorage.removeItem('contentpreview:' + previewId);
        // this will raise an event in the preview window to notify that the live preview is no longer active.
        localStorage.setItem('contentpreview:not-connected:' + previewId, '');
        localStorage.removeItem('contentpreview:not-connected:' + previewId);
    });
});

