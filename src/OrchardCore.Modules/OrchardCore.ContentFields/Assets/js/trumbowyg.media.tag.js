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
                                trumbowyg.restoreRange();
                                trumbowyg.range.deleteContents();

                                var handle = modalEl._pickerHandle;
                                var selectedFiles = handle ? handle.getSelectedFiles() : [];
                                for (let i = 0; i < selectedFiles.length; i++) {
                                    var mediaBodyContent = ' [image]' + selectedFiles[i].filePath + '[/image]';
                                    var node = document.createTextNode(mediaBodyContent);
                                    trumbowyg.range.insertNode(node);
                                }
                                
                                trumbowyg.syncCode();
                                trumbowyg.$c.trigger('tbwchange');
                                //avoid tag to be selected after add it
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