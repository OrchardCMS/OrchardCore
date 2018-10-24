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
using OrchardCore.Modules;

namespace OrchardCore.Distributed.Core.Services
{
    /// <summary>
    /// In a distributed environment, allows to synchronize the tenant states by subscribing
    /// to the 'Shell' channel, and then by publishing and reacting to shell event messages.
    /// </summary>
    public class DistributedShell : IDefaultShellEvents
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageBus _messageBus;
        private readonly SemaphoreSlim _synLock;
        private bool _initialized;

        public DistributedShell(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IRunningShellTable runningShellTable,
            IShellSettingsManager shellSettingsManager,
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IMessageBus> _messageBuses)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _runningShellTable = runningShellTable;
            _shellSettingsManager = shellSettingsManager;
            _httpContextAccessor = httpContextAccessor;

            if (shellSettings.Name == ShellHelper.DefaultShellName)
            {
                _messageBus = _messageBuses.LastOrDefault();
                _synLock = new SemaphoreSlim(1);
            }
        }

        /// <summary>
        /// Invoked when the 'Default' tenant is 1st created  or updated. Used to
        /// subscribe to the 'Shell' channel and react to the shell event messages.
        /// </summary>
        public async Task CreatedAsync()
        {
            if (_messageBus == null)
            {
                return;
            }

            // There is no shell context on the 1st creation before serving the 1st request.
            // And we have to also check that we are not executing an event message handler.
            if (_httpContextAccessor.HttpContext?.Features.Get<ShellContext>() == null &&
                !(_httpContextAccessor.HttpContext?.Items.ContainsKey("MessageHandler") ?? false))
            {
                // So, use a deferred task to let the requested tenant be fully initialized.
                var deferredTaskEngine = _httpContextAccessor.HttpContext.RequestServices?.GetService<IDeferredTaskEngine>();

                deferredTaskEngine?.AddTask(async context =>
                {
                    // Invoke the 'Created' event to subscribe to the 'Shell' channel.
                    using (var scope = await _shellHost.GetScopeAsync(ShellHelper.DefaultShellName))
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

                            // Try to load the last settings of the specified tenant.
                            if (!_shellSettingsManager.TryLoadSettings(tokens[0], out var settings))
                            {
                                return;
                            }

                            if (tokens[1] == "Changed" || tokens[1] == "Reload" || (tokens[1] == "Settings" &&
                                ShellHelper.DefaultShellName != settings.Name))
                            {
                                // Reload the shell of the specified tenant.
                                _shellHost.ReloadShellContextAsync(settings).GetAwaiter().GetResult();

                                // If the default shell has been reloaded.
                                if (settings.Name == ShellHelper.DefaultShellName)
                                {
                                    if (_httpContextAccessor.HttpContext == null)
                                    {
                                        _httpContextAccessor.HttpContext = new DefaultHttpContext();
                                    }

                                    _httpContextAccessor.HttpContext.Items["MessageHandler"] = true;

                                    // Invoke the 'Created' event to subscribe again to the 'Shell' channel.
                                    using (var scope = _shellHost.GetScopeAsync(settings).GetAwaiter().GetResult())
                                    {
                                        var events = scope.ServiceProvider.GetService<IDefaultShellEvents>();
                                        events?.CreatedAsync().GetAwaiter().GetResult();
                                    }
                                }
                            }

                            else if (tokens[1] == "Settings" && ShellHelper.DefaultShellName == settings.Name)
                            {
                                // The 'Default' tenant needs to stay in a running state in any instances.
                                // So, we just remove it from the running shell table, so that it is no more served.
                                // Then the folllowing 'Reload' event will register it again in the table.
                                _runningShellTable.Remove(settings);
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
        /// Invoked when any tenant descriptor has changed.
        /// </summary>
        public Task ChangedAsync(string tenant)
        {
            return PublishAsync(tenant, "Changed");
        }

        /// <summary>
        /// Invoked when any tenant has been reloaded.
        /// </summary>
        public Task ReloadAsync(string tenant)
        {
            return PublishAsync(tenant, "Reload");
        }

        /// <summary>
        /// Invoked when any tenant settings has been updated.
        /// </summary>
        public Task UpdateSettingsAsync(string tenant)
        {
            return PublishAsync(tenant, "Settings");
        }

        /// <summary>
        /// Used to publish shell event messages.
        /// </summary>
        private async Task PublishAsync(string tenant, string eventName)
        {
            // If the 'Default' tenant has changed, a message bus may have been
            // just enabled and not yet available, so let's add a deferred task.
            if (_messageBus == null && ShellHelper.DefaultShellName != tenant)
            {
                return;
            }

            // There is no shell feature if we are executing an event message handler.
            var currentShell = _httpContextAccessor.HttpContext?.Features.Get<ShellContext>();

            // If so, break the loop.
            if (currentShell == null)
            {
                return;
            }

            // If not in a request tied to this tenant we can publish immediately. Or in
            // case of the update 'Settings' event which is followed by a 'Reload' event.
            if (currentShell.Settings.Name != tenant || eventName == "Settings")
            {
                // We publish immediately without waiting for a database session to be committed.
                await (_messageBus?.PublishAsync("Shell", tenant + ':' + eventName) ?? Task.CompletedTask);
                return;
            }

            // Nothing to do if, from the last updated settings, the tenant is not yet running.
            if (!_shellSettingsManager.TryLoadSettings(tenant, out var settings) || settings.State != TenantState.Running)
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
                    // Shell event messages are always published using the 'Default' tenant.
                    using (var changedScope = await _shellHost.GetScopeAsync(ShellHelper.DefaultShellName))
                    {
                        // If the default shell has changed.
                        if (tenant == ShellHelper.DefaultShellName)
                        {
                            // Invoke the 'Created' event to subscribe again to the 'Shell' channel.
                            var events = changedScope.ServiceProvider.GetService<IDefaultShellEvents>();
                            await (events?.CreatedAsync() ?? Task.CompletedTask);
                        }

                        // Publish the shell event message.
                        var messageBus = changedScope.ServiceProvider.GetService<IMessageBus>();
                        await (messageBus?.PublishAsync("Shell", tenant + ':' + eventName) ?? Task.CompletedTask);
                    }
                }, order: 100);
            }
        }
    }
}
