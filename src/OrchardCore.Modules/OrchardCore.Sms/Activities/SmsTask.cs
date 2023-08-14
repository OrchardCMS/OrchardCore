using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Sms.Activities;

public class SmsTask : TaskActivity
{
    private readonly ISmsProviderFactory _smsProviderFactory;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly HtmlEncoder _htmlEncoder;
    protected readonly IStringLocalizer S;

    public SmsTask(
        ISmsProviderFactory smsProviderFactory,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<SmsTask> localizer,
        HtmlEncoder htmlEncoder
    )
    {
        _smsProviderFactory = smsProviderFactory;
        _expressionEvaluator = expressionEvaluator;
        S = localizer;
        _htmlEncoder = htmlEncoder;
    }

    public override string Name => nameof(SmsTask);
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

        var smsProvider = await _smsProviderFactory.CreateAsync();
        var result = await smsProvider.SendAsync(message);
        workflowContext.LastResult = result;

        if (result.Succeeded)
        {
            return Outcomes("Done");
        }

        return Outcomes("Failed");
    }
}
