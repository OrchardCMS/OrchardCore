using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class ForLoopTask : TaskActivity<ForLoopTask>, IActivityProvidedValues
{
    private readonly IWorkflowExpressionManager _expressionManager;
    protected readonly IStringLocalizer S;

    public ForLoopTask(
        IWorkflowExpressionManager expressionManager,
        IStringLocalizer<ForLoopTask> localizer)
    {
        _expressionManager = expressionManager;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["For Loop Task"];

    public override LocalizedString Category => S["Control Flow"];

    /// <summary>
    /// An expression evaluating to the start value.
    /// </summary>
    public WorkflowExpression<double> From
    {
        get => GetProperty(() => new WorkflowExpression<double>("0"));
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: the Liquid liquidfrom of an activity saved before a syntax could be chosen for each expression, used when
    /// <see cref="From"/> has no syntax and <see cref="Syntax"/> is Liquid.
    /// </summary>
    public WorkflowExpression<string> LiquidFrom
    {
        get => GetProperty(() => new WorkflowExpression<string>("0"));
        set => SetProperty(value);
    }

    /// <summary>
    /// An expression evaluating to the end value.
    /// </summary>
    public WorkflowExpression<double> To
    {
        get => GetProperty(() => new WorkflowExpression<double>("10"));
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: the Liquid liquidto of an activity saved before a syntax could be chosen for each expression, used when
    /// <see cref="To"/> has no syntax and <see cref="Syntax"/> is Liquid.
    /// </summary>
    public WorkflowExpression<string> LiquidTo
    {
        get => GetProperty(() => new WorkflowExpression<string>("10"));
        set => SetProperty(value);
    }

    /// <summary>
    /// An expression evaluating to the end value.
    /// </summary>
    public WorkflowExpression<double> Step
    {
        get => GetProperty(() => new WorkflowExpression<double>("1"));
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: the Liquid liquidstep of an activity saved before a syntax could be chosen for each expression, used when
    /// <see cref="Step"/> has no syntax and <see cref="Syntax"/> is Liquid.
    /// </summary>
    public WorkflowExpression<string> LiquidStep
    {
        get => GetProperty(() => new WorkflowExpression<string>("1"));
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: which expression an activity saved before a syntax could be chosen for each expression evaluates.
    /// It's only used when <see cref="From"/>, <see cref="To"/> or <see cref="Step"/> has no <see cref="WorkflowExpression{T}.Syntax"/>.
    /// </summary>
    public WorkflowScriptSyntax Syntax
    {
        get => GetProperty(() => WorkflowScriptSyntax.JavaScript);
        set => SetProperty(value);
    }

    /// <summary>
    /// The property name to store the current iteration number in.
    /// </summary>
    public string LoopVariableName
    {
        get => GetProperty(() => "x");
        set => SetProperty(value);
    }

    /// <summary>
    /// The current index of the iteration.
    /// </summary>
    public double Index
    {
        get => GetProperty(() => 0);
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Iterate"], S["Done"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var from = await EvaluateFromAsync(workflowContext);
        var to = await EvaluateToAsync(workflowContext);
        var step = await EvaluateStepAsync(workflowContext);

        if (Index < from)
        {
            Index = from;
        }

        if (Index < to)
        {
            workflowContext.LastResult = Index;
            workflowContext.Properties[LoopVariableName] = Index;
            Index += step;

            return Outcome("Iterate");
        }
        else
        {
            Index = from;

            return Outcome("Done");
        }
    }

    private Task<double> EvaluateFromAsync(WorkflowExecutionContext workflowContext)
        => EvaluateNumberAsync(From, LiquidFrom, workflowContext);

    private Task<double> EvaluateToAsync(WorkflowExecutionContext workflowContext)
        => EvaluateNumberAsync(To, LiquidTo, workflowContext);

    private Task<double> EvaluateStepAsync(WorkflowExecutionContext workflowContext)
        => EvaluateNumberAsync(Step, LiquidStep, workflowContext);

    private async Task<double> EvaluateNumberAsync(WorkflowExpression<double> expression, WorkflowExpression<string> legacyLiquid, WorkflowExecutionContext workflowContext)
    {
        if (!string.IsNullOrEmpty(expression.Syntax))
        {
            return await _expressionManager.EvaluateAsync(expression, workflowContext);
        }

        // An activity saved before a syntax could be chosen for each expression.
        if (Syntax == WorkflowScriptSyntax.Liquid)
        {
            return double.Parse(await _expressionManager.EvaluateAsync(new WorkflowExpression<string>(legacyLiquid.Expression, WorkflowExpressionSyntaxes.Liquid), workflowContext));
        }

        return double.TryParse(expression.Expression, out var number)
            ? number
            : await _expressionManager.EvaluateAsync(expression, workflowContext, WorkflowExpressionSyntaxes.JavaScript);
    }

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => string.IsNullOrEmpty(LoopVariableName) ? [] : [new ActivityProvidedValue { Source = WorkflowValueSource.Properties, Name = LoopVariableName, TypeName = "number", Description = S["The current index of the loop."] }];
}
