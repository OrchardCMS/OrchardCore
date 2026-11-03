using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.DataPipelines.Workflows;

/// <summary>
/// Starts or resumes a workflow when a run of a data pipeline ends, such as to notify someone when a run fails. The run
/// is the last result.
/// </summary>
public sealed class DataPipelineRunCompletedEvent : EventActivity, IActivityDescription
{
    private readonly IStringLocalizerFactory _localizerFactory;
    private readonly IStringLocalizer S;

    public DataPipelineRunCompletedEvent(IStringLocalizerFactory localizerFactory, IStringLocalizer<DataPipelineRunCompletedEvent> localizer)
    {
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public override string Name => nameof(DataPipelineRunCompletedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<DataPipelineRunCompletedEvent>("Data Pipeline Run Completed Event");

    public static LocalizationSource CategoryName => DataPipelinesLocalizationSources.Data;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    /// <summary>
    /// Gets or sets the pipeline whose runs trigger the event, or <see langword="null"/> for every pipeline.
    /// </summary>
    public string PipelineId
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the status of the runs that trigger the event: <c>Succeeded</c>, <c>Failed</c>, or empty for
    /// every run.
    /// </summary>
    public string Status
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public override Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (!workflowContext.Input.TryGetValue(RunDataPipelineTask.RunKey, out var value) || value is not IDictionary<string, object> run)
        {
            return Task.FromResult(false);
        }

        var pipelineMatches = string.IsNullOrEmpty(PipelineId) || Equals(run.TryGetValue("PipelineId", out var pipelineId) ? pipelineId : null, PipelineId);
        var statusMatches = string.IsNullOrEmpty(Status) ||
            (Status == "Failed"
                ? run.TryGetValue("Status", out var failed) && failed?.ToString() is "Failed" or "Cancelled"
                : run.TryGetValue("Status", out var status) && status?.ToString() == Status);

        return Task.FromResult(pipelineMatches && statusMatches);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcome(S["Done"]);
    }

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        workflowContext.LastResult = workflowContext.Input.TryGetValue(RunDataPipelineTask.RunKey, out var run) ? run : null;

        return Outcome("Done");
    }
}
