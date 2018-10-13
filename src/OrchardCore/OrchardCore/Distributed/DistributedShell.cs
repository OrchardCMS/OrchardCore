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
    /// 'IDefaultShellEvents' events are only invoked on the default tenant.
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
            _messageBus = _messageBuses.LastOrDefault();

            if (shellSettings.Name == ShellHelper.DefaultShellName)
            {
                _synLock = new SemaphoreSlim(1);
            }
        }

        /// <summary>
        /// This event is only invoked on the 'Default' tenant.
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
                        await _messageBus.SubscribeAsync("Shell", (channel, message) =>
                        {
                            var tokens = message.Split(':').ToArray();

                            if (tokens.Length != 2 || tokens[0].Length == 0)
                            {
                                return;
                            }

                            if (_shellSettingsManager.TryGetSettings(tokens[0], out var settings))
                            {
                                if (tokens[1] == "Changed")
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
        public async Task ChangedAsync(string tenant)
        {
            var currentShell = _httpContextAccessor.HttpContext?.Features.Get<ShellContext>();

            if (currentShell?.Settings.Name == tenant && currentShell.ActiveScopes > 0)
            {
                using (var scope = currentShell.CreateScope())
                {
                    var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();

                    deferredTaskEngine.AddTask(async context =>
                    {
                        if (_shellSettingsManager.TryGetSettings(ShellHelper.DefaultShellName, out var _defaultSettings))
                        {
                            using (var changedScope = await _shellHost.GetScopeAsync(_defaultSettings))
                            {
                                if (tenant == ShellHelper.DefaultShellName)
                                {
                                    // Need to re-activate the default shell to subcribe again to the channel.
                                    var defaultShellEvents = changedScope.ServiceProvider.GetService<IDefaultShellEvents>();
                                    await (defaultShellEvents?.CreatedAsync() ?? Task.CompletedTask);
                                }

                                var messageBus = changedScope.ServiceProvider.GetService<IMessageBus>();
                                await (messageBus?.PublishAsync("Shell", tenant + ":Changed") ?? Task.CompletedTask);
                            }
                        }
                    }, order: 10);
                }

                return;
            }

            await (_messageBus?.PublishAsync("Shell", tenant + ":Changed") ?? Task.CompletedTask);
        }
    }
}
