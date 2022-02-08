using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Locking.Distributed;
using OrchardCore.Settings;

namespace OrchardCore.Modules
{
    public class ModularBackgroundJobService : BackgroundService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly IBackgroundJobQueue _taskQueue;
        private readonly ILogger _logger;

        public ModularBackgroundJobService(
            IHttpContextAccessor httpContextAccessor,
            IBackgroundJobQueue taskQueue,
            IShellHost shellHost,
            ILogger<ModularBackgroundJobService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _taskQueue = taskQueue;
            _shellHost = shellHost;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
            {
                _logger.LogInformation("'{ServiceName}' is stopping.", nameof(ModularBackgroundJobService));
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                var partitionCount = System.Environment.ProcessorCount;
                var collection = new ConcurrentQueue<BackgroundJobQueueItem>();
                var semaphore = new SemaphoreSlim(0);
                var cts = new CancellationTokenSource(); // to signal that queueing is completed
                var tasks = Enumerable.Range(0, partitionCount - 1)
                    .Select(i =>
                        Task.Run(async () =>
                        {
                            while (true)
                            {
                                // TODO bump in the other cancel token. NB. This will never be hit, it's an endless queue.
                                if (cts.Token.IsCancellationRequested && !collection.Any())
                                {
                                    _logger.LogDebug("No more background items to take");
                                    break;
                                }
                                else if (!cts.Token.IsCancellationRequested)
                                {
                                    try
                                    {
                                        await semaphore.WaitAsync(cts.Token);
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        //ignore
                                    }
                                }

                                // TODO default http context
                                // TODO cancellation (timeout)

                                if (collection.TryDequeue(out var backgroundJobQueueItem))
                                {
                                    if (!_shellHost.TryGetShellContext(backgroundJobQueueItem.ShellName, out var shellContext) ||
                                       shellContext.Settings.State != TenantState.Running ||
                                       shellContext.Pipeline == null
                                       )
                                    {
                                        // TODO The job should timeout.
                                        _logger.LogWarning("Shell '{ShellName}' is not running, job '{Name}' discarded)", backgroundJobQueueItem.ShellName, backgroundJobQueueItem.BackgroundJob.Name);
                                        continue;
                                    }

                                    _httpContextAccessor.HttpContext = shellContext.CreateHttpContext();

                                    // Allow the queue state to be set and persisted.
                                    await backgroundJobQueueItem.Ready;

                                    var shellScope = await _shellHost.GetScopeAsync(shellContext.Settings);

                                    await shellScope.UsingAsync(async (scope) =>
                                    {
                                        try
                                        {
                                            var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>(); ;
                                            (var locker, var locked) = await distributedLock.TryAcquireBackgroundJobLockAsync(backgroundJobQueueItem.BackgroundJob.BackgroundJobId);
                                            if (!locked)
                                            {
                                                _logger.LogDebug("Could not aquire scheduling lock on Background job '{Name}', id '{BackgroundJobId}'", backgroundJobQueueItem.BackgroundJob.Name, backgroundJobQueueItem.BackgroundJob.BackgroundJobId);
                                                throw new Exception($"Could not aquire execution lock on Background job {backgroundJobQueueItem.BackgroundJob.BackgroundJobId}");
                                            }


                                            await using (var acquiredLock = locker)
                                            {
                                                var siteService = scope.ServiceProvider.GetService<ISiteService>();
                                                if (siteService != null)
                                                {
                                                    try
                                                    {
                                                        _httpContextAccessor.HttpContext.SetBaseUrl((await siteService.GetSiteSettingsAsync()).BaseUrl);
                                                    }
                                                    catch (Exception ex) when (!ex.IsFatal())
                                                    {
                                                        _logger.LogError(ex, "Error while getting the base url from the site settings of the tenant '{TenantName}'.", shellContext.Settings.Name);
                                                    }
                                                }

                                                var executor = scope.ServiceProvider.GetRequiredService<IBackgroundJobExecutor>();
                                                await executor.ExecuteAsync(backgroundJobQueueItem.BackgroundJob, stoppingToken);
                                            }
                                        }
                                        catch (Exception ex) when (!ex.IsFatal())
                                        {
                                            // TODO try reschedule, if could not aquire lock? or drop into timeout.

                                            _logger.LogError(ex, "Error executing background job '{Name}'", backgroundJobQueueItem.BackgroundJob.Name);

                                            // TODO event handlers...
                                            //context.Exception = ex;

                                            await scope.HandleExceptionAsync(ex);
                                        }
                                    });
                                }
                            }
                        }))
                    .ToArray();

                await foreach (var backgroundJobQueueItem in _taskQueue.DequeueAllAsync(stoppingToken))
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Dequeued item '{Name}' Id '{BackgroundJobId}'", backgroundJobQueueItem.BackgroundJob.Name, backgroundJobQueueItem.BackgroundJob.BackgroundJobId);
                    }
                    collection.Enqueue(backgroundJobQueueItem);
                    semaphore.Release();
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Background job collection complete");
                }

                cts.Cancel(); // addition completed.
                await Task.WhenAll(tasks);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Background jobs threads complete");
                }
            }
        }
    }
}
