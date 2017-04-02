function getConfirmRemoveMessage() {
    return $('#confirmRemoveMessage').data('value');
}

$(function () {
    $("body").on("click", "[itemprop~='RemoveUrl']", function () {
        // don't show the confirm dialog if the link is also UnsafeUrl, as it will already be handled in base.js
        if ($(this).filter("[itemprop~='UnsafeUrl']").length == 1) {
            return false;
        }

        // use a custom message if its set in data-message
        var dataMessage = $(this).data('message');
        if (dataMessage === undefined) {
            dataMessage = getConfirmRemoveMessage();
        }

        return confirm(dataMessage);
    });
});

$(function () {
    var magicToken = $("input[name=__RequestVerificationToken]").first();
    if (magicToken) {
        $("body").on("click", "a[itemprop~='UnsafeUrl'], a[data-unsafe-url]", function () {
            var _this = $(this);
            var hrefParts = _this.attr("href").split("?");
            var form = $("<form action=\"" + hrefParts[0] + "\" method=\"POST\" />");
            form.append(magicToken.clone());
            if (hrefParts.length > 1) {
                var queryParts = hrefParts[1].split("&");
                for (var i = 0; i < queryParts.length; i++) {
                    var queryPartKVP = queryParts[i].split("=");
                    //trusting hrefs in the page here
                    form.append($("<input type=\"hidden\" name=\"" + decodeURIComponent(queryPartKVP[0]) + "\" value=\"" + decodeURIComponent(queryPartKVP[1]) + "\" />"));
                }
            }
            form.css({ "position": "absolute", "left": "-9999em" });
            $("body").append(form);

            var unsafeUrlPrompt = _this.data("unsafe-url");

            if (unsafeUrlPrompt && unsafeUrlPrompt.length > 0) {
                if (!confirm(unsafeUrlPrompt)) {
                    return false;
                }
            }

            if (_this.filter("[itemprop~='RemoveUrl']").length == 1) {
                // use a custom message if its set in data-message
                var dataMessage = _this.data('message');
                if (dataMessage === undefined) {
                    dataMessage = getConfirmRemoveMessage();
                }

                if (!confirm(dataMessage)) {
                    return false;
                }
            }

            form.submit();
            return false;

        });
    }
});

$(function () {
    $('input[data-toggle="collapse"]').each(function () {
        // Prevent bootstrap from altering its behavior
        // c.f. https://github.com/twbs/bootstrap/issues/21079
        $(this).removeAttr("data-toggle");

        // Expand the section if necessary
        var target = $($(this).data('target'));
        if ($(this).prop('checked')) {
            target.addClass('show');
        }

        $(this).on('change', function (e) {
            // During a double-click, ignore state changes while the element is collapsing
            if (target.hasClass('collapsing')) {
                $(this).prop('checked', !$(this).prop('checked'));                
            }
            target.collapse($(this).prop('checked') ? 'show' : 'hide');
        });
    });
});

$(function () {
    $('input[data-toggle="collapse active"]').each(function () {
        // Prevent bootstrap from altering its behavior for inputs that hide target when input value is checked
        // c.f. https://github.com/twbs/bootstrap/issues/21079
        $(this).removeAttr("data-toggle");

        // Expand the section if necessary
        var target = $($(this).data('target'));
        if (!$(this).prop('checked')) {
            target.addClass('show');
        }

        $(this).on('change', function (e) {
            // During a double-click, ignore state changes while the element is collapsing
            if (target.hasClass('collapsing')) {
                console.log('collapsing');
                $(this).prop('checked', !$(this).prop('checked'));
            }
            target.collapse($(this).prop('checked') ? 'hide' : 'show');
        });
    });
});

function getTechnicalName(name){
    var result = "", c;

    if (!name || name.length == 0) {
        return "";
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