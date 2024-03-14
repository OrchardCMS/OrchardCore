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
                            $("#mediaApp").detach().appendTo('#mediaModalBody .modal-body');
                            $("#mediaApp").show();
                            mediaApp.selectedMedias = [];
                            var modal = new bootstrap.Modal($("#mediaModalBody"));
                            modal.show();
                            //disable an reset on click event over the button to avoid issue if press button multiple times or have multiple editor
                            $('#mediaBodySelectButton').off('click');
                            $('#mediaBodySelectButton').on('click', function (v) {
                                trumbowyg.restoreRange();
                                trumbowyg.range.deleteContents();
                                
                                for (i = 0; i < mediaApp.selectedMedias.length; i++) {
                                    var mediaBodyContent = ' [image]' + mediaApp.selectedMedias[i].mediaPath + '[/image]';
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
