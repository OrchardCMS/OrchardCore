using System.Collections;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class ForEachTask : TaskActivity<ForEachTask>, IActivityProvidedValues
{
    private readonly IWorkflowExpressionManager _expressionManager;
    protected readonly IStringLocalizer S;

    public ForEachTask(
        IWorkflowExpressionManager expressionManager,
        IStringLocalizer<ForEachTask> localizer)
    {
        _expressionManager = expressionManager;
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

    /// <summary>
    /// Legacy: the Liquid liquidenumerable of an activity saved before a syntax could be chosen for each expression, used when
    /// <see cref="Enumerable"/> has no syntax and <see cref="Syntax"/> is Liquid.
    /// </summary>
    public WorkflowExpression<object> LiquidEnumerable
    {
        get => GetProperty(() => new WorkflowExpression<object>());
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: which expression an activity saved before a syntax could be chosen for each expression evaluates.
    /// It's only used when <see cref="Enumerable"/> has no <see cref="WorkflowExpression{T}.Syntax"/>.
    /// </summary>
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
        => Outcome(S["Iterate"], S["Done"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var items = await EvaluateItemsAsync(workflowContext);

        var count = items.Count;

        if (Index < count)
        {
            var current = Current = items[Index];

            // TODO: Implement nested scopes. See https://github.com/OrchardCMS/OrchardCore/projects/4#card-6992776
            workflowContext.Properties[LoopVariableName] = current;
            workflowContext.LastResult = current;
            Index++;

            return Outcome("Iterate");
        }
        else
        {
            Index = 0;

            return Outcome("Done");
        }
    }

    private async Task<List<object>> EvaluateItemsAsync(WorkflowExecutionContext workflowContext)
    {
        var expression = WorkflowExpressionSyntaxes.Resolve(Enumerable, LiquidEnumerable.Expression, Syntax);

        // A Liquid template renders its list as JSON or comma-separated text.
        if (string.Equals(expression.Syntax, WorkflowExpressionSyntaxes.Liquid, StringComparison.OrdinalIgnoreCase))
        {
            return ToList(await _expressionManager.EvaluateAsync(new WorkflowExpression<object>(expression.Expression, expression.Syntax), workflowContext));
        }

        return (await _expressionManager.EvaluateAsync(expression, workflowContext))?.ToList() ?? [];
    }

    private static List<object> ToList(object result)
    {
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

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
    {
        var lastResult = ActivityProvidedValue.LastResult("any", S["The current item of the loop."]);

        return string.IsNullOrEmpty(LoopVariableName)
            ? [lastResult]
            : [new ActivityProvidedValue { Source = WorkflowValueSource.Properties, Name = LoopVariableName, TypeName = "any", Description = S["The current item of the loop."] }, lastResult];
    }
}
