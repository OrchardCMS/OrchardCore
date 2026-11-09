using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Clusters;

/// <summary>
/// Auto releases the tenants running on this instance after the max idle time of the clusters.
/// </summary>
public sealed class ClusteredTenantInactivityService : BackgroundService
{
    private static readonly TimeSpan s_checkPeriod = TimeSpan.FromMinutes(1);

    private readonly IShellHost _shellHost;
    private readonly IOptionsMonitor<ClustersOptions> _optionsMonitor;
    private readonly ILogger _logger;

    public ClusteredTenantInactivityService(
        IShellHost shellHost,
        IOptionsMonitor<ClustersOptions> optionsMonitor,
        ILogger<ClusteredTenantInactivityService> logger)
    {
        _shellHost = shellHost;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    /// <summary>
    /// Releases the tenants having a pipeline which didn't get a request during the max idle time, if it is configured.
    /// </summary>
    public async Task ReleaseIdleTenantsAsync()
    {
        var maxIdleTime = _optionsMonitor.CurrentValue.MaxIdleTime;

        if (!maxIdleTime.HasValue)
        {
            return;
        }

        foreach (var shellContext in _shellHost.ListShellContexts())
        {
            if (shellContext.HasPipeline() && DateTime.UtcNow - shellContext.LastRequestTimeUtc > maxIdleTime.Value)
            {
                await _shellHost.ReleaseShellContextAsync(shellContext.Settings, eventSource: false);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(s_checkPeriod);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ReleaseIdleTenantsAsync();
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Failed to release the idle tenants.");
            }
        }
    }
}
