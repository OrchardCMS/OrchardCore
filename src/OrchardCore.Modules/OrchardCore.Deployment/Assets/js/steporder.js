function updateStepOrders(oldIndex, newIndex) {
    var url = $('#stepOrderUrl').data("url");
    var id = $('#deploymentPlanId').data("id");

    $.ajax({
        url: url,
        method: 'POST',
        data: {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
            id: id,
            newIndex: newIndex,
            oldIndex: oldIndex,
        },
        error: function (error) {
            alert($('#stepOrderErrorMessage').data("message"));
        }
    });
}

$(function () {
    var sortable = document.getElementById("stepOrder");

    var sortable = Sortable.create(sortable, {
        handle: ".ui-sortable-handle",
        onSort: function (evt) {
            var oldIndex = evt.oldIndex;
            var newIndex = evt.newIndex;
            updateStepOrders(oldIndex, newIndex);
        }
    });
});
