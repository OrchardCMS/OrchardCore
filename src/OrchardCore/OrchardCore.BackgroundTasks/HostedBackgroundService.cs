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

                CleanSchedulers();

                foreach (var shellContext in shellContexts)
                {
                    if (!shellContext.IsRunning())
                    {
                        continue;
                    }

                    IEnumerable<Type> taskTypes;
                    using (var scope = shellContext.EnterServiceScope())
                    {
                        taskTypes = scope.GetBackgroundTaskTypes();
                    }

                    var tenant = shellContext.Settings.Name;

                    foreach (var taskType in taskTypes)
                    {
                        var taskName = taskType.FullName;


                        if (shellContext.Released)
                        {
                            break;
                        }

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
            return _shellHost.ListShellContexts()?.OrderBy(x => x.Settings.Name) ?? Enumerable.Empty<ShellContext>();
        }

        private void CleanSchedulers()
        {
            var schedulers = _schedulers.Where(kv => !kv.Value.ShellContext.IsRunning()).Select(kv => kv.Key);

            foreach (var scheduler in schedulers)
            {
                _schedulers.Remove(scheduler);
            }
        }

        private class Scheduler
        {
            public Scheduler(ShellContext shellContext, IBackgroundTask task, DateTime startUtc)
            {
                ShellContext = shellContext;
                var attribute = task.GetType().GetCustomAttribute<BackgroundTaskAttribute>();
                Schedule = attribute?.Schedule ?? "* * * * *";
                StartUtc = startUtc;
            }

            public ShellContext ShellContext { get; }
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
        public static bool IsRunning(this ShellContext shellContext)
        {
            return !shellContext.Released && shellContext.Settings?.State == TenantState.Running;
        }
    }
}