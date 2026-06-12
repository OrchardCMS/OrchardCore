using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class SetOutputTask : TaskActivity<SetOutputTask>
{
    private readonly IWorkflowScriptEvaluator _scriptEvaluator;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public SetOutputTask(
        IWorkflowScriptEvaluator scriptEvaluator,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<SetOutputTask> localizer)
    {
        _scriptEvaluator = scriptEvaluator;
        _expressionEvaluator = expressionEvaluator;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["Set Output Task"];

    public override LocalizedString Category => S["Primitives"];

    public string OutputName
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public WorkflowExpression<object> Value
    {
        get => GetProperty(() => new WorkflowExpression<object>());
        set => SetProperty(value);
    }

    public WorkflowExpression<object> LiquidValue
    {
        get => GetProperty(() => new WorkflowExpression<object>());
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
            WorkflowScriptSyntax.Liquid => await _expressionEvaluator.EvaluateAsync(LiquidValue, workflowContext, null),
            WorkflowScriptSyntax.JavaScript => await _scriptEvaluator.EvaluateAsync(Value, workflowContext),
            _ => throw new NotSupportedException($"The syntax {Syntax} isn't supported for SetOutputTask.")
        };

        workflowContext.Output[OutputName] = value;

        return Outcomes("Done");
    }
}
