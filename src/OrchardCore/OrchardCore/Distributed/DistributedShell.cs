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
        /// Invoked when the 'Default' tenant is 1st created  or has changed. Used to
        /// subscribe to the 'Shell' channel and react to the shell 'Changed' messages.
        /// </summary>
        public async Task CreatedAsync()
        {
            if (_messageBus == null)
            {
                return;
            }

            // There is no shell on the 1st creation before serving the 1st request.
            if (_httpContextAccessor.HttpContext?.Features.Get<ShellContext>() == null)
            {
                // So, use a deferred task to let the requested tenant be fully initialized.
                var deferredTaskEngine = _httpContextAccessor.HttpContext.RequestServices?.GetService<IDeferredTaskEngine>();

                deferredTaskEngine?.AddTask(async context =>
                {
                    // Try to retrieve the last updated settings of the `Default` tenant.
                    if (!_shellSettingsManager.TryGetSettings(ShellHelper.DefaultShellName, out var defaultSettings))
                    {
                        return;
                    }

                    // Invoke the 'Created' event to subscribe to the 'Shell' channel.
                    using (var scope = await _shellHost.GetScopeAsync(defaultSettings))
                    {
                        var events = scope.ServiceProvider.GetService<IDefaultShellEvents>();
                        await (events?.CreatedAsync() ?? Task.CompletedTask);
                    }
                }, order: 100);

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

                            // Validate the message {tenant}:{event}
                            if (tokens.Length != 2 || tokens[0].Length == 0)
                            {
                                return;
                            }

                            // Try to retrieve the last settings of the specified tenant.
                            if (!_shellSettingsManager.TryGetSettings(tokens[0], out var settings))
                            {
                                return;
                            }

                            if (tokens[1] == "Changed")
                            {
                                // Reload the shell of the specified tenant.
                                _shellHost.ReloadShellContextAsync(settings).GetAwaiter().GetResult();

                                // If the default shell has been reloaded.
                                if (settings.Name == ShellHelper.DefaultShellName)
                                {
                                    // Invoke the 'Created' event to subscribe again to the 'Shell' channel.
                                    using (var scope = _shellHost.GetScopeAsync(settings).GetAwaiter().GetResult())
                                    {
                                        var events = scope.ServiceProvider.GetService<IDefaultShellEvents>();
                                        events?.CreatedAsync().GetAwaiter().GetResult();
                                    }
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
        /// Invoked when any tenant has changed. Used to publish shell 'Changed' messages.
        /// </summary>
        public async Task ChangedAsync(string tenant)
        {
            // If the 'Default' tenant has changed, a message bus may have been
            // just enabled and not yet available, so let's add a deferred task.
            if (_messageBus == null && ShellHelper.DefaultShellName != tenant)
            {
                return;
            }

            // There is no shell feature if we are executing the `Changed` message handler.
            var currentShell = _httpContextAccessor.HttpContext?.Features.Get<ShellContext>();

            // If so, break the loop.
            if (currentShell == null)
            {
                return;
            }

            // If not in a request tied to this tenant.
            if (currentShell.Settings.Name != tenant)
            {
                // We can publish immediately the shell 'Changed' message.
                await (_messageBus?.PublishAsync("Shell", tenant + ":Changed") ?? Task.CompletedTask);
                return;
            }

            // Nothing to do if, from the last updated settings, the tenant is not yet running.
            if (!_shellSettingsManager.TryGetSettings(tenant, out var settings) || settings.State != TenantState.Running)
            {
                return;
            }

            // Here means the request is tied to this running tenant.
            using (var scope = await _shellHost.GetScopeAsync(settings))
            {
                // So, use a deferred task to let any storage be completed.
                var deferredTaskEngine = scope.ServiceProvider?.GetService<IDeferredTaskEngine>();

                deferredTaskEngine?.AddTask(async context =>
                {
                    // Try to retrieve the last updated settings of the `Default` tenant.
                    if (!_shellSettingsManager.TryGetSettings(ShellHelper.DefaultShellName, out var defaultSettings))
                    {
                        return;
                    }

                    // 'Changed' messages are always published using the 'Default' tenant.
                    using (var changedScope = await _shellHost.GetScopeAsync(defaultSettings))
                    {
                        // If the default shell has changed.
                        if (tenant == ShellHelper.DefaultShellName)
                        {
                            // Invoke the 'Created' event to subscribe again to the 'Shell' channel.
                            var events = changedScope.ServiceProvider.GetService<IDefaultShellEvents>();
                            await (events?.CreatedAsync() ?? Task.CompletedTask);
                        }

                        // Publish the shell 'Changed' message.
                        var messageBus = changedScope.ServiceProvider.GetService<IMessageBus>();
                        await (messageBus?.PublishAsync("Shell", tenant + ":Changed") ?? Task.CompletedTask);
                    }
                }, order: 100);
            }
        }
    }
}
