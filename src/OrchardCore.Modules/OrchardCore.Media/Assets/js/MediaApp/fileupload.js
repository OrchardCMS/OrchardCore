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
        change: function (e, data) {
            const maxSizeLimit = parseInt($('#maxSizeLimit').val());
            var exceededFilesList = [];

            $.each(data.files, function (index, file) {
                if (file.size > maxSizeLimit) {
                    exceededFilesList.push(index);
                    mediaApp.errors.push(`The file '${file.name}' exceeds the limit.`);
                }
            });

            for (var i = 0; i < exceededFilesList.length; i++) {
                data.files.splice(i, 1);
            }
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
        window.dropZoneTimeout = setTimeout(function () {
            window.dropZoneTimeout = null;
            dropZone.removeClass('in');
        }, 100);
    }    
});