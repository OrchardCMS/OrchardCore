using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class LiquidTask : TaskActivity<LiquidTask>, IActivityOutputs, IActivityProvidedValues
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
        => Outcome(S["Done"]);

    private static readonly ActivityProvidedValue[] s_providedValues = [ActivityProvidedValue.LastResult("string", LocalizationSource.Create<LiquidTask>("The rendered template."))];

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => s_providedValues;

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        workflowContext.LastResult = await _expressionEvaluator.EvaluateAsync(Expression, workflowContext, null);
        workflowContext.SetActivityOutput(activityContext, "Result", workflowContext.LastResult);

        return Outcome("Done");
    }

    private static readonly ActivityOutputDescriptor[] s_outputs = [new ActivityOutputDescriptor { Name = "Result", TypeName = "string", DisplayName = LocalizationSource.Create<LiquidTask>("Result"), Description = LocalizationSource.Create<LiquidTask>("The text the template renders.") }];

    public IEnumerable<ActivityOutputDescriptor> GetOutputs()
        => s_outputs;
}
