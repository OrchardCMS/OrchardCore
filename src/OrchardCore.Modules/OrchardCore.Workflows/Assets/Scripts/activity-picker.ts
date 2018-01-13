///<reference path="../../../OrchardCore.Resources/Assets/jQuery/Typings/jquery-latest.d.ts" />

var applyFilter = function (category: string, q: string) {
    category = category || $('.activity-picker-categories .nav-link.active').attr('href').substr(1);
    q = q || <string>$('.modal-activities input[type=search]').val();

    const $cards = $('.activity.card').show();

    $cards.filter((i, el) => {
        return ($(el).data('category').toLowerCase() != category.toLowerCase() && category.toLowerCase() != 'all') || ($(el).find('.card-title').text().toLowerCase().indexOf(q.toLowerCase()) < 0 && q && q.length > 0);
    }).hide();

};

$(() => {
    $('.activity-picker-categories').on('click', '.nav-link', e => {
        applyFilter($(e.target).attr('href').substr(1), null);
    });

    $('.modal-activities input[type=search]').on('keyup', e => {
        applyFilter(null, <string>$(e.target).val());
    });
});