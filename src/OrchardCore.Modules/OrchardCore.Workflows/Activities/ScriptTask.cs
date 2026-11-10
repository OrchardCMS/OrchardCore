using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Scripting;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class ScriptTask : TaskActivity<ScriptTask>, IActivityOutputs, IActivityProvidedValues, IActivityDescription
{
    private readonly IWorkflowScriptEvaluator _scriptEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public ScriptTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<ScriptTask> localizer)
    {
        _scriptEvaluator = scriptEvaluator;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ScriptTask>("Script Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.ControlFlow;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public IList<string> AvailableOutcomes
    {
        get => GetProperty(() => new List<string> { "Done" });
        set => SetProperty(value);
    }

    /// <summary>
    /// The script can call any available functions, including setOutcome().
    /// </summary>
    public WorkflowExpression<object> Script
    {
        get => GetProperty(() => new WorkflowExpression<object>("setOutcome('Done');"));
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
#pragma warning disable OCLOC104 // The outcome names are entered by the user who designs the workflow.
        => Outcome(AvailableOutcomes.Select(x => S[x]).ToArray());
#pragma warning restore OCLOC104

    private static readonly ActivityProvidedValue[] s_providedValues = [ActivityProvidedValue.LastResult("any", LocalizationSource.Create<ScriptTask>("The value the script returns."))];

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => s_providedValues;

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var outcomes = new List<string>();
        workflowContext.LastResult = await _scriptEvaluator.EvaluateAsync(Script, workflowContext, new OutcomeMethodProvider(outcomes));
        workflowContext.SetActivityOutput(activityContext, "Result", workflowContext.LastResult);

        return Outcome(outcomes);
    }

    private static readonly ActivityOutputDescriptor[] s_outputs = [new ActivityOutputDescriptor { Name = "Result", TypeName = "any", DisplayName = LocalizationSource.Create<ScriptTask>("Result"), Description = LocalizationSource.Create<ScriptTask>("The value the script returns: its last statement.") }];

    public IEnumerable<ActivityOutputDescriptor> GetOutputs()
        => s_outputs;
}
