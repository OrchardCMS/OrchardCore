using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Clusters
{
    [BackgroundTask(
        Title = "Auto release tenants on inactivity.",
        Schedule = "* * * * *",
        Description = "Auto release tenants after the max idle time of their cluster.")]
    public class TenantInactivityBackgroundTask : IBackgroundTask
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly ClustersOptions _options;

        public TenantInactivityBackgroundTask(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IOptions<ClustersOptions> options)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _options = options.Value;
        }

        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            if (_shellHost.TryGetShellContext(_shellSettings.Name, out var shellContext))
            {
                var maxIdleTime = _options.Clusters
                    .FirstOrDefault(cluster => cluster.ClusterId == _shellSettings.GetClusterId(_options))
                    ?.MaxIdleTime;

                if (maxIdleTime.HasValue && DateTime.UtcNow.Add(maxIdleTime.Value) > shellContext.LastRequestTimeUtc)
                {
                    await _shellHost.ReleaseShellContextAsync(_shellSettings, eventSource: false) ;
                }
            }
        }
    }
}
