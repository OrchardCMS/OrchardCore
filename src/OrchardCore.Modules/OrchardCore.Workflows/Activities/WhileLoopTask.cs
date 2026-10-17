using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class WhileLoopTask : TaskActivity<WhileLoopTask>
{
    private readonly IWorkflowExpressionManager _expressionManager;
    protected readonly IStringLocalizer S;

    public WhileLoopTask(
        IWorkflowExpressionManager expressionManager,
        IStringLocalizer<WhileLoopTask> localizer)
    {
        _expressionManager = expressionManager;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["While Loop Task"];

    public override LocalizedString Category => S["Control Flow"];

    /// <summary>
    /// An expression evaluating to true or false.
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
        => Outcome(S["Iterate"], S["Done"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var loop = await _expressionManager.EvaluateAsync(WorkflowExpressionSyntaxes.Resolve(Condition, LiquidCondition.Expression, Syntax), workflowContext);

        return Outcome(loop ? "Iterate" : "Done");
    }
}
