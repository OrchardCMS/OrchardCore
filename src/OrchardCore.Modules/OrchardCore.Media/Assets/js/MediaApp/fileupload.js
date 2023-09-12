$(document).on('mediaApp:ready', function () {
    var chunkedFileUploadId = crypto.randomUUID();

    $('#fileupload')
        .fileupload({
            dropZone: $('#mediaApp'),
            limitConcurrentUploads: 20,
            dataType: 'json',
            url: $('#uploadFiles').val(),
            maxChunkSize: Number($('#maxUploadChunkSize').val() || 0),
            formData: function () {
                var antiForgeryToken = $("input[name=__RequestVerificationToken]").val();

                return [
                    { name: 'path', value: mediaApp.selectedFolder.path },
                    { name: '__RequestVerificationToken', value: antiForgeryToken },
                    { name: '__chunkedFileUploadId', value: chunkedFileUploadId },
                ]
            },
            done: function (e, data) {
                $.each(data.result.files, function (index, file) {
                    if (!file.error) {
                        mediaApp.mediaItems.push(file)
                    }
                });
            }
        })
        .on('fileuploadchunkbeforesend', (e, options) => {
            let file = options.files[0];
            // Here we replace the blob with a File object to ensure the file name and others are preserved for the backend.
            options.blob = new File(
                [options.blob],
                file.name,
                {
                    type: file.type,
                    lastModified: file.lastModified,
                });
        });
});


$(document).bind('dragover', function (e) {
    var dt = e.originalEvent.dataTransfer;
    if (dt.types && (dt.types.indexOf ? dt.types.indexOf('Files') != -1 : dt.types.contains('Files'))) {
        var dropZone = $('#customdropzone'),
            timeout = window.dropZoneTimeout;
        if (timeout) {
            clearTimeout(timeout);
        } else {
            dropZone.addClass('in');
        }
        var hoveredDropZone = $(e.target).closest(dropZone);
        window.dropZoneTimeout = setTimeout(function () {
            window.dropZoneTimeout = null;
            dropZone.removeClass('in');
        }, 100);
    }    
});