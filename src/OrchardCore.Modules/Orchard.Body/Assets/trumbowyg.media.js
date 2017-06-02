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
                            var modal = $('#mediaModalBody').modal();
                            $('#mediaSelectButton').on('click', function (v) {
                                var node = document.createTextNode('{{{ image ' + mediaApp.selectedMedia.contentItemId + ' }}}');
                                trumbowyg.restoreRange();
                                trumbowyg.range.deleteContents();
                                trumbowyg.range.insertNode(node);
                                trumbowyg.syncTextarea();
                                $(document).trigger('contentpreview:render');
                                $('#mediaModalBody').modal('hide');
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