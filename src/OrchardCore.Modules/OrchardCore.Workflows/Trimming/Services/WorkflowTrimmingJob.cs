using Microsoft.Extensions.Options;
using OrchardCore.Documents;
using OrchardCore.Jobs;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Workflows.Trimming.Models;

namespace OrchardCore.Workflows.Trimming.Services;

/// <summary>
/// Deletes the workflow instances older than the retention period of the workflow trimming settings. It runs every
/// day by default, once whatever the number of servers.
/// </summary>
public sealed class WorkflowTrimmingJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "WorkflowTrimming";

    private readonly ISiteService _siteService;
    private readonly IWorkflowTrimmingService _trimmingService;
    private readonly IDocumentManager<WorkflowTrimmingState> _stateManager;
    private readonly WorkflowTrimmingOptions _options;
    private readonly IClock _clock;

    public WorkflowTrimmingJob(
        ISiteService siteService,
        IWorkflowTrimmingService trimmingService,
        IDocumentManager<WorkflowTrimmingState> stateManager,
        IOptions<WorkflowTrimmingOptions> options,
        IClock clock)
    {
        _siteService = siteService;
        _trimmingService = trimmingService;
        _stateManager = stateManager;
        _options = options.Value;
        _clock = clock;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var settings = await _siteService.GetSettingsAsync<WorkflowTrimmingSettings>();

        if (settings.Disabled)
        {
            context.LogInformation("The trimming is disabled in the workflow trimming settings.");

            return;
        }

        var trimmed = await _trimmingService.TrimWorkflowInstancesAsync(TimeSpan.FromDays(settings.RetentionDays), _options.BatchSize);

        var state = await _stateManager.GetOrCreateMutableAsync();
        state.LastRunUtc = _clock.UtcNow;
        await _stateManager.UpdateAsync(state);

        context.LogInformation($"{trimmed} workflow instances older than {settings.RetentionDays} days were deleted.");
        context.SetResult(new { DeletedInstances = trimmed });
    }
}
