using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Sms.Activities;

public class SmsTask : TaskActivity<SmsTask>
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

    public WorkflowExpression<string> Body
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"], S["Failed"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var message = new SmsMessage
        {
            To = await _expressionEvaluator.EvaluateAsync(PhoneNumber, workflowContext, null),
            Body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, null),
        };

        var result = await _smsService.SendAsync(message);

        workflowContext.LastResult = result;

        if (result.Succeeded)
        {
            return Outcomes("Done");
        }

        return Outcomes("Failed");
    }
}
