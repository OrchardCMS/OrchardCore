using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class ForLoopTask : TaskActivity<ForLoopTask>
{
    private readonly IWorkflowScriptEvaluator _scriptEvaluator;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public ForLoopTask(
        IWorkflowScriptEvaluator scriptEvaluator,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<ForLoopTask> localizer)
    {
        _scriptEvaluator = scriptEvaluator;
        _expressionEvaluator = expressionEvaluator;
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

    public WorkflowExpression<string> LiquidStep
    {
        get => GetProperty(() => new WorkflowExpression<string>("1"));
        set => SetProperty(value);
    }

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
    {
        return Outcomes(S["Iterate"], S["Done"]);
    }

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
            return Outcomes("Iterate");
        }
        else
        {
            Index = from;
            return Outcomes("Done");
        }
    }

    private async Task<double> EvaluateFromAsync(WorkflowExecutionContext workflowContext)
    {
        return Syntax switch
        {
            WorkflowScriptSyntax.Liquid => double.Parse(await _expressionEvaluator.EvaluateAsync(LiquidFrom, workflowContext, null)),
            WorkflowScriptSyntax.JavaScript when double.TryParse(From.Expression, out var from) => from,
            WorkflowScriptSyntax.JavaScript => await _scriptEvaluator.EvaluateAsync(From, workflowContext),
            _ => throw new NotSupportedException($"The syntax {Syntax} isn't supported for ForLoopTask.")
        };
    }

    private async Task<double> EvaluateToAsync(WorkflowExecutionContext workflowContext)
    {
        return Syntax switch
        {
            WorkflowScriptSyntax.Liquid => double.Parse(await _expressionEvaluator.EvaluateAsync(LiquidTo, workflowContext, null)),
            WorkflowScriptSyntax.JavaScript when double.TryParse(To.Expression, out var to) => to,
            WorkflowScriptSyntax.JavaScript => await _scriptEvaluator.EvaluateAsync(To, workflowContext),
            _ => throw new NotSupportedException($"The syntax {Syntax} isn't supported for ForLoopTask.")
        };
    }

    private async Task<double> EvaluateStepAsync(WorkflowExecutionContext workflowContext)
    {
        return Syntax switch
        {
            WorkflowScriptSyntax.Liquid => double.Parse(await _expressionEvaluator.EvaluateAsync(LiquidStep, workflowContext, null)),
            WorkflowScriptSyntax.JavaScript when double.TryParse(Step.Expression, out var step) => step,
            WorkflowScriptSyntax.JavaScript => await _scriptEvaluator.EvaluateAsync(Step, workflowContext),
            _ => throw new NotSupportedException($"The syntax {Syntax} isn't supported for ForLoopTask.")
        };
    }
}
