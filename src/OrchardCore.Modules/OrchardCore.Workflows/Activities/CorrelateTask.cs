using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class CorrelateTask : TaskActivity<CorrelateTask>
{
    private readonly IWorkflowExpressionManager _expressionManager;

    protected readonly IStringLocalizer S;

    public CorrelateTask(
        IWorkflowExpressionManager expressionManager,
        IStringLocalizer<CorrelateTask> stringLocalizer)
    {
        _expressionManager = expressionManager;
        S = stringLocalizer;
    }

    public override LocalizedString DisplayText => S["Correlate Task"];

    public override LocalizedString Category => S["Primitives"];

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
