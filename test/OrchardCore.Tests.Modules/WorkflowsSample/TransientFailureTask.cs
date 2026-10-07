using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace WorkflowsSample;

/// <summary>
/// Fails the first time it runs in an instance, like a call to a service that is briefly down, and succeeds when
/// the instance is retried.
/// </summary>
public sealed class TransientFailureTask : TaskActivity<TransientFailureTask>
{
    public override LocalizedString DisplayText => new("Transient failure", "Transient failure");

    public override LocalizedString Category => new("Test", "Test");

    /// <summary>
    /// Whether the activity already failed in this instance. It is saved with the instance's state.
    /// </summary>
    public bool HasFailed
    {
        get => GetProperty<bool>();
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(new LocalizedString("Done", "Done"));

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (!HasFailed)
        {
            HasFailed = true;

            throw new TransientFailureException();
        }

        return Outcome("Done");
    }
}

/// <summary>
/// The failure of <see cref="TransientFailureTask"/>.
/// </summary>
public sealed class TransientFailureException : InvalidOperationException
{
    public TransientFailureException()
        : base("The service is briefly unavailable.")
    {
    }
}
