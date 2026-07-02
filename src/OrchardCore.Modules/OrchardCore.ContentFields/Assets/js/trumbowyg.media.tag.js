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
                            document.querySelector('#mediaModalHtmlField .modal-body').appendChild(document.getElementById("mediaApp"));
                            document.getElementById("mediaApp").classList.remove("d-none");
                            mediaApp.selectedMedias = [];
                            var modal = new bootstrap.Modal(document.getElementById("mediaModalHtmlField"));
                            modal.show();
                            //disable an reset on click event over the button to avoid issue if press button multiple times or have multiple editor
                            var selectButton = document.getElementById('mediaHtmlFieldSelectButton');
                            var newSelectButton = selectButton.cloneNode(true);
                            selectButton.replaceWith(newSelectButton);
                            newSelectButton.addEventListener('click', function () {
                                trumbowyg.restoreRange();
                                trumbowyg.range.deleteContents();

                                for (let i = 0; i < mediaApp.selectedMedias.length; i++) {
                                    var mediaBodyContent = ' [image]' + mediaApp.selectedMedias[i].mediaPath + '[/image]';
                                    var node = document.createTextNode(mediaBodyContent);
                                    trumbowyg.range.insertNode(node);
                                }

                                trumbowyg.syncCode();
                                trumbowyg.$c[0].dispatchEvent(new CustomEvent('tbwchange'));
                                //avoid tag to be selected after add it
                                trumbowyg.$c[0].focus();

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