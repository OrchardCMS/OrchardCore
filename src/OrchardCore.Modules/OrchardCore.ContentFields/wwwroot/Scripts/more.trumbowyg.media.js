(function ($) {
    'use strict';

    $.extend(true, $.trumbowyg, {
        langs: {
            en: {
                insertImage: 'Insert Media'
            },
            it: {
                insertImage: 'Aggiungi Immagine'
            }
        },
        plugins: {
            insertImage: {
                init: function (trumbowyg) {
                    var btnDef = {
                        fn: function () {
                            var t = trumbowyg;
                            t.saveRange();
                            $("#mediaApp").detach().appendTo('#mediaModalHtmlField .modal-body');
                            $("#mediaApp").show();
                            mediaApp.selectedMedias = [];
                            var modal = $('#mediaModalHtmlField').modal();
                            $('#mediaHtmlFieldSelectButton').on('click', function (v) {
                                t.restoreRange();
                                t.range.deleteContents();

                                $(window).trigger('scroll');

                                for (i = 0; i < mediaApp.selectedMedias.length; i++) {
                                    var img = document.createElement("img");
                                    img.src = mediaApp.selectedMedias[i].url; //mediaApp.selectedMedias[i].mediaPath;
                                    img.alt = mediaApp.selectedMedias[i].name;
                                    t.range.insertNode(img);

                                    t.syncCode();
                                }

                                t.$c.trigger('tbwchange');
                                t.$c.focus();
                                
                                $('#mediaModalHtmlField').modal('hide');
                                return true;
                            });
                        }
                    };

                    trumbowyg.addBtnDef('insertImage', btnDef);
                }
            },
            resizimg: {
                minSize: 64,
                step: 16,
            }
        }
    });
})(jQuery);