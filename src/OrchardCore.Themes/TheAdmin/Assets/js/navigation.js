$(document).ready(function () {
    $("ul.menu-admin > li").click(function () {
        $(this).siblings().find("> ul").removeClass('open')
        $(this).find("> ul").addClass("open");
    })
});