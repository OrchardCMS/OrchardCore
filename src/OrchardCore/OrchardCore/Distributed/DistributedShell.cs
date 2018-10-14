using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DeferredTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Hosting.ShellBuilders;

namespace OrchardCore.Distributed
{
    /// <summary>
    /// In a distributed environment, allows to synchronize tenant states by subscribing
    /// to the 'Shell' channel, and then by publishing and reacting to 'Changed' messages.
    /// </summary>
    public class DistributedShell : IDefaultShellEvents
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageBus _messageBus;
        private readonly SemaphoreSlim _synLock;
        private bool _initialized;

        public DistributedShell(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IShellSettingsManager shellSettingsManager,
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IMessageBus> _messageBuses)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _shellSettingsManager = shellSettingsManager;
            _httpContextAccessor = httpContextAccessor;

            if (shellSettings.Name == ShellHelper.DefaultShellName)
            {
                _messageBus = _messageBuses.LastOrDefault();
                _synLock = new SemaphoreSlim(1);
            }
        }

        /// <summary>
        /// Invoked when the 'Default' tenant has been created or recreated. Used to
        /// subscribe to the 'Shell' channel and react to the shell 'Changed' events.
        /// </summary>
        public async Task CreatedAsync()
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
                        // Subscribe to the 'Shell' channel.
                        await _messageBus.SubscribeAsync("Shell", (channel, message) =>
                        {
                            var tokens = message.Split(':').ToArray();

                            // Validate the message = {tenant}:{event}
                            if (tokens.Length != 2 || tokens[0].Length == 0)
                            {
                                return;
                            }

                            // Check for a valid tenant.
                            if (_shellSettingsManager.TryGetSettings(tokens[0], out var settings))
                            {
                                if (tokens[1] == "Changed")
                                {
                                    // Reload the shell with 'localEvent: false' to break the event loop.
                                    _shellHost.ReloadShellContextAsync(settings, localEvent: false).GetAwaiter().GetResult();
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
        /// Invoked when any tenant has changed. Used to publish shell 'Changed' events.
        /// </summary>
        public async Task ChangedAsync(string tenant)
        {
            if (_messageBus == null)
            {
                return;
            }

            var currentShell = _httpContextAccessor.HttpContext.Features.Get<ShellContext>();

            // If not in a context tied to this tenant, e.g when 'Changed' through the 'Default' tenant.
            if (currentShell?.Settings.Name != tenant)
            {
                // The shell 'Changed' event message can be published immediately.
                await (_messageBus.PublishAsync("Shell", tenant + ":Changed") ?? Task.CompletedTask);
                return;
            }

            // Try to retrieve the last updated settings.
            if (!_shellSettingsManager.TryGetSettings(tenant, out var settings))
            {
                return;
            }

            // Nothing to do if the tenant is not yet running, e.g during a tenant setup.
            if (settings.State != TenantState.Running)
            {
                return;
            }

            // Otherwise use a deferred task so that any pending database updates will be committed.
            using (var scope = await _shellHost.GetScopeAsync(settings))
            {
                var deferredTaskEngine = scope.ServiceProvider?.GetService<IDeferredTaskEngine>();

                deferredTaskEngine?.AddTask(async context =>
                {
                    if (_shellSettingsManager.TryGetSettings(ShellHelper.DefaultShellName, out var defaultSettings))
                    {
                        using (var changedScope = await _shellHost.GetScopeAsync(defaultSettings))
                        {
                            // If the default shell was changed and released.
                            if (tenant == ShellHelper.DefaultShellName)
                            {
                                // Invoke the 'Created' event to subscribe again to the 'Shell' channel.
                                var events = changedScope.ServiceProvider.GetService<IDefaultShellEvents>();
                                await (events?.CreatedAsync() ?? Task.CompletedTask);
                            }

                            // Then publish the shell 'Changed' event message.
                            var messageBus = changedScope.ServiceProvider.GetService<IMessageBus>();
                            await (messageBus?.PublishAsync("Shell", tenant + ":Changed") ?? Task.CompletedTask);
                        }
                    }
                }, order: 100);
            }
        }
    }
}
