function updateRuleOrders(conditionId, toConditionId, toPosition) {
    var url = document.getElementById('ruleOrderUrl').dataset.url;
    var parameters = {};
    document.querySelectorAll('.ruleorderparameters').forEach(function (val) {
        parameters[val.dataset.param] = val.dataset.value;
    });

    parameters["__RequestVerificationToken"] = document.querySelector("input[name='__RequestVerificationToken']").value;
    parameters["conditionId"] = conditionId;
    parameters["toConditionId"] = toConditionId;
    parameters["toPosition"] = toPosition;

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: new URLSearchParams(parameters)
    }).then(function (response) {
        if (!response.ok) {
            throw new Error('Request failed');
        }
    }).catch(function () {
        alert(document.getElementById('ruleOrderErrorMessage').dataset.message);
    });
}

document.addEventListener('DOMContentLoaded', function () {
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
        updateRuleOrders(evt.item.dataset.conditionid, evt.item.parentElement.dataset.conditiongroupid, evt.newIndex);
      }
    };
    var groups = document.querySelectorAll(".condition-group");
    for (var i = 0; i < groups.length; i++) {
      new Sortable(groups[i], sortableOptions);
    }
});
