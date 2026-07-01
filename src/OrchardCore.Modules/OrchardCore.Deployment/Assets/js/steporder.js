function updateStepOrders(oldIndex, newIndex) {
    var url = document.getElementById('stepOrderUrl').dataset.url;
    var id = document.getElementById('deploymentPlanId').dataset.id;

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: new URLSearchParams({
            __RequestVerificationToken: document.querySelector("input[name='__RequestVerificationToken']").value,
            id: id,
            newIndex: newIndex,
            oldIndex: oldIndex,
        })
    }).then(function (response) {
        if (!response.ok) {
            throw new Error('Request failed');
        }
    }).catch(function () {
        alert(document.getElementById('stepOrderErrorMessage').dataset.message);
    });
}

document.addEventListener('DOMContentLoaded', function () {
    var sortable = document.getElementById("stepOrder");
    if (!sortable) {
        return;
    }
    sortable = Sortable.create(sortable, {
        handle: ".ui-sortable-handle",
        onSort: function (evt) {
            var oldIndex = evt.oldIndex;
            var newIndex = evt.newIndex;
            updateStepOrders(oldIndex, newIndex);
        }
    });
});
