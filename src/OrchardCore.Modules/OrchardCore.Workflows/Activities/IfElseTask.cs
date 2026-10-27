using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class IfElseTask : TaskActivity<IfElseTask>, IActivityDescription
{
    private readonly IWorkflowExpressionManager _expressionManager;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public IfElseTask(
        IWorkflowExpressionManager expressionManager,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<IfElseTask> localizer)
    {
        _expressionManager = expressionManager;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<IfElseTask>("If Else Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.ControlFlow;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    /// <summary>
    /// The condition: an expression evaluating to true or false.
    /// </summary>
    public WorkflowExpression<bool> Condition
    {
        get => GetProperty(() => new WorkflowExpression<bool>());
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: the Liquid liquidcondition of an activity saved before a syntax could be chosen for each expression, used when
    /// <see cref="Condition"/> has no syntax and <see cref="Syntax"/> is Liquid.
    /// </summary>
    public WorkflowExpression<bool> LiquidCondition
    {
        get => GetProperty(() => new WorkflowExpression<bool>());
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: which expression an activity saved before a syntax could be chosen for each expression evaluates.
    /// It's only used when <see cref="Condition"/> has no <see cref="WorkflowExpression{T}.Syntax"/>.
    /// </summary>
    public WorkflowScriptSyntax Syntax
    {
        get => GetProperty(() => WorkflowScriptSyntax.JavaScript);
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["True"], S["False"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var result = await _expressionManager.EvaluateAsync(WorkflowExpressionSyntaxes.Resolve(Condition, LiquidCondition.Expression, Syntax), workflowContext);

        return Outcome(result ? "True" : "False");
    }
}
