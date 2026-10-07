using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

/// <summary>
/// Sets a workflow variable to the result of a JavaScript or Liquid expression. A declared variable gets the value
/// converted to its type; a value that doesn't convert faults the workflow.
/// </summary>
public class SetVariableTask : TaskActivity<SetVariableTask>
{
    private readonly IWorkflowScriptEvaluator _scriptEvaluator;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

    protected readonly IStringLocalizer S;

    public SetVariableTask(
        IWorkflowScriptEvaluator scriptEvaluator,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<SetVariableTask> localizer)
    {
        _scriptEvaluator = scriptEvaluator;
        _expressionEvaluator = expressionEvaluator;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["Set Variable Task"];

    public override LocalizedString Category => S["Primitives"];

    /// <summary>
    /// The name of the variable to set.
    /// </summary>
    public string VariableName
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// The JavaScript expression of the value, used when <see cref="Syntax"/> is JavaScript.
    /// </summary>
    public WorkflowExpression<object> Value
    {
        get => GetProperty(() => new WorkflowExpression<object>());
        set => SetProperty(value);
    }

    /// <summary>
    /// The Liquid expression of the value, used when <see cref="Syntax"/> is Liquid.
    /// </summary>
    public WorkflowExpression<object> LiquidValue
    {
        get => GetProperty(() => new WorkflowExpression<object>());
        set => SetProperty(value);
    }

    /// <summary>
    /// The syntax of the value.
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
        var value = Syntax switch
        {
            WorkflowScriptSyntax.Liquid => await _expressionEvaluator.EvaluateAsync(LiquidValue, workflowContext, null),
            WorkflowScriptSyntax.JavaScript => await _scriptEvaluator.EvaluateAsync(Value, workflowContext),
            _ => throw new NotSupportedException($"The syntax {Syntax} isn't supported for {nameof(SetVariableTask)}."),
        };

        // Throws when the value doesn't convert to the variable's type, which faults the workflow.
        workflowContext.Variables.Set(VariableName, value);

        return Outcome("Done");
    }
}
