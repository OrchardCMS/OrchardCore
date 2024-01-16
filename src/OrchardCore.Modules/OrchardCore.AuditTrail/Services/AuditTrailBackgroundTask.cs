using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.BackgroundTasks;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.AuditTrail.Services
{
    [BackgroundTask(
        Title = "Audit Trail Events Purger",
        Schedule = "0 0 * * *",
        Description = "Regularly purges old Audit Trail events.",
        LockTimeout = 3_000, LockExpiration = 30_000)]

    public class AuditTrailBackgroundTask : IBackgroundTask
    {
        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var siteService = serviceProvider.GetRequiredService<ISiteService>();

            var settings = (await siteService.GetSiteSettingsAsync()).As<AuditTrailTrimmingSettings>();
            if (settings.Disabled)
            {
                return;
            }

            var logger = serviceProvider.GetRequiredService<ILogger<AuditTrailBackgroundTask>>();
            logger.LogDebug("Audit Trail trimming: beginning sweep.");

            try
            {
                var clock = serviceProvider.GetRequiredService<IClock>();
                var auditTrailManager = serviceProvider.GetRequiredService<IAuditTrailManager>();

                logger.LogDebug("Starting Audit Trail trimming.");
                var deletedEvents = await auditTrailManager.TrimEventsAsync(TimeSpan.FromDays(settings.RetentionDays));
                logger.LogDebug("Audit Trail trimming completed. {EventCount} events were deleted.", deletedEvents);
                settings.LastRunUtc = clock.UtcNow;

                var container = await siteService.LoadSiteSettingsAsync();
                container.Alter<AuditTrailTrimmingSettings>(nameof(AuditTrailTrimmingSettings), settings =>
                {
                    settings.LastRunUtc = clock.UtcNow;
                });

                await siteService.UpdateSiteSettingsAsync(container);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                logger.LogError(ex, "Audit Trail trimming: error during sweep.");
            }
            finally
            {
                logger.LogDebug("Audit Trail trimming: ending sweep.");
            }
        }
    }
}
