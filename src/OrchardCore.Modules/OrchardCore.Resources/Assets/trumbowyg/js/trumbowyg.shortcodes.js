(function ($) {
    'use strict';

    $.extend(true, $.trumbowyg, {
        langs: {
            en: {
                insertShortcode: 'Insert Shortcode'
            }
        },
        plugins: {
            insertShortcode: {
                init: function (trumbowyg) {
                    var btnDef = {
                        fn: function () {
                            
                            shortcodesApp.init();
                            trumbowyg.saveRange();
                            $(shortcodesApp.$el).on('hide.bs.modal', function (e) {
                                trumbowyg.restoreRange();
                                trumbowyg.range.deleteContents();
                                trumbowyg.range.insertNode(document.createTextNode(shortcodesApp.returnShortcode));
                                trumbowyg.syncCode();
                                trumbowyg.$c.trigger('tbwchange');
                                trumbowyg.$c.focus();   
                            });                            
                        },    
                        hasIcon: true
                    };
                    trumbowyg.addBtnDef('insertShortcode', btnDef);
                }
            }
        }
    });
})(jQuery);
