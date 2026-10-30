using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Sms.Activities;

public class SmsTask : TaskActivity<SmsTask>, IActivityProvidedValues, IActivityDescription
{
    private readonly ISmsService _smsService;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public SmsTask(
        ISmsService smsService,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<SmsTask> stringLocalizer
    )
    {
        _smsService = smsService;
        _expressionEvaluator = expressionEvaluator;
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<SmsTask>("SMS Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Messaging;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

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

    private static readonly ActivityProvidedValue[] s_providedValues = [ActivityProvidedValue.LastResult("object", LocalizationSource.Create<SmsTask>("The result of sending the message."), WorkflowValueMembers.Result)];

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => s_providedValues;

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
