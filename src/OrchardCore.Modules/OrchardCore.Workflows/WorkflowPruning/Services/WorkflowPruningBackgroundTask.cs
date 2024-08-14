using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Workflows.WorkflowPruning.Models;

namespace OrchardCore.Workflows.WorkflowPruning.Services;

[BackgroundTask(
    Schedule = "0 0 * * *",
    Title = "Workflow Pruning Background Task",
    Description = "Regularly prunes old workflow instances.",
    LockTimeout = 3_000,
    LockExpiration = 30_000
)]
public class WorkflowPruningBackgroundTask : IBackgroundTask
{
    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var siteService = serviceProvider.GetRequiredService<ISiteService>();

        var workflowPruningSettings = await siteService.GetSettingsAsync<WorkflowPruningSettings>();
        if (workflowPruningSettings.Disabled)
        {
            return;
        }

        var logger = serviceProvider.GetRequiredService<ILogger<WorkflowPruningBackgroundTask>>();

        try
        {
            var clock = serviceProvider.GetRequiredService<IClock>();
            var workflowCleanUpManager = serviceProvider.GetRequiredService<IWorkflowPruningManager>();

            logger.LogDebug("Starting pruning Workflow instances.");
            var prunedCount = await workflowCleanUpManager.PruneWorkflowInstancesAsync(
                TimeSpan.FromDays(workflowPruningSettings.RetentionDays)
            );
            logger.LogDebug("Pruned {PrunedCount} workflow instances.", prunedCount);

            var siteSettings = await siteService.LoadSiteSettingsAsync();
            siteSettings.Alter<WorkflowPruningSettings>(settings =>
            {
                settings.LastRunUtc = clock.UtcNow;
            });

            await siteService.UpdateSiteSettingsAsync(siteSettings);
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            logger.LogError(ex, "Error while pruning workflow instances.");
        }
    }
}
