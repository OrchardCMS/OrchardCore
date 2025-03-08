function updateContentItemOrders(oldIndex, newIndex) {
    var urlElement = document.getElementById('ordering-url');
    var url = urlElement.dataset.url;
    var containerIdElement = document.getElementById('container-id');
    var containerId = containerIdElement.dataset.id;
    var beforeElement = document.getElementById('pager-before');
    var before = beforeElement.dataset.before;
    var afterElement = document.getElementById('pager-after');
    var after = afterElement.dataset.after;
    var pageSizeElement = document.getElementById('pager-pagesize');
    var pageSize = pageSizeElement.dataset.pagesize;
    var requestVerificationTokenElement = document.querySelector("input[name='__RequestVerificationToken']");
    var errorMessageElement = document.getElementById('update-order-error-message');

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            __RequestVerificationToken: requestVerificationTokenElement.value,
            containerId,
            oldIndex,
            newIndex,
            pagerSlimParameters: {
                before,
                after
            },
            pageSize
        })
    })
        .catch(error => {
            alert(errorMessageElement.dataset.message);
        });
}

document.addEventListener('DOMContentLoaded', function () {
    var sortableElement = document.getElementById("ci-sortable");

    var sortable = Sortable.create(sortableElement, {
        handle: ".ui-sortable-handle",
        onSort: function (evt) {
            var oldIndex = evt.oldIndex;
            var newIndex = evt.newIndex;
            updateContentItemOrders(oldIndex, newIndex);
        }
    });
});

