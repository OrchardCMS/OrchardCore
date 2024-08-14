using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Workflows.Trimming.Models;

namespace OrchardCore.Workflows.Trimming.Services;

[BackgroundTask(
    Schedule = "0 0 * * *",
    Title = "Workflow Trimming Background Task",
    Description = "Regularly prunes old workflow instances.",
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
            var workflowTrimmingManager = serviceProvider.GetRequiredService<IWorkflowTrimmingManager>();
            var batchSize = serviceProvider.GetRequiredService<IOptions<WorkflowTrimmingOptions>>().Value.BatchSize;

            logger.LogDebug("Starting trimming Workflow instances.");
            var prunedCount = await workflowTrimmingManager.TrimWorkflowInstancesAsync(
                TimeSpan.FromDays(workflowTrimmingSettings.RetentionDays),
                batchSize
            );
            logger.LogDebug("Pruned {PrunedCount} workflow instances.", prunedCount);

            var siteSettings = await siteService.LoadSiteSettingsAsync();
            siteSettings.Alter<WorkflowTrimmingSettings>(settings =>
            {
                settings.LastRunUtc = clock.UtcNow;
            });

            await siteService.UpdateSiteSettingsAsync(siteSettings);
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            logger.LogError(ex, "Error while trimming workflow instances.");
        }
    }
}
