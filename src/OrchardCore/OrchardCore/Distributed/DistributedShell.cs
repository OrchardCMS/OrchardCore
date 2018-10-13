using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DeferredTasks;
using OrchardCore.Environment.Shell;
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
        public Task ChangedAsync(string tenant)
        {
            var currentShell = _httpContextAccessor.HttpContext?.Features.Get<ShellContext>();
            var isBackground = _httpContextAccessor.HttpContext?.Items["IsBackground"] != null;

            // If not in the context of a valid request related to this tenant.
            if (currentShell?.Settings.Name != tenant || isBackground)
            {
                // The shell 'Changed' event message can be published immediately.
                return (_messageBus?.PublishAsync("Shell", tenant + ":Changed") ?? Task.CompletedTask);
            }

            // Otherwise use a deferred task so that any pending database updates will be committed.
            using (var scope = currentShell.CreateScope())
            {
                var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();

                // Can be null just after a tenant setup.
                deferredTaskEngine?.AddTask(async context =>
                {
                    if (_shellSettingsManager.TryGetSettings(ShellHelper.DefaultShellName, out var _defaultSettings))
                    {
                        using (var changedScope = await _shellHost.GetScopeAsync(_defaultSettings))
                        {
                            // If the default shell was changed and released.
                            if (tenant == ShellHelper.DefaultShellName)
                            {
                                // Invoke the 'Created' event to subscribe again to the 'Shell' channel.
                                var defaultShellEvents = changedScope.ServiceProvider.GetService<IDefaultShellEvents>();
                                await (defaultShellEvents?.CreatedAsync() ?? Task.CompletedTask);
                            }

                            // Then publish the shell 'Changed' event message.
                            var messageBus = changedScope.ServiceProvider.GetService<IMessageBus>();
                            await (messageBus?.PublishAsync("Shell", tenant + ":Changed") ?? Task.CompletedTask);
                        }
                    }
                }, order: 100);
            }

            return Task.CompletedTask;
        }
    }
}
