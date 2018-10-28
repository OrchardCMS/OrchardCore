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
    public class DistributedShell : IShellHostEvents
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

            // If not called from a deferred task or a message handler, this is the 1st creation.
            if (!(_httpContextAccessor.HttpContext?.Items.ContainsKey("DeferredTask") ?? false) &&
                !(_httpContextAccessor.HttpContext?.Items.ContainsKey("ShellChannel") ?? false))
            {
                // So, use a deferred task to let the 'Default' tenant be fully initialized.
                var deferredTaskEngine = _httpContextAccessor.HttpContext.RequestServices?.GetService<IDeferredTaskEngine>();

                deferredTaskEngine?.AddTask(async context =>
                {
                    // Mark the context as being part of a 'DeferredTask'.
                    _httpContextAccessor.HttpContext.Items["DeferredTask"] = true;

                    // Invoke the 'Created' event to subscribe to the 'Shell' channel.
                    using (var scope = await _shellHost.GetScopeAsync(ShellHelper.DefaultShellName))
                    {
                        var events = scope.ServiceProvider.GetService<IShellHostEvents>();
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

                            if (_httpContextAccessor.HttpContext == null)
                            {
                                _httpContextAccessor.HttpContext = new DefaultHttpContext();
                            }

                            // Mark the context as being part of the 'Shell' channel.
                            _httpContextAccessor.HttpContext.Items["ShellChannel"] = true;

                            if (tokens[1] == "Changed" || tokens[1] == "Reloaded" || (tokens[1] == "Updated" &&
                                ShellHelper.DefaultShellName != settings.Name))
                            {
                                // Reload the shell of the specified tenant.
                                _shellHost.ReloadShellContextAsync(settings).GetAwaiter().GetResult();

                                // If the default shell has been reloaded.
                                if (settings.Name == ShellHelper.DefaultShellName)
                                {
                                    // Invoke the 'Created' event to subscribe again to the 'Shell' channel.
                                    using (var scope = _shellHost.GetScopeAsync(settings).GetAwaiter().GetResult())
                                    {
                                        var events = scope.ServiceProvider.GetService<IShellHostEvents>();
                                        events?.CreatedAsync().GetAwaiter().GetResult();
                                    }
                                }
                            }

                            else if (tokens[1] == "Updated" && ShellHelper.DefaultShellName == settings.Name)
                            {
                                // The 'Default' tenant needs to stay in a running state in any instances.
                                // So, we just remove it from the running shell table, so that it is no more served.
                                // Then the folllowing 'Reloaded' event will register it again in the table.
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
        public Task ReloadedAsync(string tenant)
        {
            return PublishAsync(tenant, "Reloaded");
        }

        /// <summary>
        /// Invoked when any tenant settings has been updated.
        /// </summary>
        public Task UpdatedAsync(string tenant)
        {
            return PublishAsync(tenant, "Updated");
        }

        /// <summary>
        /// Used to publish shell event messages.
        /// </summary>
        private async Task PublishAsync(string tenant, string eventName)
        {
            // Nothing to do if no message bus, unless for the 'Default' tenant for which a 
            // message bus may have been just enabled, so let's use a deferred task further.
            if (_messageBus == null && ShellHelper.DefaultShellName != tenant)
            {
                return;
            }

            // if we are executing a 'Shell' event message handler, break the loop.
            if (_httpContextAccessor.HttpContext?.Items.ContainsKey("ShellChannel") ?? false)
            {
                return;
            }

            // Otherwise, normally here we can retrieve the current shell.
            var currentShell = _httpContextAccessor.HttpContext?.Features.Get<ShellContext>();

            // If not in a request tied to this tenant we can publish immediately. Or
            // in case of the 'Updated' event which is followed by a 'Reloaded' event.
            if (currentShell?.Settings.Name != tenant || eventName == "Updated")
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
                    // Mark the context as being part of a 'DeferredTask'.
                    _httpContextAccessor.HttpContext.Items["DeferredTask"] = true;

                    // Shell event messages are always published using the 'Default' tenant.
                    using (var changedScope = await _shellHost.GetScopeAsync(ShellHelper.DefaultShellName))
                    {
                        // If the default shell has changed.
                        if (tenant == ShellHelper.DefaultShellName)
                        {
                            // Invoke the 'Created' event to subscribe again to the 'Shell' channel.
                            var events = changedScope.ServiceProvider.GetService<IShellHostEvents>();
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
