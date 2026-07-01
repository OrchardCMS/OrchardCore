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
                            document.querySelector('#mediaModalBody .modal-body').appendChild(document.getElementById("mediaApp"));
                            document.getElementById("mediaApp").classList.remove("d-none");
                            mediaApp.selectedMedias = [];
                            var modal = new bootstrap.Modal(document.getElementById("mediaModalBody"));
                            modal.show();
                            //disable an reset on click event over the button to avoid issue if press button multiple times or have multiple editor
                            var selectButton = document.getElementById('mediaBodySelectButton');
                            var newSelectButton = selectButton.cloneNode(true);
                            selectButton.replaceWith(newSelectButton);
                            newSelectButton.addEventListener('click', function () {
                                //avoid multiple image insert
                                trumbowyg.restoreRange();
                                trumbowyg.range.deleteContents();

                                window.dispatchEvent(new Event('scroll'));

                                for (let i = 0; i < mediaApp.selectedMedias.length; i++) {
                                    var img = document.createElement("img");
                                    img.src = mediaApp.selectedMedias[i].url;
                                    img.alt = mediaApp.selectedMedias[i].name;
                                    trumbowyg.range.insertNode(img);
                                }

                                trumbowyg.syncCode();
                                trumbowyg.$c[0].dispatchEvent(new CustomEvent('tbwchange'));
                                //avoid image to be selected after add it
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
