using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell
{
    internal class ShellHostedService : BackgroundService
    {
        private static readonly TimeSpan MinIdleTime = TimeSpan.FromSeconds(1);

        private readonly IShellHost _shellHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, ShellIdentifier> _shellIdentifiers = new ConcurrentDictionary<string, ShellIdentifier>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _shellSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        private string _allShellsId;
        private bool _initialized;

        public ShellHostedService(
            IShellHost shellHost,
            IShellSettingsManager shellSettingsManager,
            IDistributedCache distributedCache,
            ILogger<ShellHostedService> logger)
        {
            _shellHost = shellHost;
            _shellSettingsManager = shellSettingsManager;
            _distributedCache = distributedCache;
            _logger = logger;

            shellHost.InitializedAsync += InitializedAsync;
            shellHost.ReleasedAsync += ReleasedAsync;
            shellHost.ReloadedAsync += ReloadedAsync;
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

                    var scope = await _shellHost.GetScopeAsync(ShellHelper.DefaultShellName);

                    await scope.UsingAsync(async scope =>
                    {
                        var distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

                        if (distributedCache is MemoryDistributedCache)
                        {
                            //return
                            ;
                        }

                        var allSettings = _shellHost.GetAllSettings();

                        var allShellsIdData = await distributedCache.GetAsync("ALL_SHELLS_ID");
                        var allShellsId = allShellsIdData != null ? Encoding.UTF8.GetString(allShellsIdData) : null;

                        if (_allShellsId != allShellsId)
                        {
                            _allShellsId = allShellsId;

                            var names = (await _shellSettingsManager.LoadSettingsNamesAsync()).Except(allSettings.Select(s => s.Name));

                            var newSettings = new List<ShellSettings>();

                            foreach (var name in names)
                            {
                                newSettings.Add(await _shellSettingsManager.LoadSettingsAsync(name));
                            }

                            allSettings = allSettings.Concat(newSettings);
                        }

                        foreach (var settings in allSettings)
                        {
                            var semaphore = _shellSemaphores.GetOrAdd(settings.Name, (name) => new SemaphoreSlim(1));

                            await semaphore.WaitAsync();

                            try
                            {
                                var releaseIdData = await distributedCache.GetAsync("RELEASE_ID_" + settings.Name);
                                var reloadIdData = await distributedCache.GetAsync("RELOAD_ID_" + settings.Name);

                                var releaseId = releaseIdData != null ? Encoding.UTF8.GetString(releaseIdData) : null;
                                var reloadId = reloadIdData != null ? Encoding.UTF8.GetString(reloadIdData) : null;

                                var shellIdentifier = _shellIdentifiers.GetOrAdd(settings.Name, name => new ShellIdentifier() { Name = name });

                                if (shellIdentifier.ReloadId != reloadId)
                                {
                                    shellIdentifier.ReloadId = reloadId;
                                    shellIdentifier.ReleaseId = releaseId;
                                    await _shellHost.ReloadShellContextAsync(settings, eventSink: true);
                                }
                                else if (shellIdentifier.ReleaseId != releaseId)
                                {
                                    shellIdentifier.ReleaseId = releaseId;
                                    await _shellHost.ReleaseShellContextAsync(settings, eventSink: true);
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

        public async Task InitializedAsync()
        {
            var scope = await _shellHost.GetScopeAsync(ShellHelper.DefaultShellName);

            await scope.UsingAsync(async scope =>
            {
                var distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

                if (distributedCache is MemoryDistributedCache)
                {
                    //return
                    ;
                }

                // Todo: Initialize _allShellsId before loading settings !

                var allShellsIdData = await distributedCache.GetAsync("ALL_SHELLS_ID");
                _allShellsId = allShellsIdData != null ? Encoding.UTF8.GetString(allShellsIdData) : null;

                foreach (var name in _shellHost.GetAllSettings().Select(s => s.Name))
                {
                    var releaseIdData = await distributedCache.GetAsync("RELEASE_ID_" + name);
                    var reloadIdData = await distributedCache.GetAsync("RELOAD_ID_" + name);

                    var releaseId = releaseIdData != null ? Encoding.UTF8.GetString(releaseIdData) : null;
                    var reloadId = reloadIdData != null ? Encoding.UTF8.GetString(reloadIdData) : null;

                    var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier() { Name = name });

                    shellIdentifier.ReleaseId = releaseId;
                    shellIdentifier.ReloadId = reloadId;
                }
            });

            _initialized = true;
        }

        public async Task ReleasedAsync(string name)
        {
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
                        //return
                        ;
                    }

                    var releaseId = IdGenerator.GenerateId();
                    var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier() { Name = name });
                    shellIdentifier.ReleaseId = releaseId;

                    await distributedCache.SetAsync("RELEASE_ID_", Encoding.UTF8.GetBytes(releaseId));
                });
            }
            finally
            {
                semaphore.Release();
                _shellSemaphores.TryRemove(name, out semaphore);
            }
        }

        public async Task ReloadedAsync(string name)
        {
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
                        //return
                        ;
                    }

                    if (!_shellIdentifiers.Keys.Any(n => n == name))
                    {
                        var allShellsIdData = await distributedCache.GetAsync("ALL_SHELLS_ID");
                        var previousAllShellsId = allShellsIdData != null ? Encoding.UTF8.GetString(allShellsIdData) : null;

                        var newAllShellsId = IdGenerator.GenerateId();
                        await distributedCache.SetAsync("ALL_SHELLS_ID", Encoding.UTF8.GetBytes(newAllShellsId));

                        if (_allShellsId == previousAllShellsId)
                        {
                            _allShellsId = newAllShellsId;
                        }
                    }

                    var reloadId = IdGenerator.GenerateId();
                    var shellIdentifier = _shellIdentifiers.GetOrAdd(name, name => new ShellIdentifier() { Name = name });
                    shellIdentifier.ReloadId = reloadId;

                    await distributedCache.SetAsync("RELOAD_ID_", Encoding.UTF8.GetBytes(reloadId));
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
