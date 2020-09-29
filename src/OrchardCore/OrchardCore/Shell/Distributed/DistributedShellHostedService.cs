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

namespace OrchardCore.Environment.Shell.Distributed
{
    internal class DistributedShellHostedService : BackgroundService
    {
        private const string ShellChangedIdKey = "SHELL_CHANGED_ID";
        private const string ShellCreatedIdKey = "SHELL_CREATED_ID";
        private const string ReleaseIdKeySuffix = "_RELEASE_ID";
        private const string ReloadIdKeySuffix = "_RELOAD_ID";

        private static readonly TimeSpan MinIdleTime = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan MaxBusyTime = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan MaxRetryTime = TimeSpan.FromMinutes(1);

        private readonly IShellHost _shellHost;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, ShellIdentifier> _shellIdentifiers = new ConcurrentDictionary<string, ShellIdentifier>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _shellSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        private string _shellChangedId;
        private string _shellCreatedId;

        private ShellContext _defaultContext;
        private IsolatedContext _isolatedContext;
        private bool _terminated;

        public DistributedShellHostedService(
            IShellHost shellHost,
            IShellContextFactory shellContextFactory,
            IShellSettingsManager shellSettingsManager,
            ILogger<DistributedShellHostedService> logger)
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
                _logger.LogInformation("'{ServiceName}' is stopping.", nameof(DistributedShellHostedService));
            });

            try
            {
                var minIdleTime = MinIdleTime;
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (!await TryWaitAsync(minIdleTime, stoppingToken))
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
                        _defaultContext = defaultContext;
                        var previousContext = _isolatedContext;

                        var isolatedContext = new IsolatedContext(await _shellContextFactory.CreateShellContextAsync(defaultContext.Settings));

                        lock (this)
                        {
                            _isolatedContext = isolatedContext.Acquire();
                            previousContext?.Dispose();
                        }
                    }

                    var context = _isolatedContext;

                    if (context.ServiceProvider.GetService<DistributedShellMarkerService>() == null)
                    {
                        continue;
                    }

                    var distributedCache = context.ServiceProvider.GetService<IDistributedCache>();
                    if (distributedCache == null || distributedCache is MemoryDistributedCache)
                    {
                        continue;
                    }

                    string shellChangedId, shellCreatedId;
                    try
                    {
                        shellChangedId = await distributedCache.GetStringAsync(ShellChangedIdKey);
                        shellCreatedId = await distributedCache.GetStringAsync(ShellCreatedIdKey);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        if (minIdleTime == MinIdleTime)
                        {
                            _logger.LogError(ex, "Unable to read the distributed cache before syncing all tenants.");
                        }

                        if (minIdleTime < MaxRetryTime)
                        {
                            minIdleTime *= 2;
                            if (minIdleTime > MaxRetryTime)
                            {
                                minIdleTime = MaxRetryTime;
                            }
                        }

                        continue;
                    }

                    minIdleTime = MinIdleTime;

                    if (shellChangedId == null || _shellChangedId == shellChangedId)
                    {
                        continue;
                    }

                    _shellChangedId = shellChangedId;
                    var allSettings = _shellHost.GetAllSettings();

                    if (shellCreatedId != null && _shellCreatedId != shellCreatedId)
                    {
                        _shellCreatedId = shellCreatedId;
                        var createdSettings = new List<ShellSettings>();

                        var names = (await _shellSettingsManager.LoadSettingsNamesAsync()).Except(allSettings.Select(s => s.Name));
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
                        catch (Exception ex) when (!ex.IsFatal())
                        {
                            _logger.LogError(ex, "Unable to read the distributed cache while syncing the tenant '{TenantName}'.", settings.Name);
                            break;
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
                _logger.LogError(ex, "Error while executing '{ServiceName}', the service is stopping.", nameof(DistributedShellHostedService));
            }

            lock (this)
            {
                _isolatedContext?.Dispose();
                _isolatedContext = null;
            }

            _defaultContext = null;
            _terminated = true;
        }

        public async Task LoadingAsync()
        {
            if (_terminated)
            {
                return;
            }

            var defautSettings = await _shellSettingsManager.LoadSettingsAsync(ShellHelper.DefaultShellName);
            if (defautSettings?.State != TenantState.Running)
            {
                return;
            }

            using var context = await _shellContextFactory.CreateShellContextAsync(defautSettings);
            if (context.ServiceProvider.GetService<DistributedShellMarkerService>() == null)
            {
                return;
            }

            var distributedCache = context.ServiceProvider.GetService<IDistributedCache>();
            if (distributedCache == null || distributedCache is MemoryDistributedCache)
            {
                return;
            }

            try
            {
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
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Unable to read the distributed cache before loading all tenants.");
            }
        }

        public async Task ReleasingAsync(string name)
        {
            if (_terminated)
            {
                return;
            }

            if (!_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defautSettings))
            {
                return;
            }

            if (defautSettings.State != TenantState.Running)
            {
                return;
            }

            IsolatedContext isolatedContext;
            lock (this)
            {
                isolatedContext = _isolatedContext?.Acquire();
            }

            if (isolatedContext == null)
            {
                isolatedContext = new IsolatedContext(await _shellContextFactory.CreateShellContextAsync(defautSettings)).Acquire();
            }

            using var context = isolatedContext;
            if (context.ServiceProvider.GetService<DistributedShellMarkerService>() == null)
            {
                return;
            }

            var distributedCache = context.ServiceProvider.GetService<IDistributedCache>();
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
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Unable to update the distributed cache before releasing the tenant '{TenantName}'.", name);
            }
            finally
            {
                semaphore.Release();
                _shellSemaphores.TryRemove(name, out semaphore);
            }
        }

        public async Task ReloadingAsync(string name)
        {
            if (_terminated)
            {
                return;
            }

            if (!_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defautSettings))
            {
                return;
            }

            if (defautSettings.State != TenantState.Running)
            {
                return;
            }

            IsolatedContext isolatedContext;
            lock (this)
            {
                isolatedContext = _isolatedContext?.Acquire();
            }

            if (isolatedContext == null)
            {
                isolatedContext = new IsolatedContext(await _shellContextFactory.CreateShellContextAsync(defautSettings)).Acquire();
            }

            using var context = isolatedContext;
            if (context.ServiceProvider.GetService<DistributedShellMarkerService>() == null)
            {
                return;
            }

            var distributedCache = context.ServiceProvider.GetService<IDistributedCache>();
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
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Unable to update the distributed cache before reloading the tenant '{TenantName}'.", name);
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

        internal class IsolatedContext : IDisposable
        {
            private ShellContext _context;
            internal volatile int _count;
            private bool _disposed;

            public IsolatedContext(ShellContext context)
            {
                _context = context;
            }

            public IServiceProvider ServiceProvider => _context.ServiceProvider;

            public IsolatedContext Acquire()
            {
                if (_disposed)
                {
                    return null;
                }

                Interlocked.Increment(ref _count);
                return this;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                if (Interlocked.CompareExchange(ref _count, 1, 1) == 1)
                {
                    _context.Dispose();
                    _disposed = true;
                }

                Interlocked.Decrement(ref _count);
            }
        }
    }
}
