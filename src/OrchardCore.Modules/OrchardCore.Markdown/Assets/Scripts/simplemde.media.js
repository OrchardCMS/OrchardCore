(function ($) {
    'use strict';

    $(document).on('simplemde:initialize', function (event, simplemde) {
        // Replace the image button action.
        var editor;
        simplemde.toolbar[9].action = function (e) {
            editor = e;
            $("#mediaApp").detach().appendTo('#mediaModalMarkdown .modal-body');
            $("#mediaApp").show();
            mediaApp.selectedMedias = [];
            var modal = $('#mediaModalMarkdown').modal();
        };
        $('#mediaMarkdownSelectButton').on('click', function (v) {
            var mediaMarkdownContent = "";
            for (i = 0; i < mediaApp.selectedMedias.length; i++) {
                mediaMarkdownContent += ' {{ "' + mediaApp.selectedMedias[i].mediaPath + '" | asset_url | img_tag }}';
            }
            var cm = editor.codemirror;
            cm.replaceSelection(mediaMarkdownContent)
            $('#mediaModalMarkdown').modal('hide');
        });
    });
})(jQuery);