function confirmDialog({ callback, ...options }) {
    const defaultOptions = $('#confirmRemoveModalMetadata').data();
    const { title, message, okText, cancelText, okClass, cancelClass } = $.extend({}, defaultOptions, options);

    $('<div id="confirmRemoveModal" class="modal" tabindex="-1" role="dialog">\
        <div class="modal-dialog modal-dialog-centered" role="document">\
            <div class="modal-content">\
                <div class="modal-header">\
                    <h5 class="modal-title">' + title + '</h5>\
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>\
                </div>\
                <div class="modal-body">\
                    <p>' + message + '</p>\
                </div>\
                <div class="modal-footer">\
                    <button id="modalOkButton" type="button" class="btn ' + okClass + '">' + okText + '</button>\
                    <button id="modalCancelButton" type="button" class="btn ' + cancelClass + '" data-bs-dismiss="modal">' + cancelText + '</button>\
                </div>\
            </div>\
        </div>\
    </div>').appendTo('body');

    var confirmModal = new bootstrap.Modal($('#confirmRemoveModal'), {
        backdrop: 'static',
        keyboard: false
    })

    confirmModal.show();

    document.getElementById('confirmRemoveModal').addEventListener('hidden.bs.modal', function () {
        document.getElementById('confirmRemoveModal').remove();
        confirmModal.dispose();
    });

    $('#modalOkButton').click(function () {
        callback(true);
        confirmModal.hide();
    });

    $('#modalCancelButton').click(function () {
        callback(false);
        confirmModal.hide();
    });
}

(function () {

    // Prevents page flickering while downloading css
    document.addEventListener('DOMContentLoaded', () => {
        document.body.classList.remove('preload');
    });

})();


$(function () {
    $('body').on('click', '[data-url-af~="RemoveUrl"], a[itemprop~="RemoveUrl"]', function () {
        var _this = $(this);
        if (_this.filter('a[itemprop~="UnsafeUrl"]').length == 1) {
            console.warn('Please use data-url-af instead of itemprop attribute for confirm modals. Using itemprop will eventually become deprecated.')
        }
        // don't show the confirm dialog if the link is also UnsafeUrl, as it will already be handled below.
        if (_this.filter('[data-url-af~="UnsafeUrl"], a[itemprop~="UnsafeUrl"]').length == 1) {
            return false;
        }
        confirmDialog({
            ..._this.data(),
            callback: function (resp) {
                if (resp) {
                    var url = _this.attr('href');
                    if (url == undefined) {
                        var form = _this.parents('form');
                        // This line is reuired in case we used the FormValueRequiredAttribute
                        form.append($('<input type="hidden" name="' + _this.attr('name') + '" value="' + _this.attr('value') + '" />'));
                        form.submit();
                    }
                    else {
                        window.location = url;
                    }
                }
            }
        });

        return false;
    });
});

$(function () {
    var magicToken = $('input[name=__RequestVerificationToken]').first();
    if (magicToken) {
        $('body').on('click', 'a[data-url-af~="UnsafeUrl"], a[itemprop~="UnsafeUrl"]', function () {
            var _this = $(this);
            if (_this.filter('a[itemprop~="UnsafeUrl"]').length == 1) {
                console.warn('Please use data-url-af instead of itemprop attribute for confirm modals. Using itemprop will eventually become deprecated.')
            }
            var hrefParts = _this.attr("href").split("?");
            var form = $('<form action="' + hrefParts[0] + '" method="POST" />');
            form.append(magicToken.clone());
            if (hrefParts.length > 1) {
                var queryParts = hrefParts[1].split('&');
                for (var i = 0; i < queryParts.length; i++) {
                    var queryPartKVP = queryParts[i].split('=');
                    //trusting hrefs in the page here
                    form.append($('<input type="hidden" name="' + decodeURIComponent(queryPartKVP[0]) + '" value="' + decodeURIComponent(queryPartKVP[1]) + '" />'));
                }
            }
            form.css({ 'position': 'absolute', 'left': '-9999em' });
            $("body").append(form);

            var unsafeUrlPrompt = _this.data('unsafe-url');

            if (unsafeUrlPrompt && unsafeUrlPrompt.length > 0) {
                confirmDialog({
                    ..._this.data(),
                    callback: function (resp) {
                        if (resp) {
                            form.submit();
                        }
                    }
                });

                return false;
            }

            if (_this.filter('[data-url-af~="RemoveUrl"], a[itemprop~="RemoveUrl"]').length == 1) {
                confirmDialog({
                    ..._this.data(),
                    callback: function (resp) {
                        if (resp) {
                            form.submit();
                        }
                    }
                });

                return false;
            }

            form.submit();
            return false;
        });
    }
});

(function () {
    // Tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })
})();

function getTechnicalName(name) {
    var result = '', c;

    if (!name || name.length == 0) {
        return '';
    }

    name = removeDiacritics(name);

    for (i = 0; i < name.length; i++) {
        c = name[i];
        if (isLetter(c) || (isNumber(c) && i > 0)) {
            result += c;
        }
    }

    return result;
}

function isLetter(str) {
    return str.length === 1 && str.match(/[a-z]/i);
}

function isNumber(str) {
    return str.length === 1 && str.match(/[0-9]/i);
}

//Prevent multi submissions on forms
$('body').on('submit', 'form.no-multisubmit', function (e) {
    var submittingClass = 'submitting';
    form = $(this);

    if (form.hasClass(submittingClass)) {
        e.preventDefault();
        return;
    }

    form.addClass(submittingClass);

    // safety-nest in case the form didn't refresh the page
    setTimeout(function () {
        form.removeClass(submittingClass);
    }, 5000);
});
