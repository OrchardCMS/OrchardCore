using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class SetOutputTask : TaskActivity<SetOutputTask>
{
    private readonly IWorkflowExpressionManager _expressionManager;
    protected readonly IStringLocalizer S;

    public SetOutputTask(
        IWorkflowExpressionManager expressionManager,
        IStringLocalizer<SetOutputTask> localizer)
    {
        _expressionManager = expressionManager;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["Set Output Task"];

    public override LocalizedString Category => S["Primitives"];

    public string OutputName
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

        workflowContext.Output[OutputName] = value;

        return Outcome("Done");
    }
}
