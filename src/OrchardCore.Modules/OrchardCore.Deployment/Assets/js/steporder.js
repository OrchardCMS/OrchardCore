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



// $(function () {
//     var sortableOptions = {
//       group: {
//         name: "sortable-list"
//       },
//       animation: 250,
//       forceFallback: true,
//       fallbackOnBody: true,
//       swapThreshold: 0.65,
//       onEnd: function (evt) {
//         // When nesting groups use onEnd as onSort fires for every group it passes through.
//         updateRuleOrders($(evt.item).data("conditionid"), $(evt.item).parent().data("conditiongroupid"), evt.newIndex);
//       }
//     }; 
//     var groups = document.querySelectorAll(".condition-group");
//     for (var i = 0; i < groups.length; i++) {
//       new Sortable(groups[i], sortableOptions);
//     }
// });

$(function () {
    var sortable = document.getElementById("stepOrder");

    var sortable = Sortable.create(sortable, {
        // handle: ".ui-sortable-handle",
        onSort: function (evt) {
            var oldIndex = evt.oldIndex;
            var newIndex = evt.newIndex;
            updateStepOrders(oldIndex, newIndex);
        }
    });
});

