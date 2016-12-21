$(function () {
    $(".dropdown input").change(function () {
        //alert(this);
        var input = $(this);
        var li = input.closest('li');
        li.toggleClass('active');
    });
});