$(function () {

    $(document).on('click', '.form-section-btn-toggle', function () {
        $(this).closest('.form-section').toggleClass('collapsed');
    });

});