using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

/// <summary>
/// Sets a workflow variable to the result of a JavaScript or Liquid expression. A declared variable gets the value
/// converted to its type; a value that doesn't convert faults the workflow.
/// </summary>
public class SetVariableTask : TaskActivity<SetVariableTask>, IActivityDescription
{
    private readonly IWorkflowExpressionManager _expressionManager;
    private readonly IStringLocalizerFactory _localizerFactory;

    protected readonly IStringLocalizer S;

    public SetVariableTask(
        IWorkflowExpressionManager expressionManager,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<SetVariableTask> localizer)
    {
        _expressionManager = expressionManager;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<SetVariableTask>("Set Variable Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Primitives;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    /// <summary>
    /// The name of the variable to set.
    /// </summary>
    public string VariableName
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// The value.
    /// </summary>
    public WorkflowExpression<object> Value
    {
        get => GetProperty(() => new WorkflowExpression<object>());
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: the Liquid liquidvalue of an activity saved before a syntax could be chosen for each expression, used when
    /// <see cref="Value"/> has no syntax and <see cref="Syntax"/> is Liquid.
    /// </summary>
    public WorkflowExpression<object> LiquidValue
    {
        get => GetProperty(() => new WorkflowExpression<object>());
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
        var value = await _expressionManager.EvaluateAsync(WorkflowExpressionSyntaxes.Resolve(Value, LiquidValue.Expression, Syntax), workflowContext);

        // Throws when the value doesn't convert to the variable's type, which faults the workflow.
        workflowContext.Variables.Set(VariableName, value);

        return Outcome("Done");
    }
}
