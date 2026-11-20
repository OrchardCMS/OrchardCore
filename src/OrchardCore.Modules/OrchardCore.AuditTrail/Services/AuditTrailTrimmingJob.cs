using OrchardCore.AuditTrail.Settings;
using OrchardCore.Entities;
using OrchardCore.Jobs;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.AuditTrail.Services;

/// <summary>
/// Deletes the audit trail events older than the retention period of the audit trail settings. It runs every day
/// by default, once whatever the number of servers.
/// </summary>
public sealed class AuditTrailTrimmingJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "AuditTrailTrimming";

    private readonly ISiteService _siteService;
    private readonly IAuditTrailManager _auditTrailManager;
    private readonly IClock _clock;

    public AuditTrailTrimmingJob(
        ISiteService siteService,
        IAuditTrailManager auditTrailManager,
        IClock clock)
    {
        _siteService = siteService;
        _auditTrailManager = auditTrailManager;
        _clock = clock;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var settings = await _siteService.GetSettingsAsync<AuditTrailTrimmingSettings>();

        if (settings.Disabled)
        {
            context.LogInformation("The trimming is disabled in the audit trail settings.");

            return;
        }

        var deletedEvents = await _auditTrailManager.TrimEventsAsync(TimeSpan.FromDays(settings.RetentionDays));

        var container = await _siteService.LoadSiteSettingsAsync();
        container.Alter<AuditTrailTrimmingSettings>(nameof(AuditTrailTrimmingSettings), settings =>
        {
            settings.LastRunUtc = _clock.UtcNow;
        });

        await _siteService.UpdateSiteSettingsAsync(container);

        context.LogInformation($"{deletedEvents} events older than {settings.RetentionDays} days were deleted.");
        context.SetResult(new { DeletedEvents = deletedEvents });
    }
}
