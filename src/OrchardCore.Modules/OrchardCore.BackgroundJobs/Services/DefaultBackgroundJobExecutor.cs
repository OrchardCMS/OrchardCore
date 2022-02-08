using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs.Events;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundJobs.Services
{
    public class DefaultBackgroundJobExecutor : IBackgroundJobExecutor
    {
        private readonly IBackgroundJobHandlerFactory _backgroundJobHandlerFactory;
        private readonly IBackgroundJobStore _backgroundJobStore;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly IEnumerable<IBackgroundJobEvent> _backgroundJobEvents;

        public DefaultBackgroundJobExecutor(
            IBackgroundJobHandlerFactory backgroundJobHandlerFactory,
            IBackgroundJobStore backgroundJobStore,
            IClock clock,
            IEnumerable<IBackgroundJobEvent> backgroundJobEvents,
            ILogger<DefaultBackgroundJobExecutor> logger
            )
        {
            _backgroundJobHandlerFactory = backgroundJobHandlerFactory;
            _backgroundJobStore = backgroundJobStore;
            _clock = clock;
            _backgroundJobEvents = backgroundJobEvents;
            _logger = logger;
        }

        public async ValueTask ExecuteAsync(IBackgroundJob backgroundJob, CancellationToken cancellationToken)
        {
            var name = backgroundJob.Name;
            var backgroundJobHandler = _backgroundJobHandlerFactory.Create(name);
            if (backgroundJobHandler is null)
            {
                _logger.LogWarning("Background Job '{Name}' is not registered correctly.", name);
            }

            var backgroundJobExecution = await _backgroundJobStore.GetJobByIdAsync(backgroundJob.BackgroundJobId);
            if (backgroundJobExecution is null)
            {
                _logger.LogError("Background Job '{Name}', Id '{BackgroundJobId}' could not be found.", name, backgroundJob.BackgroundJobId);

                return;
            }

            try
            {
                if (backgroundJobHandler is null)
                {
                    throw new InvalidOperationException($"Background Job '{backgroundJobExecution.BackgroundJob.Name}' is not registered correctly.");
                }

                var executionContext = new BackgroundJobExecutionContext(backgroundJobExecution);

                await _backgroundJobEvents.InvokeValueAsync((handler, context) => handler.ExecutingAsync(context), executionContext, _logger);

                await backgroundJobHandler.ExecuteAsync(backgroundJob, cancellationToken);

                await _backgroundJobEvents.InvokeValueAsync((handler, context) => handler.ExecutedAsync(context), executionContext, _logger);
            }
            catch (Exception ex)
            {
                var failureContext = new BackgroundJobFailureContext(ex, backgroundJobExecution);

                await _backgroundJobEvents.InvokeValueAsync((handler, context) => handler.FailingAsync(context), failureContext, _logger);
                _logger.LogError(ex, "Error executing background job '{Name}', id '{BackgroundJobId}'", name, backgroundJobExecution.BackgroundJob.BackgroundJobId);
                await _backgroundJobEvents.InvokeValueAsync((handler, context) => handler.FailedAsync(context), failureContext, _logger);
            }
        }
    }
}
