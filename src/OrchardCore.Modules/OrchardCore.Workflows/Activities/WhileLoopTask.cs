using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class WhileLoopTask : TaskActivity<WhileLoopTask>
{
    private readonly IWorkflowScriptEvaluator _scriptEvaluator;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public WhileLoopTask(
        IWorkflowScriptEvaluator scriptEvaluator,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<WhileLoopTask> localizer)
    {
        _scriptEvaluator = scriptEvaluator;
        _expressionEvaluator = expressionEvaluator;
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

    public WorkflowExpression<bool> LiquidCondition
    {
        get => GetProperty(() => new WorkflowExpression<bool>());
        set => SetProperty(value);
    }

    public WorkflowScriptSyntax Syntax
    {
        get => GetProperty(() => WorkflowScriptSyntax.JavaScript);
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Iterate"], S["Done"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var loop = Syntax switch
        {
            WorkflowScriptSyntax.Liquid => await _expressionEvaluator.EvaluateAsync(LiquidCondition, workflowContext, null),
            WorkflowScriptSyntax.JavaScript => await _scriptEvaluator.EvaluateAsync(Condition, workflowContext),
            _ => throw new NotSupportedException($"The syntax {Syntax} isn't supported for WhileLoopTask.")
        };

        return Outcomes(loop ? "Iterate" : "Done");
    }
}
