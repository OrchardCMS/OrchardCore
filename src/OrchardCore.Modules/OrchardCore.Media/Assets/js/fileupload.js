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
                if (!file.error) {
                    mediaApp.mediaItems.push(file)
                } else {
                    mediaApp.errors.push(file.error);
                }
            });

            $('#progress .progress-bar').css(
                'width',
                0 + '%'
            );
        }
    });
});
