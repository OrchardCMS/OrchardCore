$(function () {
    $(".menu-admin a").each(function () {
        var link = $(this);
        var href = link.attr('href');
        if (window.location.pathname == href) {
            link.parents('li').children(':checkbox').prop('checked', true);
            link.parents('li').children(':radio').prop('checked', true);
            link.addClass('active');
        }
    });
});