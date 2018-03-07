using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Hosting.ShellBuilders;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks
{
    public class HostedBackgroundService : BackgroundService
    {
        private Dictionary<string, Scheduler> _schedulers = new Dictionary<string, Scheduler>();

        private readonly IShellHost _shellHost;

        public HostedBackgroundService(
            IShellHost shellHost,
            ILogger<HostedBackgroundService> logger)
        {
            _shellHost = shellHost;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => Logger.LogDebug($"HostedBackgroundService is stopping."));

            var referenceTime = DateTime.UtcNow;

            while (!cancellationToken.IsCancellationRequested)
            {
                var shellContexts = GetShellContexts();

                foreach (var shellContext in shellContexts)
                {
                    if (shellContext.Released || shellContext.Settings?.State != TenantState.Running)
                    {
                        continue;
                    }

                    var tenant = shellContext.Settings.Name;

                    IEnumerable<Type> taskTypes;

                    using (var scope = shellContext.EnterServiceScope())
                    {
                        taskTypes = scope.GetBackgroundTaskTypes();
                    }

                    foreach (var taskType in taskTypes)
                    {
                        var taskName = taskType.FullName;

                        using (var scope = shellContext.EnterServiceScope())
                        {
                            var task = scope.GetBackgroundTaskOfType(taskType);

                            if (task == null)
                            {
                                continue;
                            }

                            if (!_schedulers.TryGetValue(tenant + taskName, out Scheduler scheduler))
                            {
                                _schedulers[tenant + taskName] = scheduler =
                                    new Scheduler(tenant, task, referenceTime);
                            }

                            if (!scheduler.ShouldRun())
                            {
                                continue;
                            }

                            try
                            {
                                if (Logger.IsEnabled(LogLevel.Information))
                                {
                                    Logger.LogInformation(
                                        "Start processing background task \"{0}\" on tenant \"{1}\".",
                                        tenant, taskName);
                                }

                                shellContext.EnsureActivated(scope);
                                shellContext.RequestStarted();

                                await task.DoWorkAsync(scope.ServiceProvider, cancellationToken);

                                if (Logger.IsEnabled(LogLevel.Information))
                                {
                                    Logger.LogInformation(
                                        "Finished processing background task \"{0}\" on tenant \"{1}\".",
                                        tenant, taskName);
                                }
                            }

                            catch (Exception ex)
                            {
                                if (Logger.IsEnabled(LogLevel.Error))
                                {
                                    Logger.LogError(ex,
                                        "Error while processing background task \"{0}\" on tenant \"{1}\".",
                                        tenant, taskName);
                                }
                            }

                            finally
                            {
                                shellContext.RequestEnded();

                                if (shellContext.CanTerminate)
                                {
                                    await shellContext.TerminateAsync(scope);
                                    shellContext.Dispose();
                                }
                            }
                        }

                        if (shellContext.Released)
                        {
                            break;
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        private IEnumerable<ShellContext> GetShellContexts()
        {
            var shellContexts = _shellHost.ListShellContexts()?
                .OrderBy(x => x.Settings.Name) ?? Enumerable.Empty<ShellContext>();

            var tenants = shellContexts.Select(s => s.Settings.Name);
            CleanSchedulers(tenants);

            return shellContexts;
        }

        private void CleanSchedulers(IEnumerable<string> tenants)
        {
            var schedulers = _schedulers.Where(kv => !tenants.Contains(kv.Value.Tenant)).Select(kv => kv.Key);

            foreach (var scheduler in schedulers)
            {
                _schedulers.Remove(scheduler);
            }
        }

        private class Scheduler
        {
            public Scheduler(string tenant, IBackgroundTask task, DateTime startUtc)
            {
                Tenant = tenant;
                var attribute = task.GetType().GetCustomAttribute<BackgroundTaskAttribute>();
                Schedule = attribute?.Schedule ?? "* * * * *";
                StartUtc = startUtc;
            }

            public string Tenant { get; }
            public string Schedule { get; }
            public DateTime StartUtc { get; set; }

            public bool ShouldRun()
            {
                if (DateTime.UtcNow > CrontabSchedule.Parse(Schedule).GetNextOccurrence(StartUtc))
                {
                    StartUtc = DateTime.UtcNow;
                    return true;
                }

                return false;
            }
        }
    }

    internal static class ServiceScopeExtensions
    {
        public static IEnumerable<Type> GetBackgroundTaskTypes(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetServices<IBackgroundTask>().Select(t => t.GetType());
        }

        public static IBackgroundTask GetBackgroundTaskOfType(this IServiceScope scope, Type type)
        {
            return scope.ServiceProvider.GetServices<IBackgroundTask>().FirstOrDefault(t => t.GetType() == type);
        }
    }

    internal static class ShellContextExtensions
    {
        public static void EnsureActivated(this ShellContext shellContext, IServiceScope scope)
        {
            if (!shellContext.IsActivated || shellContext.IsActivating)
            {
                lock (shellContext)
                {
                    if (!shellContext.IsActivated)
                    {
                        shellContext.IsActivating = true;

                        var tenantEvents = scope.ServiceProvider.GetServices<IModularTenantEvents>();

                        foreach (var tenantEvent in tenantEvents)
                        {
                            tenantEvent.ActivatingAsync().Wait();
                        }

                        shellContext.IsActivated = true;

                        foreach (var tenantEvent in tenantEvents)
                        {
                            tenantEvent.ActivatedAsync().Wait();
                        }

                        shellContext.IsActivating = false;
                    }
                }
            }
        }

        public static async Task TerminateAsync(this ShellContext shellContext, IServiceScope scope)
        {
            var tenantEvents = scope.ServiceProvider.GetServices<IModularTenantEvents>();

            foreach (var tenantEvent in tenantEvents)
            {
                await tenantEvent.TerminatingAsync();
            }

            foreach (var tenantEvent in tenantEvents.Reverse())
            {
                await tenantEvent.TerminatedAsync();
            }
        }
    }
}