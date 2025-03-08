function updateStepOrders(oldIndex, newIndex) {
    const urlElement = document.getElementById('stepOrderUrl');
    const idElement = document.getElementById('deploymentPlanId');
    const requestVerificationTokenElement = document.querySelector("input[name='__RequestVerificationToken']");
    const errorMessageElement = document.getElementById('stepOrderErrorMessage');

    const url = urlElement.dataset.url;
    const id = idElement.dataset.id;

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            __RequestVerificationToken: requestVerificationTokenElement.value,
            id,
            newIndex,
            oldIndex,
        }),
    })
        .then(response => response.json())
        .catch(error => {
            alert(errorMessageElement.dataset.message);
        });
}

document.addEventListener('DOMContentLoaded', () => {
    const sortableElement = document.getElementById('stepOrder');
    if (!sortableElement) {
        return;
    }

    Sortable.create(sortableElement, {
        handle: '.ui-sortable-handle',
        onSort: (evt) => {
            const oldIndex = evt.oldIndex;
            const newIndex = evt.newIndex;
            updateStepOrders(oldIndex, newIndex);
        },
    });
});
