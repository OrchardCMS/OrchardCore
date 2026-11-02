using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Activities;

public class SignalEvent : EventActivity, IActivityProvidedValues, IActivityDescription
{
    public static string EventName => nameof(SignalEvent);
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public SignalEvent(IWorkflowExpressionEvaluator expressionEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<SignalEvent> localizer)
    {
        _expressionEvaluator = expressionEvaluator;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public override string Name => EventName;
    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<SignalEvent>("Signal Event");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Http;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);
    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public WorkflowExpression<string> SignalName
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override async Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var signalName = await _expressionEvaluator.EvaluateAsync(SignalName, workflowContext, null);
        return string.Equals(workflowContext.Input.GetValue<string>("Signal"), signalName, StringComparison.OrdinalIgnoreCase);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome("Done");

    private static readonly ActivityProvidedValue[] s_providedValues = [new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "Signal", TypeName = "string", Description = LocalizationSource.Create<SignalEvent>("The name of the signal.") }];

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => s_providedValues;
}
