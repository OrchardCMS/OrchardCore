using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities;

public class ForkTask : TaskActivity<ForkTask>
{
    protected readonly IStringLocalizer S;

    public ForkTask(IStringLocalizer<ForkTask> localizer)
    {
        S = localizer;
    }

    public override LocalizedString DisplayText => S["Fork Task"];

    public override LocalizedString Category => S["Control Flow"];

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
