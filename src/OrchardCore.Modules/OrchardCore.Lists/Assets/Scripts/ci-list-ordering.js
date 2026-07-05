function updateContentItemOrders(oldIndex, newIndex) {
    var url = document.getElementById('ordering-url').dataset.url;
    var containerId = document.getElementById('container-id').dataset.id;
    var before = document.getElementById('pager-before').dataset.before;
    var after = document.getElementById('pager-after').dataset.after;
    var pageSize = document.getElementById('pager-pagesize').dataset.pagesize;

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: new URLSearchParams({
            __RequestVerificationToken: document.querySelector("input[name='__RequestVerificationToken']").value,
            containerId: containerId,
            oldIndex: oldIndex,
            newIndex: newIndex,
            'pagerSlimParameters[before]': before,
            'pagerSlimParameters[after]': after,
            pageSize: pageSize
        })
    }).then(function (response) {
        if (!response.ok) {
            throw new Error('Request failed');
        }
    }).catch(function () {
        alert(document.getElementById('update-order-error-message').dataset.message);
    });
}

document.addEventListener('DOMContentLoaded', function () {
    var sortable = document.getElementById("ci-sortable");

    Sortable.create(sortable, {
        handle: ".ui-sortable-handle",
        onSort: function (evt) {
            var oldIndex = evt.oldIndex;
            var newIndex = evt.newIndex;
            updateContentItemOrders(oldIndex, newIndex);
        }
    });
});
