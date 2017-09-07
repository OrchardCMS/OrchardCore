$(document).on('mediaApp:ready', function () {
    $('#fileupload').fileupload({
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
                mediaApp.mediaItems.push(file)
            });
            $('#progress .progress-bar').css(
                'width',
                0 + '%'
            );
        }
    });
});
