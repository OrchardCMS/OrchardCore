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
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell
{
    internal class ShellHostedService : BackgroundService
    {
        private const string ShellsIdKey = "SHELLS_ID";
        private const string ReleaseIdKeyPrefix = "RELEASE_ID_";
        private const string ReloadIdKeyPrefix = "RELOAD_ID_";

        private static readonly TimeSpan MinIdleTime = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan MaxBusyTime = TimeSpan.FromSeconds(2);

        private readonly IShellHost _shellHost;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, ShellIdentifier> _shellIdentifiers = new ConcurrentDictionary<string, ShellIdentifier>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _shellSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        private bool _initialized;
        private string _shellsId;

        private ShellContext _defaultContext;
        private ShellContext _isolatedContext;

        public ShellHostedService(
            IShellHost shellHost,
            IShellContextFactory shellContextFactory,
            IShellSettingsManager shellSettingsManager,
            ILogger<ShellHostedService> logger)
        {
            _shellHost = shellHost;
            _shellContextFactory = shellContextFactory;
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

                    if (!_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defautSettings))
                    {
                        continue;
                    }

                    var defaultContext = await _shellHost.GetOrCreateShellContextAsync(defautSettings);

                    if (_defaultContext != defaultContext)
                    {
                        _isolatedContext?.Dispose();

                        _isolatedContext = await _shellContextFactory.CreateShellContextAsync(defautSettings);

                        _defaultContext = defaultContext;
                    }

                    await _isolatedContext.CreateScope().UsingAsync(async scope =>
                    {
                        var distributedCache = scope.ServiceProvider.GetService<IDistributedCache>();

                        if (distributedCache == null || distributedCache is MemoryDistributedCache)
                        {
                            return;
                        }

                        var allSettings = _shellHost.GetAllSettings();

                        var shellsId = await distributedCache.GetStringAsync(ShellsIdKey);

                        if (shellsId != null && _shellsId != shellsId)
                        {
                            _shellsId = shellsId;

                            var names = (await _shellSettingsManager.LoadSettingsNamesAsync())
                                .Except(allSettings.Select(s => s.Name));

                            var newSettings = new List<ShellSettings>();

                            foreach (var name in names)
                            {
                                newSettings.Add(await _shellSettingsManager.LoadSettingsAsync(name));
                            }

                            allSettings = allSettings.Concat(newSettings);
                        }

                        var startTime = DateTime.UtcNow;

                        foreach (var settings in allSettings)
                        {
                            var maxBusyTime = DateTime.UtcNow - startTime;

                            if (maxBusyTime > MaxBusyTime)
                            {
                                if (!await TryWaitAsync(MinIdleTime, stoppingToken))
                                {
                                    break;
                                }

                                startTime = DateTime.UtcNow;
                            }

                            var semaphore = _shellSemaphores.GetOrAdd(settings.Name, (name) => new SemaphoreSlim(1));

                            await semaphore.WaitAsync();

                            try
                            {
                                var releaseId = await distributedCache.GetStringAsync(ReleaseIdKeyPrefix + settings.Name);

                                if (releaseId != null)
                                {
                                    var shellIdentifier = _shellIdentifiers.GetOrAdd(settings.Name, name => new ShellIdentifier() { Name = name });

                                    if (shellIdentifier.ReleaseId != releaseId)
                                    {
                                        shellIdentifier.ReleaseId = releaseId;

                                        await _shellHost.ReleaseShellContextAsync(settings, eventSink: true);
                                    }
                                }

                                var reloadId = await distributedCache.GetStringAsync(ReloadIdKeyPrefix + settings.Name);

                                if (reloadId != null)
                                {
                                    var shellIdentifier = _shellIdentifiers.GetOrAdd(settings.Name, name => new ShellIdentifier() { Name = name });

                                    if (shellIdentifier.ReloadId != reloadId)
                                    {
                                        shellIdentifier.ReloadId = reloadId;

                                        await _shellHost.ReloadShellContextAsync(settings, eventSink: true);
                                    }
                                }
                            }
                            finally
                            {
                                semaphore.Release();
                                _shellSemaphores.TryRemove(settings.Name, out semaphore);
                            }
                        }
                    },
                    activate: false);
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Error while executing '{ServiceName}', the service is stopping.", nameof(ShellHostedService));
            }

            _isolatedContext?.Dispose();
        }

        public async Task InitializingAsync()
        {
            var names = await _shellSettingsManager.LoadSettingsNamesAsync();

            if (!names.Any(n => n == ShellHelper.DefaultShellName))
            {
                return;
            }

            var defautSettings = await _shellSettingsManager.LoadSettingsAsync(ShellHelper.DefaultShellName);

            if (defautSettings?.State != TenantState.Running)
            {
                return;
            }

            using var isolatedContext = await _shellContextFactory.CreateShellContextAsync(defautSettings);

            await isolatedContext.CreateScope().UsingAsync(async scope =>
            {
                var distributedCache = scope.ServiceProvider.GetService<IDistributedCache>();

                if (distributedCache == null || distributedCache is MemoryDistributedCache)
                {
                    return;
                }

                _shellsId = await distributedCache.GetStringAsync(ShellsIdKey);

                foreach (var name in names)
                {
                    var releaseId = await distributedCache.GetStringAsync(ReleaseIdKeyPrefix + name);

                    if (releaseId != null)
                    {
                        var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier() { Name = name });

                        shellIdentifier.ReleaseId = releaseId;
                    }

                    var reloadId = await distributedCache.GetStringAsync(ReloadIdKeyPrefix + name);

                    if (reloadId != null)
                    {
                        var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier() { Name = name });

                        shellIdentifier.ReloadId = reloadId;
                    }
                }
            },
            activate: false);

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
                    var distributedCache = scope.ServiceProvider.GetService<IDistributedCache>();

                    if (distributedCache == null || distributedCache is MemoryDistributedCache)
                    {
                        return Task.CompletedTask;
                    }

                    var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier() { Name = name });

                    shellIdentifier.ReleaseId = IdGenerator.GenerateId();

                    return distributedCache.SetStringAsync(ReleaseIdKeyPrefix + name, shellIdentifier.ReleaseId);
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
                    var distributedCache = scope.ServiceProvider.GetService<IDistributedCache>();

                    if (distributedCache == null || distributedCache is MemoryDistributedCache)
                    {
                        return;
                    }

                    var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier() { Name = name });

                    shellIdentifier.ReloadId = IdGenerator.GenerateId();

                    await distributedCache.SetStringAsync(ReloadIdKeyPrefix + name, shellIdentifier.ReloadId);

                    if (name != ShellHelper.DefaultShellName && !_shellHost.TryGetSettings(name, out _))
                    {
                        await distributedCache.SetStringAsync(ShellsIdKey, IdGenerator.GenerateId());
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

        internal class ShellIdentifier
        {
            public string Name { get; set; }
            public string ReleaseId { get; set; }
            public string ReloadId { get; set; }
        }
    }
}
