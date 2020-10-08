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
    /// <summary>
    /// Keep in sync tenants by sharing shell identifiers through an <see cref="IDistributedCache"/>.
    /// </summary>
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
                    // Waiting for the idle time on each loop.
                    if (!await TryWaitAsync(minIdleTime, stoppingToken))
                    {
                        break;
                    }

                    // If there is no default tenant, nothing to do.
                    if (!_shellHost.TryGetShellContext(ShellHelper.DefaultShellName, out var defaultContext))
                    {
                        continue;
                    }

                    // If the default tenant is not running, nothing to do.
                    if (defaultContext.Settings.State != TenantState.Running)
                    {
                        continue;
                    }

                    // Check if the default tenant has changed locally.
                    if (_defaultContext != defaultContext)
                    {
                        _defaultContext = defaultContext;
                        var previousContext = _isolatedContext;

                        // Build a new isolated context based on the settings of the default tenant.
                        var isolatedContext = new IsolatedContext(await _shellContextFactory.CreateShellContextAsync(defaultContext.Settings));

                        lock (this)
                        {
                            // Acquire this context and dispose the previous one.
                            _isolatedContext = isolatedContext.Acquire();
                            previousContext?.Dispose();
                        }
                    }

                    var context = _isolatedContext;

                    // If the distributed shell feature is not enabled, nothing to do.
                    if (context.ServiceProvider.GetService<DistributedShellMarkerService>() == null)
                    {
                        continue;
                    }

                    // If the current distributed cache is an in memery cache, nothing to do.
                    var distributedCache = context.ServiceProvider.GetService<IDistributedCache>();
                    if (distributedCache == null || distributedCache is MemoryDistributedCache)
                    {
                        continue;
                    }

                    // Try to retrieve the tenant global identifiers from the distributed cache.
                    string shellChangedId, shellCreatedId;
                    try
                    {
                        shellChangedId = await distributedCache.GetStringAsync(ShellChangedIdKey);
                        shellCreatedId = await distributedCache.GetStringAsync(ShellCreatedIdKey);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        // Log the error only once.
                        if (minIdleTime == MinIdleTime)
                        {
                            _logger.LogError(ex, "Unable to read the distributed cache before syncing all tenants.");
                        }

                        // Retry but with a longer idle time.
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

                    // Check if at least one tenant has changed.
                    if (shellChangedId == null || _shellChangedId == shellChangedId)
                    {
                        continue;
                    }

                    _shellChangedId = shellChangedId;
                    var allSettings = _shellHost.GetAllSettings();

                    // Check if at least one tenant has been created.
                    if (shellCreatedId != null && _shellCreatedId != shellCreatedId)
                    {
                        _shellCreatedId = shellCreatedId;
                        var createdSettings = new List<ShellSettings>();

                        // The new tenants are those that are not already loaded.
                        var names = (await _shellSettingsManager.LoadSettingsNamesAsync()).Except(allSettings.Select(s => s.Name));
                        foreach (var name in names)
                        {
                            // Load the settings of each new tenant.
                            createdSettings.Add(await _shellSettingsManager.LoadSettingsAsync(name));
                        }

                        // Add all newly loaded settings to the list.
                        allSettings = allSettings.Concat(createdSettings);
                    }

                    var startTime = DateTime.UtcNow;

                    // Check specific identifiers of all tenants.
                    foreach (var settings in allSettings)
                    {
                        // If busy for a too long time.
                        var maxBusyTime = DateTime.UtcNow - startTime;
                        if (maxBusyTime > MaxBusyTime)
                        {
                            // Wait again for the idle time.
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
                            // Try to retrieve the release identifier of this tenant from the distributed cache.
                            var releaseId = await distributedCache.GetStringAsync(settings.Name + ReleaseIdKeySuffix);
                            if (releaseId != null)
                            {
                                // Check if the release identifier of this tenant has changed.
                                var shellIdentifier = _shellIdentifiers.GetOrAdd(settings.Name, name => new ShellIdentifier());
                                if (shellIdentifier.ReleaseId != releaseId)
                                {
                                    // Upate the local identifier.
                                    shellIdentifier.ReleaseId = releaseId;

                                    // Keep in sync this tenant by releasing it locally.
                                    await _shellHost.ReleaseShellContextAsync(settings, eventSource: false);
                                }
                            }

                            // Try to retrieve the reload identifier of this tenant from the distributed cache.
                            var reloadId = await distributedCache.GetStringAsync(settings.Name + ReloadIdKeySuffix);
                            if (reloadId != null)
                            {
                                // Check if the reload identifier of this tenant has changed.
                                var shellIdentifier = _shellIdentifiers.GetOrAdd(settings.Name, name => new ShellIdentifier());
                                if (shellIdentifier.ReloadId != reloadId)
                                {
                                    // Upate the local identifier.
                                    shellIdentifier.ReloadId = reloadId;

                                    // Keep in sync this tenant by reloading it locally.
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

            // If there is no default tenant or it is not running, nothing to do.
            var defautSettings = await _shellSettingsManager.LoadSettingsAsync(ShellHelper.DefaultShellName);
            if (defautSettings?.State != TenantState.Running)
            {
                return;
            }

            // If the distributed shell feature is not enabled, nothing to do.
            using var context = await _shellContextFactory.CreateShellContextAsync(defautSettings);
            if (context.ServiceProvider.GetService<DistributedShellMarkerService>() == null)
            {
                return;
            }

            // If the current distributed cache is an in memery cache, nothing to do.
            var distributedCache = context.ServiceProvider.GetService<IDistributedCache>();
            if (distributedCache == null || distributedCache is MemoryDistributedCache)
            {
                return;
            }

            try
            {
                // Retrieve the tenant global identifiers from the distributed cache.
                _shellChangedId = await distributedCache.GetStringAsync(ShellChangedIdKey);
                _shellCreatedId = await distributedCache.GetStringAsync(ShellCreatedIdKey);

                // Retrieve the names of all the tenants.
                var names = await _shellSettingsManager.LoadSettingsNamesAsync();
                foreach (var name in names)
                {
                    // Retrieve the release identifier of this tenant from the distributed cache.
                    var releaseId = await distributedCache.GetStringAsync(name + ReleaseIdKeySuffix);
                    if (releaseId != null)
                    {
                        // Initialize the release identifier of this tenant in the local collection.
                        var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier());
                        shellIdentifier.ReleaseId = releaseId;
                    }

                    // Retrieve the reload identifier of this tenant from the distributed cache.
                    var reloadId = await distributedCache.GetStringAsync(name + ReloadIdKeySuffix);
                    if (reloadId != null)
                    {
                        // Initialize the reload identifier of this tenant in the local collection.
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

            // If there is no default tenant, nothing to do.
            if (!_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defautSettings))
            {
                return;
            }

            // If the default tenant is not running, nothing to do.
            if (defautSettings.State != TenantState.Running)
            {
                return;
            }

            IsolatedContext isolatedContext;
            lock (this)
            {
                // Try to acquire the isolated context.
                isolatedContext = _isolatedContext?.Acquire();
            }

            if (isolatedContext == null)
            {
                // Or create a new one if not yet initialized by the main loop.
                isolatedContext = new IsolatedContext(await _shellContextFactory.CreateShellContextAsync(defautSettings)).Acquire();
            }

            using var context = isolatedContext;

            // If the distributed shell feature is not enabled, nothing to do.
            if (context.ServiceProvider.GetService<DistributedShellMarkerService>() == null)
            {
                return;
            }

            // If the current distributed cache is an in memery cache, nothing to do.
            var distributedCache = context.ServiceProvider.GetService<IDistributedCache>();
            if (distributedCache == null || distributedCache is MemoryDistributedCache)
            {
                return;
            }

            var semaphore = _shellSemaphores.GetOrAdd(name, name => new SemaphoreSlim(1));
            await semaphore.WaitAsync();
            try
            {
                // Update this tenant in the local collection with a new release identifier.
                var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier());
                shellIdentifier.ReleaseId = IdGenerator.GenerateId();

                // Update the release identifier of this tenant in the distributed cache.
                await distributedCache.SetStringAsync(name + ReleaseIdKeySuffix, shellIdentifier.ReleaseId);

                // Also update the global identifier telling that a tenant has been changed.
                await distributedCache.SetStringAsync(ShellChangedIdKey, shellIdentifier.ReleaseId);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Unable to update the distributed cache before releasing the tenant '{TenantName}'.", name);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task ReloadingAsync(string name)
        {
            if (_terminated)
            {
                return;
            }

            // If there is no default tenant, nothing to do.
            if (!_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defautSettings))
            {
                return;
            }

            // If the default tenant is not running, nothing to do.
            if (defautSettings.State != TenantState.Running)
            {
                return;
            }

            IsolatedContext isolatedContext;
            lock (this)
            {
                // Try to acquire the isolated context.
                isolatedContext = _isolatedContext?.Acquire();
            }

            if (isolatedContext == null)
            {
                // Or create a new one if not yet initialized by the main loop.
                isolatedContext = new IsolatedContext(await _shellContextFactory.CreateShellContextAsync(defautSettings)).Acquire();
            }

            using var context = isolatedContext;

            // If the distributed shell feature is not enabled, nothing to do.
            if (context.ServiceProvider.GetService<DistributedShellMarkerService>() == null)
            {
                return;
            }

            // If the current distributed cache is an in memery cache, nothing to do.
            var distributedCache = context.ServiceProvider.GetService<IDistributedCache>();
            if (distributedCache == null || distributedCache is MemoryDistributedCache)
            {
                return;
            }

            var semaphore = _shellSemaphores.GetOrAdd(name, name => new SemaphoreSlim(1));
            await semaphore.WaitAsync();
            try
            {
                // Update this tenant in the local collection with a new reload identifier.
                var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier());
                shellIdentifier.ReloadId = IdGenerator.GenerateId();

                // Update the reload identifier of this tenant in the distributed cache.
                await distributedCache.SetStringAsync(name + ReloadIdKeySuffix, shellIdentifier.ReloadId);

                // Check if it is a new created tenant that has not been already loaded.
                if (name != ShellHelper.DefaultShellName && !_shellHost.TryGetSettings(name, out _))
                {
                    // Also update the global identifier telling that a tenant has been created.
                    await distributedCache.SetStringAsync(ShellCreatedIdKey, shellIdentifier.ReloadId);
                }

                // Also update the global identifier telling that a tenant has been changed.
                await distributedCache.SetStringAsync(ShellChangedIdKey, shellIdentifier.ReloadId);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Unable to update the distributed cache before reloading the tenant '{TenantName}'.", name);
            }
            finally
            {
                semaphore.Release();
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

            public IsolatedContext(ShellContext context)
            {
                _context = context;
            }

            public IServiceProvider ServiceProvider => _context.ServiceProvider;

            public IsolatedContext Acquire()
            {
                Interlocked.Increment(ref _count);
                return this;
            }

            public void Dispose()
            {
                if (Interlocked.Decrement(ref _count) == 0)
                {
                    _context.Dispose();
                }
            }
        }
    }
}
