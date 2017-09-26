(function ($) {
    'use strict';

    $(document).on('simplemde:initialize', function (event, simplemde) {
        // Replace the image button action.
        simplemde.toolbar[9].action = function (editor) {
            console.log('custom drawImage');
            $("#mediaApp").detach().appendTo('#mediaModalMarkdown .modal-body');
            $("#mediaApp").show();
            var modal = $('#mediaModalMarkdown').modal();
            $('#mediaSelectButton').on('click', function (v) {
                var cm = editor.codemirror;
                cm.replaceSelection('{{ "' + mediaApp.selectedMedia.mediaPath + '" | asset_url | img_tag }}')
                $('#mediaModalMarkdown').modal('hide');
            });
        };
    });
})(jQuery);