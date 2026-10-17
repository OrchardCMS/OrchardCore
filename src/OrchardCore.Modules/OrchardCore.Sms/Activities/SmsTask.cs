using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Sms.Activities;

public class SmsTask : TaskActivity<SmsTask>, IActivityProvidedValues
{
    private readonly ISmsService _smsService;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public SmsTask(
        ISmsService smsService,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<SmsTask> stringLocalizer
    )
    {
        _smsService = smsService;
        _expressionEvaluator = expressionEvaluator;
        S = stringLocalizer;
    }

    public override LocalizedString DisplayText => S["SMS Task"];

    public override LocalizedString Category => S["Messaging"];

    public WorkflowExpression<string> PhoneNumber
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> FromNumber
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> Body
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"], S["Failed"]);

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => [ActivityProvidedValue.LastResult("object", S["The result of sending the message."], WorkflowValueMembers.Result(S))];

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var message = new SmsMessage
        {
            To = await _expressionEvaluator.EvaluateAsync(PhoneNumber, workflowContext, null),
            From = await _expressionEvaluator.EvaluateAsync(FromNumber, workflowContext, null),
            Body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, null),
        };

        var result = await _smsService.SendAsync(message, workflowContext.CancellationToken);

        workflowContext.LastResult = result;

        if (result.Succeeded)
        {
            return Outcome("Done");
        }

        return Outcome("Failed");
    }
}
