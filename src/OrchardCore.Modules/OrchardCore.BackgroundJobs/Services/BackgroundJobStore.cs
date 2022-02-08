using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs.Events;
using OrchardCore.BackgroundJobs.Indexes;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.Modules;
using YesSql;
using YesSql.Services;

namespace OrchardCore.BackgroundJobs.Services
{
    public class BackgroundJobStore : IBackgroundJobStore
    {
        private readonly IBackgroundJobSession _backgroundJobSession;
        private readonly ISession _session;
        private readonly IEnumerable<IBackgroundJobEvent> _backgroundJobEvents;
        private readonly ILogger _logger;

        public BackgroundJobStore(
            IBackgroundJobSession backgroundJobSession,
            ISession session,
            IEnumerable<IBackgroundJobEvent> backgroundJobEvents,
            ILogger<BackgroundJobStore> logger)

        {
            _backgroundJobSession = backgroundJobSession;
            _session = session;
            _backgroundJobEvents = backgroundJobEvents;
            _logger = logger;
        }

        public async ValueTask CreateJobAsync(BackgroundJobExecution backgroundJobExecution)
        {
            var context = new BackgroundJobCreateContext(backgroundJobExecution);

            await _backgroundJobEvents.InvokeValueAsync((handler, context) => handler.CreatingAsync(context), context, _logger);

            _session.Save(backgroundJobExecution);
            await _session.SaveChangesAsync();

            await _backgroundJobEvents.InvokeValueAsync((e) => e.CreatedAsync(context), _logger);
        }

        public async ValueTask<BackgroundJobExecution> GetJobByIdAsync(string backgroundJobId)
        {
            if (_backgroundJobSession.TryRecallBackgroundJobById(backgroundJobId, out var backgroundJobExecution))
            {
                return backgroundJobExecution;
            }


            var document = await _session.Query<BackgroundJobExecution, BackgroundJobIndex>(x => x.BackgroundJobId == backgroundJobId).FirstOrDefaultAsync();
            if (document is not null)
            {
                _backgroundJobSession.Store(document);
            }

            return document;
        }

        public async ValueTask<BackgroundJobExecution> GetJobByCorrelationIdAsync(string correlationId)
        {
            if (_backgroundJobSession.TryRecallBackgroundJobByCorrelationId(correlationId, out var backgroundJobExecution))
            {
                return backgroundJobExecution;
            }


            var document = await _session.Query<BackgroundJobExecution, BackgroundJobIndex>(x => x.CorrelationId == correlationId).FirstOrDefaultAsync();
            if (document is not null)
            {
                _backgroundJobSession.Store(document);
            }

            return document;
        }

        public async ValueTask<IEnumerable<BackgroundJobExecution>> GetJobsByIdAsync(string[] backgroundJobIds)
        {
            List<BackgroundJobExecution> storedItems = null;
            List<BackgroundJobExecution> backgroundJobExecutions = null;
            foreach (var backgroundJobId in backgroundJobIds)
            {
                if (_backgroundJobSession.TryRecallBackgroundJobById(backgroundJobId, out var backgroundJobExecution))
                {
                    storedItems ??= new List<BackgroundJobExecution>();

                    storedItems.Add(backgroundJobExecution);
                }
            }

            var backgroundJobIdsToQuery = storedItems != null
                 ? backgroundJobIds.Except(storedItems.Select(x => x.BackgroundJob.BackgroundJobId), StringComparer.OrdinalIgnoreCase)
                 : backgroundJobIds;

            if (backgroundJobIdsToQuery.Any())
            {
                backgroundJobExecutions = (await
                        _session.Query<BackgroundJobExecution, BackgroundJobIndex>(x => x.BackgroundJobId.IsIn(backgroundJobIdsToQuery))
                        .ListAsync()
                    ).ToList();
            }

            if (backgroundJobExecutions is not null)
            {
                foreach (var backgroundJobExecution in backgroundJobExecutions)
                {
                    _backgroundJobSession.Store(backgroundJobExecution);
                }
                if (storedItems is not null)
                {
                    backgroundJobExecutions.AddRange(storedItems);
                }
            }
            else if (storedItems is not null)
            {
                backgroundJobExecutions = storedItems;
            }
            else
            {
                return Enumerable.Empty<BackgroundJobExecution>();
            }

            return backgroundJobExecutions;
        }

        public async ValueTask UpdateJobAsync(BackgroundJobExecution backgroundJobExecution)
        {
            var context = new BackgroundJobUpdateContext(backgroundJobExecution);

            await _backgroundJobEvents.InvokeValueAsync((handler, context) => handler.UpdatingAsync(context), context, _logger);
            _session.Save(backgroundJobExecution, checkConcurrency: true);
            await _session.SaveChangesAsync();

            await _backgroundJobEvents.InvokeValueAsync((handler, context) => handler.UpdatedAsync(context), context, _logger);
        }

        public async ValueTask<bool> DeleteJobAsync(BackgroundJobExecution backgroundJobExecution)
        {
            if (backgroundJobExecution.State.CurrentStatus == BackgroundJobStatus.Queued || backgroundJobExecution.State.CurrentStatus == BackgroundJobStatus.Executing)
            {
                // Job cancellation is managed by timeouts.
                throw new InvalidOperationException("Cannot delete a job that is queued or executing");
            }

            var context = new BackgroundJobDeleteContext(backgroundJobExecution);

            await _backgroundJobEvents.InvokeValueAsync((handler, context) => handler.DeletingAsync(context), context, _logger);
            if (!context.Cancel)
            {
                // TODO running jobs can't be deleted.
                _session.Delete(backgroundJobExecution);
                //await _session.SaveChangesAsync();
            }

            await _backgroundJobEvents.InvokeValueAsync((handler, context) => handler.DeletedAsync(context), context, _logger);

            return !context.Cancel;
        }
    }
}
