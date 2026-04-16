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
                            var modalEl = document.getElementById("mediaModalHtmlField");
                            var modal = new bootstrap.Modal(modalEl);
                            modal.show();
                            //disable and reset on click event over the button to avoid issue if press button multiple times or have multiple editor
                            $('#mediaHtmlFieldSelectButton').off('click');
                            $('#mediaHtmlFieldSelectButton').on('click', function (v) {
                                //avoid multiple image insert
                                trumbowyg.restoreRange();
                                trumbowyg.range.deleteContents();

                                $(window).trigger('scroll');

                                var handle = modalEl._pickerHandle;
                                var selectedFiles = handle ? handle.getSelectedFiles() : [];
                                for (let i = 0; i < selectedFiles.length; i++) {
                                    var img = document.createElement("img");
                                    img.src = selectedFiles[i].url || '';
                                    img.alt = selectedFiles[i].name;
                                    trumbowyg.range.insertNode(img);
                                }
                                
                                trumbowyg.syncCode();
                                trumbowyg.$c.trigger('tbwchange');
                                //avoid image to be selected after add it
                                trumbowyg.$c.focus();

                                modal.hide();
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