using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class CorrelateTask : TaskActivity<CorrelateTask>
{
    private readonly IWorkflowScriptEvaluator _scriptEvaluator;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

    protected readonly IStringLocalizer S;

    public CorrelateTask(
        IWorkflowScriptEvaluator scriptEvaluator,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<CorrelateTask> stringLocalizer)
    {
        _scriptEvaluator = scriptEvaluator;
        _expressionEvaluator = expressionEvaluator;
        S = stringLocalizer;
    }

    public override LocalizedString DisplayText => S["Correlate Task"];

    public override LocalizedString Category => S["Primitives"];

    public WorkflowExpression<string> Value
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowScriptSyntax Syntax
    {
        get => GetProperty(() => WorkflowScriptSyntax.JavaScript);
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var value = Syntax switch
        {
            WorkflowScriptSyntax.Liquid => await _expressionEvaluator.EvaluateAsync(Value, workflowContext, null),
            WorkflowScriptSyntax.JavaScript => await _scriptEvaluator.EvaluateAsync(Value, workflowContext, null),
            _ => throw new NotSupportedException($"The syntax {Syntax} isn't supported for CorrelateTask.")
        };

        workflowContext.CorrelationId = value?.Trim();

        return Outcomes("Done");
    }
}
