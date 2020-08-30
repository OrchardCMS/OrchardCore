using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell
{
    internal class ShellHostedService : BackgroundService
    {
        private static readonly TimeSpan MinIdleTime = TimeSpan.FromSeconds(1);

        private readonly IShellHost _shellHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly ILogger _logger;

        private ConcurrentDictionary<string, Shell> _shells = new ConcurrentDictionary<string, Shell>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _shellSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private bool _initialized;

        public ShellHostedService(IShellHost shellHost, IShellSettingsManager shellSettingsManager, ILogger<ShellHostedService> logger)
        {
            shellHost.InitializedAsync += InitializedAsync;
            shellHost.ReleasedAsync += ReleasedAsync;
            shellHost.ReloadedAsync += ReloadedAsync;

            _shellHost = shellHost;
            _shellSettingsManager = shellSettingsManager;
            _logger = logger;
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

                    if (!_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defaultShellSettings) || defaultShellSettings.State == TenantState.Initializing)
                    {
                        continue;
                    }

                    await _semaphore.WaitAsync();

                    ShellScope scope;
                    try
                    {
                        scope = await _shellHost.GetScopeAsync(defaultShellSettings);
                    }
                    catch
                    {
                        continue;
                    }
                    finally
                    {
                        _semaphore.Release();
                    }

                    await scope.UsingAsync(async scope =>
                    {
                        var manager = scope.ServiceProvider.GetRequiredService<IVolatileDocumentManager<ShellsDocument>>();

                        var document = await manager.GetImmutableAsync();

                        foreach (var shell in document.Shells.Values)
                        {
                            var semaphore = _shellSemaphores.GetOrAdd(shell.Name, (name) => new SemaphoreSlim(1));

                            await semaphore.WaitAsync();

                            try
                            {
                                if (_shellHost.TryGetSettings(shell.Name, out var shellSettings))
                                {
                                    var localShell = _shells.GetOrAdd(shell.Name, new Shell() { Name = shell.Name });

                                    if (localShell.ReloadId != shell.ReloadId)
                                    {
                                        await _shellHost.ReloadShellContextAsync(shellSettings, eventSink: true);

                                        localShell.ReleaseId = shell.ReleaseId;
                                        localShell.ReloadId = shell.ReloadId;
                                    }
                                    else if (localShell.ReleaseId != shell.ReleaseId)
                                    {
                                        await _shellHost.ReleaseShellContextAsync(shellSettings, eventSink: true);

                                        localShell.ReleaseId = shell.ReleaseId;
                                    }
                                }
                                else
                                {
                                    shellSettings = await _shellSettingsManager.LoadSettingsAsync(shell.Name);
                                    await _shellHost.ReloadShellContextAsync(shellSettings, eventSink: true);

                                    var localShell = _shells.GetOrAdd(shell.Name, new Shell() { Name = shell.Name });

                                    localShell.ReleaseId = shell.ReleaseId;
                                    localShell.ReloadId = shell.ReloadId;
                                }
                            }
                            finally
                            {
                                semaphore.Release();
                                _shellSemaphores.TryRemove(shell.Name, out semaphore);
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
                var manager = scope.ServiceProvider.GetRequiredService<IVolatileDocumentManager<ShellsDocument>>();

                var shells = (await manager.GetImmutableAsync()).Shells.Values;

                foreach (var shell in shells)
                {
                    _shells[shell.Name] = new Shell()
                    {
                        Name = shell.Name,
                        ReleaseId = shell.ReleaseId,
                        ReloadId = shell.ReloadId
                    };
                }
            });

            _initialized = true;
        }

        public async Task ReleasedAsync(string name)
        {
            await _semaphore.WaitAsync();

            ShellScope scope;
            try
            {
                if (!_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defaultShellSettings) || defaultShellSettings.State == TenantState.Initializing)
                {
                    return;
                }

                scope = await _shellHost.GetScopeAsync(defaultShellSettings);
            }
            finally
            {
                _semaphore.Release();
            }

            var semaphore = _shellSemaphores.GetOrAdd(name, (name) => new SemaphoreSlim(1));

            await semaphore.WaitAsync();

            try
            {
                await scope.UsingAsync(async scope =>
                {
                    var manager = scope.ServiceProvider.GetRequiredService<IVolatileDocumentManager<ShellsDocument>>();

                    var document = await manager.GetMutableAsync();

                    if (document.Shells.TryGetValue(name, out var shell))
                    {
                        shell.ReleaseId = IdGenerator.GenerateId();
                    }
                    else
                    {
                        shell = new Shell()
                        {
                            Name = name,
                            ReleaseId = IdGenerator.GenerateId()
                        };

                        document.Shells[name] = shell;
                    }

                    await manager.UpdateAsync(document);

                    var localShell = _shells.GetOrAdd(shell.Name, name => new Shell() { Name = name });

                    localShell.ReleaseId = shell.ReleaseId;
                    localShell.ReloadId = shell.ReloadId;
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
            await _semaphore.WaitAsync();

            ShellScope scope;
            try
            {
                if (!_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defaultShellSettings) || defaultShellSettings.State == TenantState.Initializing)
                {
                    return;
                }

                scope = await _shellHost.GetScopeAsync(defaultShellSettings);
            }
            finally
            {
                _semaphore.Release();
            }

            var semaphore = _shellSemaphores.GetOrAdd(name, (name) => new SemaphoreSlim(1));

            await semaphore.WaitAsync();

            try
            {
                await scope.UsingAsync(async scope =>
                {
                    var manager = scope.ServiceProvider.GetRequiredService<IVolatileDocumentManager<ShellsDocument>>();

                    var document = await manager.GetMutableAsync();

                    if (document.Shells.TryGetValue(name, out var shell))
                    {
                        shell.ReloadId = IdGenerator.GenerateId();
                    }
                    else
                    {
                        shell = new Shell()
                        {
                            Name = name,
                            ReloadId = IdGenerator.GenerateId()
                        };

                        document.Shells[name] = shell;
                    }

                    await manager.UpdateAsync(document);

                    var localShell = _shells.GetOrAdd(shell.Name, name => new Shell() { Name = name });

                    localShell.ReleaseId = shell.ReleaseId;
                    localShell.ReloadId = shell.ReloadId;
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
    }
}
