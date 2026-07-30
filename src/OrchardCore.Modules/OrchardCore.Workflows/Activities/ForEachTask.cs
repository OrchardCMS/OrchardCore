using System.Collections;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class ForEachTask : TaskActivity<ForEachTask>
{
    private readonly IWorkflowScriptEvaluator _scriptEvaluator;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public ForEachTask(
        IWorkflowScriptEvaluator scriptEvaluator,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<ForEachTask> localizer)
    {
        _scriptEvaluator = scriptEvaluator;
        _expressionEvaluator = expressionEvaluator;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["For Each Task"];

    public override LocalizedString Category => S["Control Flow"];

    /// <summary>
    /// An expression evaluating to an enumerable object to iterate over.
    /// </summary>
    public WorkflowExpression<IEnumerable<object>> Enumerable
    {
        get => GetProperty(() => new WorkflowExpression<IEnumerable<object>>());
        set => SetProperty(value);
    }

    public WorkflowExpression<object> LiquidEnumerable
    {
        get => GetProperty(() => new WorkflowExpression<object>());
        set => SetProperty(value);
    }

    public WorkflowScriptSyntax Syntax
    {
        get => GetProperty(() => WorkflowScriptSyntax.JavaScript);
        set => SetProperty(value);
    }

    /// <summary>
    /// The current iteration value.
    /// </summary>
    public string LoopVariableName
    {
        get => GetProperty(() => "x");
        set => SetProperty(value);
    }

    /// <summary>
    /// The current iteration value.
    /// </summary>
    public object Current
    {
        get => GetProperty<object>();
        set => SetProperty(value);
    }

    /// <summary>
    /// The current number of iterations executed.
    /// </summary>
    public int Index
    {
        get => GetProperty(() => 0);
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Iterate"], S["Done"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var items = Syntax switch
        {
            WorkflowScriptSyntax.Liquid => await EvaluateLiquidEnumerableAsync(workflowContext),
            WorkflowScriptSyntax.JavaScript => (await _scriptEvaluator.EvaluateAsync(Enumerable, workflowContext)).ToList(),
            _ => throw new NotSupportedException($"The syntax {Syntax} isn't supported for ForEachTask.")
        };

        var count = items.Count;

        if (Index < count)
        {
            var current = Current = items[Index];

            // TODO: Implement nested scopes. See https://github.com/OrchardCMS/OrchardCore/projects/4#card-6992776
            workflowContext.Properties[LoopVariableName] = current;
            workflowContext.LastResult = current;
            Index++;
            return Outcomes("Iterate");
        }
        else
        {
            Index = 0;
            return Outcomes("Done");
        }
    }

    private async Task<List<object>> EvaluateLiquidEnumerableAsync(WorkflowExecutionContext workflowContext)
    {
        var result = await _expressionEvaluator.EvaluateAsync(LiquidEnumerable, workflowContext, null);

        if (result == null)
        {
            return [];
        }

        if (result is IEnumerable enumerable and not string)
        {
            return enumerable.Cast<object>().ToList();
        }

        if (result is not string stringResult || string.IsNullOrWhiteSpace(stringResult))
        {
            return [result];
        }

        var trimmed = stringResult.Trim();

        if (trimmed.StartsWith('['))
        {
            return JsonSerializer.Deserialize<List<object>>(trimmed) ?? [];
        }

        return trimmed
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Cast<object>()
            .ToList();
    }
}
