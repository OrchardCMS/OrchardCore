(function ($) {
    'use strict';

    $.extend(true, $.trumbowyg, {
        langs: {
            en: {
                insertImage: 'Insert Media'
            }
        },
        plugins: {
            insertImage: {
                init: function (trumbowyg) {
                    var btnDef = {
                        fn: function () {
                            trumbowyg.saveRange();
                            $("#mediaApp").detach().appendTo('#mediaModalHtmlField .modal-body');
                            $("#mediaApp").show();
                            mediaApp.selectedMedias = [];
                            var modal = $('#mediaModalHtmlField').modal();
                            $('#mediaHtmlFieldSelectButton').on('click', function (v) {
                                var mediaBodyContent = "";
                                for (i = 0; i < mediaApp.selectedMedias.length; i++) {
                                    var img = document.createElement("img");
                                    img.src = mediaApp.selectedMedias[i].url;
                                    img.alt = mediaApp.selectedMedias[i].name;
                                    trumbowyg.range.insertNode(img);
                                }
                                trumbowyg.syncTextarea();
                                $(document).trigger('contentpreview:render');
                                $('#mediaModalHtmlField').modal('hide');
                                return true;
                            });
                        }
                    };

                    trumbowyg.addBtnDef('insertImage', btnDef);
                }
            }
        }
    });
})(jQuery);