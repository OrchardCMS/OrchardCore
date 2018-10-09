using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Distributed
{
    /// <summary>
    /// 'IDefaultTenantEvents' are only invoked on the default tenant.
    /// </summary>
    public class DistributedShell : IDefaultTenantEvents
    {
        private readonly IShellHost _shellHost ;
        private readonly ShellSettings _shellSettings;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IMessageBus _messageBus;

        public DistributedShell(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IShellSettingsManager shellSettingsManager,
            IEnumerable<IMessageBus> _messageBuses)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _shellSettingsManager = shellSettingsManager;
            _messageBus = _messageBuses.LastOrDefault();
        }

        public Task DefaultTenantCreatedAsync()
        {
            if (_messageBus != null)
            {
                return _messageBus.SubscribeAsync("ShellReload", (channel, message) =>
                {
                    if (_shellSettingsManager.TryGetSettings(message, out var settings))
                    {
                        _shellHost.ReloadShellContextAsync(settings, broadcast: false).GetAwaiter().GetResult();
                    }
                });
            }

            return Task.CompletedTask;
        }

        public Task ReloadedAsync(string tenant)
        {
            if (_messageBus != null)
            {
                return _messageBus.PublishAsync("ShellReload", tenant);
            }

            return Task.CompletedTask;
        }
    }
}
