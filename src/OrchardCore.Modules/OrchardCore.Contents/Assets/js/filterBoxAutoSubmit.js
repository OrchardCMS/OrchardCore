$(function () {
    $(".filter-box select, .filter-box :checkbox").on("change", function () {
        this.form.submit();
    });
})
