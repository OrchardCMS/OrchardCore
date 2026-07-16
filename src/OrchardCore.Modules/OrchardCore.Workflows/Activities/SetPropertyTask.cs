using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class SetPropertyTask : TaskActivity<SetPropertyTask>
{
    private readonly IWorkflowScriptEvaluator _scriptEvaluator;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public SetPropertyTask(
        IWorkflowScriptEvaluator scriptEvaluator,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<SetPropertyTask> localizer)
    {
        _scriptEvaluator = scriptEvaluator;
        _expressionEvaluator = expressionEvaluator;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["Set Property Task"];

    public override LocalizedString Category => S["Primitives"];

    public string PropertyName
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
            _ => throw new NotSupportedException($"The syntax {Syntax} isn't supported for SetPropertyTask.")
        };

        workflowContext.Properties[PropertyName] = value;

        return Outcomes("Done");
    }
}
