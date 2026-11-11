using Microsoft.Extensions.Localization;
using OrchardCore.Jobs.Models;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Jobs;

/// <summary>
/// Starts or resumes a workflow when a run of a job ends, such as to notify someone when a run fails. The run is the
/// last result.
/// </summary>
public sealed class JobRunCompletedEvent : EventActivity, IActivityDescription
{
    /// <summary>
    /// The value of <see cref="Status"/> for the runs that failed or were canceled.
    /// </summary>
    public const string FailedStatus = "Failed";

    private readonly IStringLocalizerFactory _localizerFactory;
    private readonly IStringLocalizer S;

    public JobRunCompletedEvent(IStringLocalizerFactory localizerFactory, IStringLocalizer<JobRunCompletedEvent> localizer)
    {
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public override string Name => nameof(JobRunCompletedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<JobRunCompletedEvent>("Job Run Completed Event");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Background;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    /// <summary>
    /// Gets or sets the name of the job whose runs trigger the event, or <see langword="null"/> for every job.
    /// </summary>
    public string JobName
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the status of the runs that trigger the event: <c>Succeeded</c>, <c>Failed</c> for the runs that
    /// failed or were canceled, or empty for every run.
    /// </summary>
    public string Status
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public override Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (!workflowContext.Input.TryGetValue(RunJobTask.RunKey, out var value) || value is not IDictionary<string, object> run)
        {
            return Task.FromResult(false);
        }

        var jobName = run.TryGetValue("JobName", out var name) ? name?.ToString() : null;
        var status = run.TryGetValue("Status", out var runStatus) ? runStatus?.ToString() : null;

        var jobMatches = string.IsNullOrEmpty(JobName) || string.Equals(jobName, JobName, StringComparison.OrdinalIgnoreCase);
        var statusMatches = string.IsNullOrEmpty(Status) ||
            (Status == FailedStatus
                ? status is nameof(JobRunStatus.Failed) or nameof(JobRunStatus.Canceled)
                : status == Status);

        return Task.FromResult(jobMatches && statusMatches);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcome(S["Done"]);
    }

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        workflowContext.LastResult = workflowContext.Input.TryGetValue(RunJobTask.RunKey, out var run) ? run : null;

        return Outcome("Done");
    }
}
