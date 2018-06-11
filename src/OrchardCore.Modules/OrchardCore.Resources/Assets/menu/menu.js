var activeClass = 'active';

$(document).ready(function () {
    // Find navbar item with current url and set active.
    var currentUrl = window.location.href.replace(window.location.protocol + '//' + window.location.host, '');
    $('li > a[href="' + currentUrl + '"]').parent().addClass(activeClass);

    // Smooth scrolling on click and set active.
    $('li > a').click(function (e) {
        var href = $(this).attr('href');
        if (href != '' && href != '#') {
            var anchor = $(href.replace(/^\/|\/$/g, ''));
            if (!(typeof anchor.position() === "undefined")) {
                var scrollPoint = anchor.position().top;
                $('body,html').animate({ scrollTop: scrollPoint }, 300);
                if (!$(this).hasClass(activeClass)) {
                    $('li.active').removeClass(activeClass);
                    $(this).parent().addClass(activeClass);
                }
            }
        }
    })
});