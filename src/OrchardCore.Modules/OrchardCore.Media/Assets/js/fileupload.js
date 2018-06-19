$(document).on('mediaApp:ready', function () {
    $('#fileupload').fileupload({
        dropZone: $('#mediaApp'),
        limitConcurrentUploads: 20,
        dataType: 'json',
        url: $('#uploadFiles').val(),
        formData: function () {
            var antiForgeryToken = $("input[name=__RequestVerificationToken]").val();

            return [
                { name: 'path', value: mediaApp.selectedFolder.path },
                { name: '__RequestVerificationToken', value: antiForgeryToken },
            ]
        },
        done: function (e, data) {
            $.each(data.result.files, function (index, file) {
                if (!file.error) {
                    mediaApp.mediaItems.push(file)
                }
            });
        }
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
        dropZone.toggleClass('hover', hoveredDropZone.length);
        window.dropZoneTimeout = setTimeout(function () {
            window.dropZoneTimeout = null;
            dropZone.removeClass('in hover');
        }, 100);
    }    
});