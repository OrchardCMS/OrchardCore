using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Hosting.ShellBuilders;

namespace OrchardCore.Modules
{
    internal class ModularBackgroundService : BackgroundService, IModularBackgroundService
    {
        private static TimeSpan PollingTime = TimeSpan.FromMinutes(1);
        private static TimeSpan MinIdleTime = TimeSpan.FromSeconds(10);

        private readonly ConcurrentDictionary<string, BackgroundTaskScheduler> _schedulers =
            new ConcurrentDictionary<string, BackgroundTaskScheduler>();

        private readonly ConcurrentDictionary<string, IChangeToken> _changeTokens =
            new ConcurrentDictionary<string, IChangeToken>();

        private readonly IShellHost _shellHost;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ModularBackgroundService(
            IShellHost shellHost,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ModularBackgroundService> logger)
        {
            _shellHost = shellHost;
            _httpContextAccessor = httpContextAccessor;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
            {
                Logger.LogDebug("'{ServiceName}' is stopping.", nameof(ModularBackgroundService));
            });

            while (GetRunningShells().Count() < 1)
            {
                await Task.Delay(MinIdleTime, stoppingToken);
            }

            var previousShells = Enumerable.Empty<ShellContext>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var runningShells = GetRunningShells();
                await UpdateAsync(previousShells, runningShells, stoppingToken);
                previousShells = runningShells;

                var pollingDelay = Task.Delay(PollingTime, stoppingToken);

                await RunAsync(runningShells, stoppingToken);
                await WaitAsync(pollingDelay, stoppingToken);
            }
        }

        private async Task RunAsync(IEnumerable<ShellContext> runningShells, CancellationToken stoppingToken)
        {
            await GetShellsToRun(runningShells).ForEachAsync(async shell =>
            {
                var tenant = shell.Settings.Name;

                var schedulers = GetSchedulersToRun(tenant);

                foreach (var scheduler in schedulers)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    using (var scope = _shellHost.EnterServiceScope(shell.Settings, out var context))
                    {
                        _httpContextAccessor.HttpContext.Update(shell);

                        if (scope == null || !context.IsActivated)
                        {
                            break;
                        }

                        var taskName = scheduler.Name;

                        var task = scope.GetTaskByTypeName(taskName);

                        if (task == null)
                        {
                            continue;
                        }

                        try
                        {
                            Logger.LogInformation("Start processing background task '{TaskName}' on tenant '{TenantName}'.", taskName, tenant);
                            scheduler.Run();

                            await task.DoWorkAsync(scope.ServiceProvider, stoppingToken);

                            scheduler.Idle();
                            Logger.LogInformation("Finished processing background task '{TaskName}' on tenant '{TenantName}'.", taskName, tenant);
                        }

                        catch (Exception e)
                        {
                            Logger.LogError(e, "Error while processing background task '{TaskName}' on tenant '{TenantName}'.", taskName, tenant);
                        }
                    }
                }
            });
        }

        private async Task UpdateAsync(IEnumerable<ShellContext> previousShells, IEnumerable<ShellContext> runningShells, CancellationToken stoppingToken)
        {
            var referenceTime = DateTime.UtcNow;

            await GetShellsToUpdate(previousShells, runningShells).ForEachAsync(async shell =>
            {
                var tenant = shell.Settings.Name;

                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                using (var scope = _shellHost.EnterServiceScope(shell.Settings, out var context))
                {
                    if (scope == null || !context.IsActivated)
                    {
                        return;
                    }

                    var taskTypes = scope.GetTaskTypes();
                    CleanSchedulers(tenant, taskTypes);

                    if (!taskTypes.Any())
                    {
                        return;
                    }

                    _changeTokens[tenant] = scope.GetTaskSettingsChangeToken();

                    foreach (var taskType in taskTypes)
                    {
                        var taskName = taskType.FullName;

                        if (!_schedulers.TryGetValue(tenant + taskName, out BackgroundTaskScheduler scheduler))
                        {
                            _schedulers[tenant + taskName] = scheduler = new BackgroundTaskScheduler(tenant, taskName, referenceTime);
                        }

                        if (!scheduler.Released && scheduler.Updated)
                        {
                            continue;
                        }

                        scheduler = scheduler.Clone();

                        try
                        {
                            var settings = await scope.GetTaskSettingsAsync(taskType);

                            if (scheduler.Released || !scheduler.Settings.Schedule.Equals(settings.Schedule))
                            {
                                scheduler.ReferenceTime = referenceTime;
                            }

                            scheduler.Settings = settings.Clone();
                            scheduler.Released = false;
                            scheduler.Updated = true;
                            scheduler.CanRun();
                        }

                        catch (Exception e)
                        {
                            Logger.LogError(e, "Error while updating settings of background task '{TaskName}' on tenant '{TenantName}'.", taskName, tenant);
                        }

                        finally
                        {
                            _schedulers[tenant + taskName] = scheduler;
                        }
                    }
                }
            });
        }

        private async Task WaitAsync(Task pollingDelay, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(MinIdleTime, stoppingToken);
                await pollingDelay;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task UpdateAsync(string tenant)
        {
            var shell = GetRunningShells().FirstOrDefault(s => s.Settings.Name == tenant);

            if (shell != null)
            {
                await UpdateAsync(Enumerable.Empty<ShellContext>(), new [] { shell }, CancellationToken.None);
            }
        }

        public async Task<BackgroundTaskSettings> GetSettingsAsync(string tenant, string taskName)
        {
            await UpdateAsync(tenant);

            if (_schedulers.TryGetValue(tenant + taskName, out BackgroundTaskScheduler scheduler))
            {
                return scheduler.Settings.Clone();
            }

            return BackgroundTaskSettings.None;
        }

        public async Task<IEnumerable<BackgroundTaskSettings>> GetSettingsAsync(string tenant)
        {
            await UpdateAsync(tenant);

            return _schedulers.Where(kv => kv.Value.Tenant == tenant)
                .Select(kv => kv.Value.Settings.Clone()).ToArray().AsEnumerable();
        }

        private IEnumerable<ShellContext> GetRunningShells()
        {
            return _shellHost.ListShellContexts().Where(s => s.Settings.State == TenantState.Running && s.IsActivated).ToArray();
        }

        private IEnumerable<ShellContext> GetShellsToRun(IEnumerable<ShellContext> shells)
        {
            var tenantsToRun = _schedulers.Where(s => s.Value.CanRun()).Select(s => s.Value.Tenant).Distinct().ToArray();
            return shells.Where(s => tenantsToRun.Contains(s.Settings.Name)).ToArray();
        }

        private IEnumerable<ShellContext> GetShellsToUpdate(IEnumerable<ShellContext> previousShells, IEnumerable<ShellContext> runningShells)
        {
            var released = previousShells.Where(s => s.Released).Select(s => s.Settings.Name).ToArray();

            if (released.Any())
            {
                UpdateSchedulers(released, s => s.Released = true);
            }

            var changed = _changeTokens.Where(t => t.Value.HasChanged).Select(t => t.Key).ToArray();

            if (changed.Any())
            {
                UpdateSchedulers(changed, s => s.Updated = false);
            }

            var valid = previousShells.Select(s => s.Settings.Name).Except(released).Except(changed);
            var tenantsToUpdate = runningShells.Select(s => s.Settings.Name).Except(valid).ToArray();

            return runningShells.Where(s => tenantsToUpdate.Contains(s.Settings.Name)).ToArray();
        }

        private IEnumerable<BackgroundTaskScheduler> GetSchedulersToRun(string tenant)
        {
            return _schedulers.Where(s => s.Value.Tenant == tenant && s.Value.CanRun()).Select(s => s.Value).ToArray();
        }

        private void UpdateSchedulers(IEnumerable<string> tenants, Action<BackgroundTaskScheduler> action)
        {
            var keys = _schedulers.Where(kv => tenants.Contains(kv.Value.Tenant)).Select(kv => kv.Key).ToArray();

            foreach (var key in keys)
            {
                if (_schedulers.TryGetValue(key, out BackgroundTaskScheduler scheduler))
                {
                    _schedulers[key] = scheduler.Clone(s => action(s));
                }
            }
        }

        private void CleanSchedulers(string tenant, IEnumerable<Type> taskTypes)
        {
            var validKeys = taskTypes.Select(type => tenant + type.FullName).ToArray();

            var keys = _schedulers.Where(kv => kv.Value.Tenant == tenant).Select(kv => kv.Key).ToArray();

            foreach (var key in keys)
            {
                if (!validKeys.Contains(key))
                {
                    _schedulers.TryRemove(key, out var scheduler);
                }
            }
        }
    }

    internal static class EnumerableExtensions
    {
        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body)
        {
            var partitionCount = System.Environment.ProcessorCount;

            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(partitionCount)
                select Task.Run(async delegate
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await body(partition.Current);
                        }
                    }
                }));
        }
    }

    internal static class HttpContextExtensions
    {
        public static void Update(this HttpContext httpContext, ShellContext shell)
        {
            httpContext.Request.Host = new HostString(shell.Settings.RequestUrlHost ?? "localhost");
            httpContext.Request.Path = "/" + shell.Settings.RequestUrlPrefix ?? "";
            httpContext.Items["IsBackground"] = true;
        }
    }

    internal static class ServiceScopeExtensions
    {
        public static IEnumerable<Type> GetTaskTypes(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetServices<IBackgroundTask>().Select(t => t.GetType());
        }

        public static IBackgroundTask GetTaskByTypeName(this IServiceScope scope, string type)
        {
            return scope.ServiceProvider.GetServices<IBackgroundTask>().FirstOrDefault(t => t.GetType().FullName == type);
        }

        public static async Task<BackgroundTaskSettings> GetTaskSettingsAsync(this IServiceScope scope, Type type)
        {
            var providers = scope.ServiceProvider.GetServices<IBackgroundTaskSettingsProvider>();

            foreach (var provider in providers.OrderBy(p => p.Order))
            {
                var settings = await provider.GetSettingsAsync(type);

                if (settings != null && settings != BackgroundTaskSettings.None)
                {
                    return settings;
                }
            }

            return new BackgroundTaskSettings() { Name = type.FullName };
        }

        public static IChangeToken GetTaskSettingsChangeToken(this IServiceScope scope)
        {
            var providers = scope.ServiceProvider.GetServices<IBackgroundTaskSettingsProvider>();

            var changeTokens = new List<IChangeToken>();
            foreach (var provider in providers.OrderBy(p => p.Order))
            {
                var changeToken = provider.ChangeToken;
                if (changeToken != null)
                {
                    changeTokens.Add(changeToken);
                }
            }

            if (changeTokens.Count == 0)
            {
                return NullChangeToken.Singleton;
            }

            return new CompositeChangeToken(changeTokens);
        }
    }
}