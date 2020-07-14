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
                            // Clear shortcodeApp value before opening.
                            
                            shortcodeApp.value = '';
                            shortcodeApp.showControls = false;
                            // TODO localize
                            var m = trumbowyg.openModal($('#shortcode-title').data('localized'), '<div id="shortcode-popover-trumbowyg"></div>')
                            .on('tbwconfirm', function() {
                                $(this).off('tbwconfirm');  
                                shortcodeApp.close();                               
                                trumbowyg.closeModal();

                                trumbowyg.restoreRange();
                                trumbowyg.range.deleteContents();
                                trumbowyg.range.insertNode(document.createTextNode(shortcodeApp.value.defaultShortcode));
                                trumbowyg.syncCode();
                                trumbowyg.$c.trigger('tbwchange');
                                trumbowyg.$c.focus();
                            })
                            .on('tbwcancel', function(){
                                $(this).off('tbwcancel');
                                shortcodeApp.close();
                                trumbowyg.closeModal();
                            });
                            // TODO this will need to size better to allow for larger hints.
                            m.height('350px');
                            // Category data placed on the parent to avoid repeats because of localization adaptions to textarea.
                            shortcodeApp.init('#shortcode-popover-trumbowyg', $(trumbowyg.$c[0]).parents('.shortcode-popover-categories')[0], false);
                        },    
                        hasIcon: true
                    };
                    trumbowyg.addBtnDef('insertShortcode', btnDef);
                }
            }
        }
    });
})(jQuery);