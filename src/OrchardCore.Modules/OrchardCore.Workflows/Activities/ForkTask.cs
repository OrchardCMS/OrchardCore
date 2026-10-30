using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities;

public class ForkTask : TaskActivity<ForkTask>, IActivityDescription
{
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public ForkTask(IStringLocalizerFactory localizerFactory, IStringLocalizer<ForkTask> localizer)
    {
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ForkTask>("Fork Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.ControlFlow;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public IList<string> Forks
    {
        get => GetProperty(() => new List<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
#pragma warning disable OCLOC104 // The fork names are entered by the user who designs the workflow.
        => Forks.SelectMany(f => Outcome(S[f]));
#pragma warning restore OCLOC104

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcome(Forks);
    }
}
