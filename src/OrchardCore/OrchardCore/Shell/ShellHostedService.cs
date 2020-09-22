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
        private const string ShellChangedIdKey = "SHELL_CHANGED_ID";
        private const string ShellCreatedIdKey = "SHELL_CREATED_ID";
        private const string ReleaseIdKeySuffix = "_RELEASE_ID";
        private const string ReloadIdKeySuffix = "_RELOAD_ID";

        private static readonly TimeSpan MinIdleTime = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan MaxBusyTime = TimeSpan.FromSeconds(2);

        private readonly IShellHost _shellHost;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, ShellIdentifier> _shellIdentifiers = new ConcurrentDictionary<string, ShellIdentifier>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _shellSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        private string _shellChangedId;
        private string _shellCreatedId;

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

            shellHost.LoadingAsync += LoadingAsync;
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

                    if (!_shellHost.TryGetShellContext(ShellHelper.DefaultShellName, out var defaultContext))
                    {
                        continue;
                    }

                    if (defaultContext.Settings.State != TenantState.Running)
                    {
                        continue;
                    }

                    if (_defaultContext != defaultContext)
                    {
                        _isolatedContext?.Dispose();

                        _isolatedContext = await _shellContextFactory.CreateShellContextAsync(defaultContext.Settings);

                        _defaultContext = defaultContext;
                    }

                    var distributedCache = _isolatedContext.ServiceProvider.GetService<IDistributedCache>();

                    if (distributedCache == null || distributedCache is MemoryDistributedCache)
                    {
                        continue;
                    }

                    var shellChangedId = await distributedCache.GetStringAsync(ShellChangedIdKey);

                    if (shellChangedId == null || _shellChangedId == shellChangedId)
                    {
                        continue;
                    }

                    _shellChangedId = shellChangedId;

                    var allSettings = _shellHost.GetAllSettings();

                    var shellCreatedId = await distributedCache.GetStringAsync(ShellCreatedIdKey);

                    if (shellCreatedId != null && _shellCreatedId != shellCreatedId)
                    {
                        _shellCreatedId = shellCreatedId;

                        var names = (await _shellSettingsManager.LoadSettingsNamesAsync()).Except(allSettings.Select(s => s.Name));

                        var createdSettings = new List<ShellSettings>();

                        foreach (var name in names)
                        {
                            createdSettings.Add(await _shellSettingsManager.LoadSettingsAsync(name));
                        }

                        allSettings = allSettings.Concat(createdSettings);
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

                        var semaphore = _shellSemaphores.GetOrAdd(settings.Name, name => new SemaphoreSlim(1));

                        await semaphore.WaitAsync();

                        try
                        {
                            var releaseId = await distributedCache.GetStringAsync(settings.Name + ReleaseIdKeySuffix);

                            if (releaseId != null)
                            {
                                var shellIdentifier = _shellIdentifiers.GetOrAdd(settings.Name, name => new ShellIdentifier());

                                if (shellIdentifier.ReleaseId != releaseId)
                                {
                                    shellIdentifier.ReleaseId = releaseId;

                                    await _shellHost.ReleaseShellContextAsync(settings, eventSource: false);
                                }
                            }

                            var reloadId = await distributedCache.GetStringAsync(settings.Name + ReloadIdKeySuffix);

                            if (reloadId != null)
                            {
                                var shellIdentifier = _shellIdentifiers.GetOrAdd(settings.Name, name => new ShellIdentifier());

                                if (shellIdentifier.ReloadId != reloadId)
                                {
                                    shellIdentifier.ReloadId = reloadId;

                                    await _shellHost.ReloadShellContextAsync(settings, eventSource: false);
                                }
                            }
                        }
                        finally
                        {
                            semaphore.Release();
                            _shellSemaphores.TryRemove(settings.Name, out semaphore);
                        }
                    }
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Error while executing '{ServiceName}', the service is stopping.", nameof(ShellHostedService));
            }

            _isolatedContext?.Dispose();

            _defaultContext = null;
        }

        public async Task LoadingAsync()
        {
            var defautSettings = await _shellSettingsManager.LoadSettingsAsync(ShellHelper.DefaultShellName);

            if (defautSettings?.State != TenantState.Running)
            {
                return;
            }

            using var isolatedContext = await _shellContextFactory.CreateShellContextAsync(defautSettings);

            var distributedCache = isolatedContext.ServiceProvider.GetService<IDistributedCache>();

            if (distributedCache == null || distributedCache is MemoryDistributedCache)
            {
                return;
            }

            _shellChangedId = await distributedCache.GetStringAsync(ShellChangedIdKey);

            _shellCreatedId = await distributedCache.GetStringAsync(ShellCreatedIdKey);

            var names = await _shellSettingsManager.LoadSettingsNamesAsync();

            foreach (var name in names)
            {
                var releaseId = await distributedCache.GetStringAsync(name + ReleaseIdKeySuffix);

                if (releaseId != null)
                {
                    var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier());

                    shellIdentifier.ReleaseId = releaseId;
                }

                var reloadId = await distributedCache.GetStringAsync(name + ReloadIdKeySuffix);

                if (reloadId != null)
                {
                    var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier());

                    shellIdentifier.ReloadId = reloadId;
                }
            }
        }

        public async Task ReleasingAsync(string name)
        {
            if (!_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defautSettings))
            {
                return;
            }

            if (defautSettings.State != TenantState.Running)
            {
                return;
            }

            using var isolatedContext = await _shellContextFactory.CreateShellContextAsync(defautSettings);

            var distributedCache = isolatedContext.ServiceProvider.GetService<IDistributedCache>();

            if (distributedCache == null || distributedCache is MemoryDistributedCache)
            {
                return;
            }

            var semaphore = _shellSemaphores.GetOrAdd(name, name => new SemaphoreSlim(1));

            await semaphore.WaitAsync();

            try
            {
                var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier());

                shellIdentifier.ReleaseId = IdGenerator.GenerateId();

                await distributedCache.SetStringAsync(name + ReleaseIdKeySuffix, shellIdentifier.ReleaseId);

                await distributedCache.SetStringAsync(ShellChangedIdKey, shellIdentifier.ReleaseId);
            }
            finally
            {
                semaphore.Release();
                _shellSemaphores.TryRemove(name, out semaphore);
            }
        }

        public async Task ReloadingAsync(string name)
        {
            if (!_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defautSettings))
            {
                return;
            }

            if (defautSettings.State != TenantState.Running)
            {
                return;
            }

            using var isolatedContext = await _shellContextFactory.CreateShellContextAsync(defautSettings);

            var distributedCache = isolatedContext.ServiceProvider.GetService<IDistributedCache>();

            if (distributedCache == null || distributedCache is MemoryDistributedCache)
            {
                return;
            }

            var semaphore = _shellSemaphores.GetOrAdd(name, name => new SemaphoreSlim(1));

            await semaphore.WaitAsync();

            try
            {
                var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier());

                shellIdentifier.ReloadId = IdGenerator.GenerateId();

                await distributedCache.SetStringAsync(name + ReloadIdKeySuffix, shellIdentifier.ReloadId);

                if (name != ShellHelper.DefaultShellName && !_shellHost.TryGetSettings(name, out _))
                {
                    await distributedCache.SetStringAsync(ShellCreatedIdKey, shellIdentifier.ReloadId);
                }

                await distributedCache.SetStringAsync(ShellChangedIdKey, shellIdentifier.ReloadId);
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
            public string ReleaseId { get; set; }
            public string ReloadId { get; set; }
        }
    }
}
