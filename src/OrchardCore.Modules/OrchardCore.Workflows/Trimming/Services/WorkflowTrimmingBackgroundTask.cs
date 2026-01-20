using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Documents;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Workflows.Trimming.Models;

namespace OrchardCore.Workflows.Trimming.Services;

[BackgroundTask(
    Schedule = "0 0 * * *",
    Title = "Workflow Trimming Background Task",
    Description = "Regularly deletes old workflow instances.",
    LockTimeout = 3_000,
    LockExpiration = 30_000
)]
public class WorkflowTrimmingBackgroundTask : IBackgroundTask
{
    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var siteService = serviceProvider.GetRequiredService<ISiteService>();

        var workflowTrimmingSettings = await siteService.GetSettingsAsync<WorkflowTrimmingSettings>();
        if (workflowTrimmingSettings.Disabled)
        {
            return;
        }

        var logger = serviceProvider.GetRequiredService<ILogger<WorkflowTrimmingBackgroundTask>>();

        try
        {
            var clock = serviceProvider.GetRequiredService<IClock>();
            var workflowTrimmingManager = serviceProvider.GetRequiredService<IWorkflowTrimmingService>();
            var batchSize = serviceProvider.GetRequiredService<IOptions<WorkflowTrimmingOptions>>().Value.BatchSize;

            logger.LogDebug("Starting trimming Workflow instances.");

            var trimmedCount = await workflowTrimmingManager.TrimWorkflowInstancesAsync(
                TimeSpan.FromDays(workflowTrimmingSettings.RetentionDays),
                batchSize
            );

            logger.LogDebug("Trimmed {TrimmedCount} workflow instances.", trimmedCount);

            var workflowTrimmingSateDocumentManager = serviceProvider.GetRequiredService<IDocumentManager<WorkflowTrimmingState>>();
            var workflowTrimmingState = await workflowTrimmingSateDocumentManager.GetOrCreateMutableAsync();
            workflowTrimmingState.LastRunUtc = clock.UtcNow;
            await workflowTrimmingSateDocumentManager.UpdateAsync(workflowTrimmingState);
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            logger.LogError(ex, "Error while trimming workflow instances.");
        }
    }
}
