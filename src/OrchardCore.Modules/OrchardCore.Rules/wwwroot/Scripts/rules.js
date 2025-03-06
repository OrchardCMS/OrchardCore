function updateRuleOrders(conditionId, toConditionId, toPosition) {
    var url = $('#ruleOrderUrl').data("url");
    var parameters = {};
    $('.ruleorderparameters').each(function(i, val) {
        parameters[$(val).data("param")] = $(val).data("value");
    });

    parameters["__RequestVerificationToken"] = $("input[name='__RequestVerificationToken']").val();
    parameters["conditionId"] = conditionId;
    parameters["toConditionId"] = toConditionId;
    parameters["toPosition"] = toPosition;

    $.ajax({
        url: url,
        method: 'POST',
        data: parameters,
        error: function (error) {
            alert($('#ruleOrderErrorMessage').data("message"));
        }
    });
}

$(function () {
    var sortableOptions = {
      group: {
        name: "sortable-list"
      },
      animation: 250,
      forceFallback: true,
      fallbackOnBody: true,
      swapThreshold: 0.65,
      onEnd: function (evt) {
        // When nesting groups use onEnd as onSort fires for every group it passes through.
        updateRuleOrders($(evt.item).data("conditionid"), $(evt.item).parent().data("conditiongroupid"), evt.newIndex);
      }
    }; 
    var groups = document.querySelectorAll(".condition-group");
    for (var i = 0; i < groups.length; i++) {
      new Sortable(groups[i], sortableOptions);
    }
});
