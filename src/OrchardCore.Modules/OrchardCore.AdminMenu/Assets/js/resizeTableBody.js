// javascript to keep the height of list of content items to the full height of the page minus the pager height.
// Once (if) implement flexbox on theAdmin this can be surely better achieved through pure css.
$(function () {
    resizeListBody();

    $('.fade-in-pager').css({ opacity: 1 });
    $('#content-items-list-body').css('overflow', 'auto');

    $(window).on('resize', function () {
        resizeListBody();
    });

    function resizeListBody() {
        var adjustValue = 20;

        var listBody = $('#content-items-list-body');

        if (listBody.offset() == undefined) {
            return;
        }
        var listTop = listBody.offset().top;
        var pagerHeight = $('#footer-pager').height();
        var winHeight = $(window).height();
        var newHeight = winHeight - (listTop + pagerHeight);
        listBody.height(newHeight- adjustValue);
    }
})
