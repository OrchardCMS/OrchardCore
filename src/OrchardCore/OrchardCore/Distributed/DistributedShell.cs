using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Distributed
{
    /// <summary>
    /// 'IDistributedShell' specific events are only invoked on the default tenant.
    /// </summary>
    public class DistributedShell : IDistributedShell
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IMessageBus _messageBus;
        private readonly SemaphoreSlim _synLock;
        private bool _initialized;

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

            if (shellSettings.Name == ShellHelper.DefaultShellName)
            {
                _synLock = new SemaphoreSlim(1);
            }
        }

        #region IDistributedShell
        /// <summary>
        /// This event is only invoked on the 'Default' tenant.
        /// </summary>
        public async Task ActivatedAsync(string tenant)
        {
            if (_messageBus == null)
            {
                return;
            }

            if (!_initialized && _synLock != null)
            {
                await _synLock.WaitAsync();

                try
                {
                    if (!_initialized)
                    {
                        await _messageBus.SubscribeAsync("Shell", (channel, message) =>
                        {
                            var tokens = message.Split(':').ToArray();

                            if (tokens.Length != 2 || tokens[0].Length == 0)
                            {
                                return;
                            }

                            if (_shellSettingsManager.TryGetSettings(tokens[0], out var settings))
                            {
                                if (tokens[1] == "Terminated")
                                {
                                    _shellHost.ReloadShellContextAsync(settings, fireEvent: false).GetAwaiter().GetResult();
                                }
                            }
                        });
                    }
                }
                finally
                {
                    _initialized = true;
                    _synLock.Release();
                }
            }
        }

        /// <summary>
        /// This event is only invoked on the 'Default' tenant.
        /// </summary>
        public Task TerminatedAsync(string tenant)
        {
            return (_messageBus?.PublishAsync("Shell", tenant + ":Terminated") ?? Task.CompletedTask);
        }
        #endregion

        #region IModularTenantEvents
        Task IModularTenantEvents.ActivatingAsync() { return Task.CompletedTask; }

        public async Task ActivatedAsync()
        {
            if (_shellSettings.Name == ShellHelper.DefaultShellName)
            {
                await ActivatedAsync(ShellHelper.DefaultShellName);
            }

            else if (_shellSettingsManager.TryGetSettings(ShellHelper.DefaultShellName, out var defaultSettings))
            {
                using (var scope = await _shellHost.GetScopeAsync(defaultSettings))
                {
                    var distributedShell = scope.ServiceProvider.GetService<IDistributedShell>();
                    await (distributedShell?.ActivatedAsync(_shellSettings.Name) ?? Task.CompletedTask);
                }
            }
        }

        public Task TerminatingAsync() { return Task.CompletedTask; }

        public async Task TerminatedAsync()
        {
            if (_shellSettings.Name == ShellHelper.DefaultShellName)
            {
                await TerminatedAsync(ShellHelper.DefaultShellName);
            }

            else if (_shellSettingsManager.TryGetSettings(ShellHelper.DefaultShellName, out var defaultSettings))
            {
                using (var scope = await _shellHost.GetScopeAsync(defaultSettings))
                {
                    var distributedShell = scope.ServiceProvider.GetService<IDistributedShell>();
                    await (distributedShell?.TerminatedAsync(_shellSettings.Name) ?? Task.CompletedTask);
                }
            }
        }
        #endregion
    }
}
