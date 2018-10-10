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
    public class DistributedShell : IModularTenantEvents, IDefaultTenantEvents
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

        public Task ActivatingAsync() { return Task.CompletedTask; }

        public Task ActivatedAsync()
        {
            // The default tenant need to be activated to subscribe to the 'Reload' channel.
            // This is the case e.g as soon as any static file is requested from any tenant.
            if (_shellSettings.Name == ShellHelper.DefaultShellName && _messageBus != null)
            {
                return _messageBus.SubscribeAsync("Reload", (channel, message) =>
                {
                    if (_shellSettingsManager.TryGetSettings(message, out var settings))
                    {
                        _shellHost.ReloadShellContextAsync(settings, fireEvent: false).GetAwaiter().GetResult();
                    }
                });
            }

            return Task.CompletedTask;
        }

        public Task TerminatingAsync() { return Task.CompletedTask; }
        public Task TerminatedAsync() { return Task.CompletedTask; }

        /// <summary>
        /// This event is only invoked on the 'Default' tenant.
        /// </summary>
        public async Task ReloadAsync(string tenant)
        {
            if (_messageBus != null)
            {
                await _messageBus.PublishAsync("Reload", tenant);
            }

            return;
        }
    }
}
