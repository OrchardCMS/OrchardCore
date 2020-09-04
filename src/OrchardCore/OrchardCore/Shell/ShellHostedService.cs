using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell
{
    internal class ShellHostedService : BackgroundService
    {
        private const string AllShellsIdKey = "ALL_SHELLS_ID";
        private const string ReleaseIdKeyPrefix = "RELEASE_ID_";
        private const string ReloadIdKeyPrefix = "RELOAD_ID_";

        private static readonly TimeSpan MinIdleTime = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan MaxBusyTime = TimeSpan.FromSeconds(1);

        private readonly IShellHost _shellHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, ShellState> _shellStates = new ConcurrentDictionary<string, ShellState>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _shellSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        private string _allShellsId;
        private bool _initialized;

        public ShellHostedService(
            IShellHost shellHost,
            IShellSettingsManager shellSettingsManager,
            ILogger<ShellHostedService> logger)
        {
            _shellHost = shellHost;
            _shellSettingsManager = shellSettingsManager;
            _logger = logger;

            shellHost.InitializingAsync += InitializingAsync;
            shellHost.ReleasingAsync += ReleasingAsync;
            shellHost.ReloadingAsync += ReloadingAsync;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
            {
                _logger.LogInformation("'{ServiceName}' is stopping.", nameof(ShellHostedService));
            });

            try
            {
                var maxBusyDelay = Task.CompletedTask;

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (!await TryWaitAsync(MinIdleTime, stoppingToken))
                    {
                        break;
                    }

                    if (!_initialized)
                    {
                        continue;
                    }

                    var scope = await _shellHost.GetScopeAsync(ShellHelper.DefaultShellName);

                    await scope.UsingAsync(async scope =>
                    {
                        var distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

                        if (distributedCache is MemoryDistributedCache)
                        {
                            return;
                        }

                        var allSettings = _shellHost.GetAllSettings();

                        var allShellsId = await distributedCache.GetStringAsync(AllShellsIdKey);

                        if (_allShellsId != allShellsId)
                        {
                            _allShellsId = allShellsId;

                            var names = (await _shellSettingsManager.LoadSettingsNamesAsync())
                                .Except(allSettings
                                .Select(s => s.Name));

                            var newSettings = new List<ShellSettings>();

                            foreach (var name in names)
                            {
                                newSettings.Add(await _shellSettingsManager.LoadSettingsAsync(name));
                            }

                            allSettings = allSettings.Concat(newSettings);
                        }

                        foreach (var settings in allSettings)
                        {
                            if (maxBusyDelay.IsCompleted)
                            {
                                if (!await TryWaitAsync(MinIdleTime, stoppingToken))
                                {
                                    break;
                                }

                                maxBusyDelay = Task.Delay(MaxBusyTime, stoppingToken);
                            }

                            var semaphore = _shellSemaphores.GetOrAdd(settings.Name, (name) => new SemaphoreSlim(1));

                            await semaphore.WaitAsync();

                            try
                            {
                                var shellState = _shellStates.GetOrAdd(settings.Name, name => new ShellState() { Name = name });

                                var releaseId = await distributedCache.GetStringAsync(ReleaseIdKeyPrefix + settings.Name);
                                var reloadId = await distributedCache.GetStringAsync(ReloadIdKeyPrefix + settings.Name);

                                if (releaseId != null && shellState.ReleaseId != releaseId)
                                {
                                    shellState.ReleaseId = releaseId;

                                    await _shellHost.ReleaseShellContextAsync(settings, eventSink: true);
                                }

                                if (reloadId != null && shellState.ReloadId != reloadId)
                                {
                                    shellState.ReloadId = reloadId;

                                    await _shellHost.ReloadShellContextAsync(settings, eventSink: true);
                                }
                            }
                            finally
                            {
                                semaphore.Release();
                                _shellSemaphores.TryRemove(settings.Name, out semaphore);
                            }
                        }
                    });
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Error while executing '{ServiceName}', the service is stopping.", nameof(ShellHostedService));
            }
        }

        public async Task InitializingAsync()
        {
            var names = await _shellSettingsManager.LoadSettingsNamesAsync();

            if (names.FirstOrDefault(n => n == ShellHelper.DefaultShellName) == null)
            {
                return;
            }

            var defaultSettings = await _shellSettingsManager.LoadSettingsAsync(ShellHelper.DefaultShellName);

            if (defaultSettings.State != TenantState.Running)
            {
                return;
            }

            var scope = await _shellHost.GetScopeAsync(defaultSettings);

            await scope.UsingAsync(async scope =>
            {
                var distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

                if (distributedCache is MemoryDistributedCache)
                {
                    return;
                }

                var _allShellsId = await distributedCache.GetStringAsync(AllShellsIdKey);

                foreach (var name in names)
                {
                    var shellState = _shellStates.GetOrAdd(name, name => new ShellState() { Name = name });

                    shellState.ReleaseId = await distributedCache.GetStringAsync(ReleaseIdKeyPrefix + name);
                    shellState.ReloadId = await distributedCache.GetStringAsync(ReloadIdKeyPrefix + name);
                }
            });

            _initialized = true;
        }

        public async Task ReleasingAsync(string name)
        {
            var semaphore = _shellSemaphores.GetOrAdd(name, (name) => new SemaphoreSlim(1));

            await semaphore.WaitAsync();

            try
            {
                var scope = await _shellHost.GetScopeAsync(ShellHelper.DefaultShellName);

                await scope.UsingAsync(scope =>
                {
                    var distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

                    if (distributedCache is MemoryDistributedCache)
                    {
                        return Task.CompletedTask;
                    }

                    var shellState = _shellStates.GetOrAdd(name, name => new ShellState() { Name = name });

                    shellState.ReleaseId = IdGenerator.GenerateId();

                    return distributedCache.SetStringAsync(ReleaseIdKeyPrefix + name, shellState.ReleaseId);
                });
            }
            finally
            {
                semaphore.Release();
                _shellSemaphores.TryRemove(name, out semaphore);
            }
        }

        public async Task ReloadingAsync(string name)
        {
            if (name == ShellHelper.DefaultShellName && !_initialized)
            {
                _initialized = true;

                return;
            }

            var semaphore = _shellSemaphores.GetOrAdd(name, (name) => new SemaphoreSlim(1));

            await semaphore.WaitAsync();

            try
            {
                var scope = await _shellHost.GetScopeAsync(ShellHelper.DefaultShellName);

                await scope.UsingAsync(async scope =>
                {
                    var distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

                    if (distributedCache is MemoryDistributedCache)
                    {
                        return;
                    }

                    var shellState = _shellStates.GetOrAdd(name, name => new ShellState() { Name = name });

                    shellState.ReloadId = IdGenerator.GenerateId();

                    await distributedCache.SetStringAsync(ReloadIdKeyPrefix + name, shellState.ReloadId);

                    if (name != ShellHelper.DefaultShellName && !_shellHost.TryGetSettings(name, out _))
                    {
                        await distributedCache.SetStringAsync(AllShellsIdKey, IdGenerator.GenerateId());
                    }
                });
            }
            finally
            {
                semaphore.Release();
                _shellSemaphores.TryRemove(name, out semaphore);
            }
        }

        private async Task<bool> TryWaitAsync(TimeSpan delay, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(delay, stoppingToken);

                return true;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        internal class ShellState
        {
            public string Name { get; set; }
            public string ReleaseId { get; set; }
            public string ReloadId { get; set; }
        }
    }
}
