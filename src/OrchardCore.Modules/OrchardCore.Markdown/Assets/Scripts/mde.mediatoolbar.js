var mdeToolbar;

$(function () {
    mdeToolbar = [
        {
            name: "image",
            action: function (editor) {
                $("#mediaApp").detach().appendTo('#mediaModalMarkdown .modal-body');
                document.getElementById("mediaApp").classList.remove("d-none");
                mediaApp.selectedMedias = [];
                var modal = new bootstrap.Modal($('#mediaModalMarkdown'));
                modal.show();
                $('#mediaMarkdownSelectButton').on('click', function (v) {
                    var mediaMarkdownContent = "";
                    for (i = 0; i < mediaApp.selectedMedias.length; i++) {
                        mediaMarkdownContent += ' [image]' + mediaApp.selectedMedias[i].mediaPath + '[/image]';
                    }
                    var cm = editor.codemirror;
                    cm.replaceSelection(mediaMarkdownContent)
                    modal.hide();
                    $(this).off('click');
                });
            },
            className: "far fa-image fa-sm",
            title: "Insert Image",
            default: true
        }
    ];
});
