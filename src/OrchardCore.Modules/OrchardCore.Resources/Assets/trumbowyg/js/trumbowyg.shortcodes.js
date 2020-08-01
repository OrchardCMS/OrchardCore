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
                            trumbowyg.saveRange();         
                            
                            shortcodesApp.init(function (returnShortcode) {
                                trumbowyg.restoreRange();
                                trumbowyg.range.deleteContents();
                                trumbowyg.range.insertNode(document.createTextNode(returnShortcode));
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
