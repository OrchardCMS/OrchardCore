using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Events;

/// <summary>
/// The start activity of a workflow that other workflows run with the Execute Workflow task (see
/// <see cref="WorkflowType.IsActivity"/>). Its input variables are set from the task's inputs.
/// </summary>
public class StartedByWorkflowEvent : EventActivity, IActivityDescription
{
    private readonly IStringLocalizerFactory _localizerFactory;

    protected readonly IStringLocalizer S;

    public StartedByWorkflowEvent(
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<StartedByWorkflowEvent> localizer)
    {
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<StartedByWorkflowEvent>("Started By Workflow Event");

    public static LocalizationSource CategoryName => WorkflowsLocalizationSources.Primitives;

    public override string Name => nameof(StartedByWorkflowEvent);

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public override bool HasEditor => false;

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome("Done");
}
