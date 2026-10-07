using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Events;

/// <summary>
/// The start activity of a workflow that other workflows run with the Execute Workflow task (see
/// <see cref="WorkflowType.IsActivity"/>). Its input variables are set from the task's inputs.
/// </summary>
public class StartedByWorkflowEvent : EventActivity
{
    protected readonly IStringLocalizer S;

    public StartedByWorkflowEvent(IStringLocalizer<StartedByWorkflowEvent> localizer)
    {
        S = localizer;
    }

    public override string Name => nameof(StartedByWorkflowEvent);

    public override LocalizedString DisplayText => S["Started By Workflow Event"];

    public override LocalizedString Category => S["Primitives"];

    public override bool HasEditor => false;

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome("Done");
}
