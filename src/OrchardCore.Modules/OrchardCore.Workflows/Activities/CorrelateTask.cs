using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class CorrelateTask : TaskActivity<CorrelateTask>, IActivityDescription
{
    private readonly IWorkflowExpressionManager _expressionManager;

    private readonly IStringLocalizerFactory _localizerFactory;

    protected readonly IStringLocalizer S;

    public CorrelateTask(
        IWorkflowExpressionManager expressionManager,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<CorrelateTask> stringLocalizer)
    {
        _expressionManager = expressionManager;
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<CorrelateTask>("Correlate Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Primitives;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    /// <summary>
    /// The value to correlate the workflow instance with.
    /// </summary>
    public WorkflowExpression<string> Value
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: which expression an activity saved before a syntax could be chosen for each expression evaluates.
    /// It's only used when <see cref="Value"/> has no <see cref="WorkflowExpression{T}.Syntax"/>.
    /// </summary>
    public WorkflowScriptSyntax Syntax
    {
        get => GetProperty(() => WorkflowScriptSyntax.JavaScript);
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var value = await _expressionManager.EvaluateAsync(WorkflowExpressionSyntaxes.Resolve(Value, Value.Expression, Syntax), workflowContext);

        workflowContext.CorrelationId = value?.Trim();

        return Outcome("Done");
    }
}
