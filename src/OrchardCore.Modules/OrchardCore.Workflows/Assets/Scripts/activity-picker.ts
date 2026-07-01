var applyFilter = function (category: string | null, q: string | null) {
    const type = document.querySelector<HTMLElement>('.modal-activities')?.dataset.activityType;
    category = category || document.querySelector('.activity-picker-categories .nav-link.active')?.getAttribute('href')?.substring(1) || '';
    q = q || (document.querySelector<HTMLInputElement>('.modal-activities input[type=search]')?.value ?? '');

    const cards = Array.from(document.querySelectorAll<HTMLElement>('.activity.col'));
    cards.forEach((card) => card.style.display = '');

    // Remove activities whoes type doesn't match the configured activity type.
    cards.filter((card) => card.dataset.activityType != type)
        .forEach((card) => card.style.display = 'none');

    if (q.length > 0) {
        // Remove activities whose title doesn't match the query.
        cards.filter((card) => (card.querySelector('.card-title')?.textContent ?? '').toLowerCase().indexOf(q!.toLowerCase()) < 0)
            .forEach((card) => card.style.display = 'none');
    }
    else {
        // Remove activities whose category doesn't match the selected one.
        cards.filter((card) => (card.dataset.category ?? '').toLowerCase() != category!.toLowerCase() && category!.toLowerCase() != 'all')
            .forEach((card) => card.style.display = 'none');
    }

    // Show or hide categories based on whether there are any available activities.
    document.querySelectorAll<HTMLElement>('.activity-picker-categories [data-category]').forEach((categoryListItem) => {
        const category = categoryListItem.dataset.category;

        // Count number of activities within this category and for the specified activity type (Event or Task).
        const activityCount = document.querySelectorAll(`.activity.col[data-category='${category}'][data-activity-type='${type}']`).length;
        categoryListItem.style.display = activityCount == 0 ? 'none' : '';
    });
};

document.addEventListener('DOMContentLoaded', () => {
    document.querySelector('.activity-picker-categories')?.addEventListener('click', (e) => {
        const link = (e.target as Element)?.closest<HTMLElement>('.nav-link');
        if (!link) {
            return;
        }
        applyFilter(link.getAttribute('href')?.substring(1) ?? null, null);
    });

    document.querySelector('.modal-activities input[type=search]')?.addEventListener('keyup', (e) => {
        applyFilter(null, (e.target as HTMLInputElement).value);
    });

    document.getElementById('activity-picker')?.addEventListener('show.bs.modal', function (this: HTMLElement, event) {
        const button = (event as any).relatedTarget as HTMLElement | null;
        const title = button?.dataset.pickerTitle ?? '';
        const type = button?.dataset.activityType ?? '';
        const modal = this;
        modal.querySelector<HTMLElement>('[href="#all"]')?.click();
        const modalTitle = modal.querySelector('.modal-title');
        if (modalTitle) {
            modalTitle.textContent = title;
        }
        modal.dataset.activityType = type;
        applyFilter(null, null);
    });
});
