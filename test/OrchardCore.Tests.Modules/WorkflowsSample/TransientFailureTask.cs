using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace WorkflowsSample;

/// <summary>
/// Fails the first times it runs in an instance (<see cref="Failures"/>), like a call to a service that is briefly
/// down, and succeeds when it's retried after that.
/// </summary>
public sealed class TransientFailureTask : TaskActivity<TransientFailureTask>
{
    public override LocalizedString DisplayText => new("Transient failure", "Transient failure");

    public override LocalizedString Category => new("Test", "Test");

    /// <summary>
    /// How many times the activity fails before it succeeds, once by default.
    /// </summary>
    public int Failures
    {
        get => GetProperty(() => 1);
        set => SetProperty(value);
    }

    /// <summary>
    /// How many times the activity failed in this instance. It is saved with the instance's state.
    /// </summary>
    public int FailedCount
    {
        get => GetProperty<int>();
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(new LocalizedString("Done", "Done"));

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (FailedCount < Failures)
        {
            FailedCount++;

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
