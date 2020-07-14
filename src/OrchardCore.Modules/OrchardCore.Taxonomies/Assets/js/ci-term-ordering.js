function updateContentItemOrders(oldIndex, newIndex) {
    var url = $('#ordering-url').data("url");
    var taxonomyItemId = $('#term-id').data("id");
    $.ajax({
        url: url,
        method: 'POST',
        data: {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
            taxonomyItemId: taxonomyItemId,
            oldIndex: oldIndex,
            newIndex: newIndex
        },
        error: function (error) {
            alert($('#update-order-error-message').data("message"));
        }
    });
}

$(function () {
    var sortable = document.getElementById("ci-sortable");

    sortable = Sortable.create(sortable, {
        handle: ".ui-sortable-handle",
        onSort: function (evt) {
            var oldIndex = evt.oldIndex;
            var newIndex = evt.newIndex;
            updateContentItemOrders(oldIndex, newIndex);
        }
    });
});
