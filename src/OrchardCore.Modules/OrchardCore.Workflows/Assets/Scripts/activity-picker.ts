///<reference path='../Lib/jquery/typings.d.ts' />

var applyFilter = function (category: string, q: string) {
    const type = $('.modal-activities').data('activity-type');
    category = category || $('.activity-picker-categories .nav-link.active').attr('href').substr(1);
    q = q || <string>$('.modal-activities input[type=search]').val();

    const $cards = $('.activity.col').show();

    // Remove activities whoes type doesn't match the configured activity type.
    $cards.filter((i, el) => {
        return $(el).data('activity-type') != type;
    }).hide();

    if (q.length > 0) {
        // Remove activities whose title doesn't match the query.
        $cards.filter((i, el) => {
            return $(el).find('.card-title').text().toLowerCase().indexOf(q.toLowerCase()) < 0 && q && q.length > 0;
        }).hide();
    }
    else {
        // Remove activities whose category doesn't match the selected one.
        $cards.filter((i, el) => {
            return $(el).data('category').toLowerCase() != category.toLowerCase() && category.toLowerCase() != 'all';
        }).hide();
    }

    // Show or hide categories based on whether there are any available activities.
    $('.activity-picker-categories [data-category]').each((i, el) => {
        const categoryListItem = $(el);
        const category = categoryListItem.data('category');

        // Count number of activities within this category and for the specified activity type (Event or Task).
        const activityCount = $(`.activity.col[data-category='${category}'][data-activity-type='${type}']`).length;
        activityCount == 0 ? categoryListItem.hide() : categoryListItem.show();
    });
};

$(() => {
    $('.activity-picker-categories').on('click', '.nav-link', e => {
        applyFilter($(e.target).attr('href').substr(1), null);
    });

    $('.modal-activities input[type=search]').on('keyup', e => {
        applyFilter(null, <string>$(e.target).val());
    });

    $('#activity-picker').on('show.bs.modal', function (event) {
        var modalEvent = event as any;
        var button = $(modalEvent.relatedTarget); // Button that triggered the modal.
        var title = button.data('picker-title');
        var type = button.data('activity-type');
        var modal = $(this);
        modal.find('[href="#all"]').click();
        modal.find('.modal-title').text(title);
        modal.data('activity-type', type);
        applyFilter(null, null);
    })
});
