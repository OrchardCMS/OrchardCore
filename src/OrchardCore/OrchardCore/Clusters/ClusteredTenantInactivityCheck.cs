using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Clusters;

/// <summary>
/// Auto release clustered tenants on inactivity.
/// </summary>
[BackgroundTask(
    Title = "Auto release clustered tenants on inactivity.",
    Schedule = "* * * * *",
    Description = "Auto release tenants after the max idle time of their cluster.")]
public class ClusteredTenantInactivityCheck : IBackgroundTask
{
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IOptionsMonitor<ClustersOptions> _optionsMonitor;

    public ClusteredTenantInactivityCheck(
        IShellHost shellHost,
        ShellSettings shellSettings,
        IOptionsMonitor<ClustersOptions> optionsMonitor)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _optionsMonitor = optionsMonitor;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var options = _optionsMonitor.CurrentValue;

        // Check if the clusters max idle time is configured and if the tenant pipeline has been built.
        if (options.MaxIdleTime.HasValue &&
            _shellHost.TryGetShellContext(_shellSettings.Name, out var shellContext) &&
            shellContext.HasPipeline())
        {
            // Check if the clusters max idle time has expired for this tenant.
            if (shellContext.LastRequestTimeUtc.Add(options.MaxIdleTime.Value) < DateTime.UtcNow)
            {
                await _shellHost.ReleaseShellContextAsync(_shellSettings, eventSource: false);
            }
        }
    }
}
