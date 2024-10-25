(function ($) {
    'use strict';

    $.extend(true, $.trumbowyg, {
        langs: {
            // jshint camelcase:false
            en: {
                fontFamily: 'Font'
            },
            az: {
                fontFamily: 'Şrift'
            },
            by: {
                fontFamily: 'Шрыфт'
            },
            ca: {
                fontFamily: 'Font'
            },
            da: {
                fontFamily: 'Skrifttype'
            },
            de: {
                fontFamily: 'Schriftart'
            },
            es: {
                fontFamily: 'Fuente'
            },
            et: {
                fontFamily: 'Font'
            },
            fr: {
                fontFamily: 'Police'
            },
            hu: {
                fontFamily: 'Betűtípus'
            },
            ko: {
                fontFamily: '글꼴'
            },
            nl: {
                fontFamily: 'Lettertype'
            },
            pt_br: {
                fontFamily: 'Fonte',
            },
            ru: {
                fontFamily: 'Шрифт'
            },
            sl: {
                fontFamily: 'Pisava'
            },
            tr: {
                fontFamily: 'Yazı tipi'
            },
            zh_tw: {
                fontFamily: '字體',
            },
        }
    });
    // jshint camelcase:true

    var defaultOptions = {
        fontList: [
            {name: 'Arial', family: 'Arial, Helvetica, sans-serif'},
            {name: 'Arial Black', family: 'Arial Black, Gadget, sans-serif'},
            {name: 'Comic Sans', family: 'Comic Sans MS, Textile, cursive, sans-serif'},
            {name: 'Courier New', family: 'Courier New, Courier, monospace'},
            {name: 'Georgia', family: 'Georgia, serif'},
            {name: 'Impact', family: 'Impact, Charcoal, sans-serif'},
            {name: 'Lucida Console', family: 'Lucida Console, Monaco, monospace'},
            {name: 'Lucida Sans', family: 'Lucida Sans Uncide, Lucida Grande, sans-serif'},
            {name: 'Palatino', family: 'Palatino Linotype, Book Antiqua, Palatino, serif'},
            {name: 'Tahoma', family: 'Tahoma, Geneva, sans-serif'},
            {name: 'Times New Roman', family: 'Times New Roman, Times, serif'},
            {name: 'Trebuchet', family: 'Trebuchet MS, Helvetica, sans-serif'},
            {name: 'Verdana', family: 'Verdana, Geneva, sans-serif'}
        ]
    };

    // Add dropdown with web safe fonts
    $.extend(true, $.trumbowyg, {
        plugins: {
            fontfamily: {
                init: function (trumbowyg) {
                    trumbowyg.o.plugins.fontfamily = $.extend({},
                      defaultOptions,
                      trumbowyg.o.plugins.fontfamily || {}
                    );

                    trumbowyg.addBtnDef('fontfamily', {
                        dropdown: buildDropdown(trumbowyg),
                        hasIcon: false,
                        text: trumbowyg.lang.fontFamily
                    });
                }
            }
        }
    });

    function buildDropdown(trumbowyg) {
        var dropdown = [];

        $.each(trumbowyg.o.plugins.fontfamily.fontList, function (index, font) {
            trumbowyg.addBtnDef('fontfamily_' + index, {
                title: '<span style="font-family: ' + font.family + ';">' + font.name + '</span>',
                hasIcon: false,
                fn: function () {
                    trumbowyg.execCmd('fontName', font.family, true);
                }
            });
            dropdown.push('fontfamily_' + index);
        });

        return dropdown;
    }
})(jQuery);
