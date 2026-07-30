using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class LiquidTask : TaskActivity<LiquidTask>
{
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public LiquidTask(
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<LiquidTask> localizer)
    {
        _expressionEvaluator = expressionEvaluator;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["Liquid Task"];

    public override LocalizedString Category => S["Control Flow"];

    public WorkflowExpression<object> Expression
    {
        get => GetProperty(() => new WorkflowExpression<object>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        workflowContext.LastResult = await _expressionEvaluator.EvaluateAsync(Expression, workflowContext, null);

        return Outcomes("Done");
    }
}
