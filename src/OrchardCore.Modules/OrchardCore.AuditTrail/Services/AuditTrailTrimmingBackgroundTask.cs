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
    [BackgroundTask(Schedule = "0 0 * * *", Description = "A background task that regularly deletes old Audit Trail Events.")]
    public class AuditTrailTrimmingBackgroundTask : IBackgroundTask
    {
        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var siteService = serviceProvider.GetService<ISiteService>();
            var clock = serviceProvider.GetService<IClock>();
            var logger = serviceProvider.GetService<ILogger<AuditTrailTrimmingBackgroundTask>>();
            var auditTrailTrimmingSettings = (await siteService.GetSiteSettingsAsync()).As<AuditTrailTrimmingSettings>();

            logger.LogDebug("Audit Trail trimming: beginning sweep.");

            try
            {
                // We don't need to check the audit trail for events to remove every 10 minutes. 
                // Let's stick with the hours of the minimum run interval.
                if (!GetIsTimeToTrim(auditTrailTrimmingSettings, clock)) return;

                logger.LogDebug("Starting Audit Trail trimming.");
                var auditTrailManager = serviceProvider.GetService<IAuditTrailManager>();

                var deletedEvents = await auditTrailManager.TrimAsync(TimeSpan.FromDays(auditTrailTrimmingSettings.RetentionPeriodDays));
                logger.LogDebug("Audit Trail trimming completed. {0} events were deleted.", deletedEvents);
                auditTrailTrimmingSettings.LastRunUtc = clock.UtcNow;
                
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


        private bool GetIsTimeToTrim(AuditTrailTrimmingSettings auditTrailTrimmingSettings, IClock clock)
        {
            if (auditTrailTrimmingSettings.Disabled) return false;

            var lastRun = auditTrailTrimmingSettings.LastRunUtc ?? DateTime.MinValue;
            var now = clock.UtcNow;
            var interval = TimeSpan.FromHours(auditTrailTrimmingSettings.MinimumRunIntervalHours);
            return now - lastRun >= interval;
        }
    }
}
