using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class SetPropertyTask : TaskActivity<SetPropertyTask>, IActivityOutputs, IActivityProvidedValues, IActivityDescription
{
    private readonly IWorkflowExpressionManager _expressionManager;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public SetPropertyTask(
        IWorkflowExpressionManager expressionManager,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<SetPropertyTask> localizer)
    {
        _expressionManager = expressionManager;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<SetPropertyTask>("Set Property Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Primitives;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public string PropertyName
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// The value.
    /// </summary>
    public WorkflowExpression<object> Value
    {
        get => GetProperty(() => new WorkflowExpression<object>());
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: the Liquid liquidvalue of an activity saved before a syntax could be chosen for each expression, used when
    /// <see cref="Value"/> has no syntax and <see cref="Syntax"/> is Liquid.
    /// </summary>
    public WorkflowExpression<object> LiquidValue
    {
        get => GetProperty(() => new WorkflowExpression<object>());
        set => SetProperty(value);
    }

    /// <summary>
    /// Legacy: which expression an activity saved before a syntax could be chosen for each expression evaluates.
    /// It's only used when <see cref="Value"/> has no <see cref="WorkflowExpression{T}.Syntax"/>.
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
        var value = await _expressionManager.EvaluateAsync(WorkflowExpressionSyntaxes.Resolve(Value, LiquidValue.Expression, Syntax), workflowContext);

        workflowContext.Properties[PropertyName] = value;
        workflowContext.SetActivityOutput(activityContext, "Value", value);

        return Outcome("Done");
    }

    private static readonly ActivityOutputDescriptor[] s_outputs = [new ActivityOutputDescriptor { Name = "Value", TypeName = "any", DisplayName = LocalizationSource.Create<SetPropertyTask>("Value"), Description = LocalizationSource.Create<SetPropertyTask>("The value the task sets the property to.") }];

    public IEnumerable<ActivityOutputDescriptor> GetOutputs()
        => s_outputs;

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => string.IsNullOrEmpty(PropertyName) ? [] : [new ActivityProvidedValue { Source = WorkflowValueSource.Properties, Name = PropertyName, TypeName = "any", Description = LocalizationSource.Create<SetPropertyTask>("The value this activity sets.") }];
}
