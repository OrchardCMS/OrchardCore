function updateRuleOrders(conditionId, toConditionId, toPosition) {
    const urlElement = document.getElementById('ruleOrderUrl');
    const url = urlElement.dataset.url;
    const parameters = {};
    const parameterElements = document.querySelectorAll('.ruleorderparameters');
    for (const element of parameterElements) {
        parameters[element.dataset.param] = element.dataset.value;
    }

    const requestVerificationTokenElement = document.querySelector("input[name='__RequestVerificationToken']");
    parameters["__RequestVerificationToken"] = requestVerificationTokenElement.value;
    parameters["conditionId"] = conditionId;
    parameters["toConditionId"] = toConditionId;
    parameters["toPosition"] = toPosition;

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(parameters),
    })
    .catch(error => {
        const errorMessageElement = document.getElementById('ruleOrderErrorMessage');
        alert(errorMessageElement.dataset.message);
    });
}

document.addEventListener('DOMContentLoaded', function () {
    const groups = document.querySelectorAll(".condition-group");
    const sortableOptions = {
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
    for (var i = 0; i < groups.length; i++) {
      new Sortable(groups[i], sortableOptions);
    }
});
